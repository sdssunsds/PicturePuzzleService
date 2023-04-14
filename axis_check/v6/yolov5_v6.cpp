#include "yolov5_v6.h"
#include <fstream>
#define USE_FP16  // set USE_INT8 or USE_FP16 or USE_FP32
#define DEVICE 0  // GPU id

#define MAX_IMAGE_INPUT_SIZE_THRESH 5000 * 5000 // ensure it exceed the maximum size in the input images !

const char* INPUT_BLOB_NAME_v6 = "data";
const char* OUTPUT_BLOB_NAME_v6 = "prob";
static Logger gLogger;
using namespace nvinfer1;
cv::Rect yolov5_v6_inf::get_rect_v6(int w,int h, float bbox[4]) {
    float l, r, t, b;
    float r_w = v6_config.INPUT_W / (w * 1.0);
    float r_h = v6_config.INPUT_H / (h * 1.0);
    if (r_h > r_w) {
        l = bbox[0] - bbox[2] / 2.f;
        r = bbox[0] + bbox[2] / 2.f;
        t = bbox[1] - bbox[3] / 2.f - (v6_config.INPUT_H - r_w * h) / 2;
        b = bbox[1] + bbox[3] / 2.f - (v6_config.INPUT_H - r_w * h) / 2;
        l = l / r_w;
        r = r / r_w;
        t = t / r_w;
        b = b / r_w;
    }
    else {
        l = bbox[0] - bbox[2] / 2.f - (v6_config.INPUT_W - r_h * w) / 2;
        r = bbox[0] + bbox[2] / 2.f - (v6_config.INPUT_W - r_h * w) / 2;
        t = bbox[1] - bbox[3] / 2.f;
        b = bbox[1] + bbox[3] / 2.f;
        l = l / r_h;
        r = r / r_h;
        t = t / r_h;
        b = b / r_h;
    }
    return cv::Rect(round(l), round(t), round(r - l), round(b - t));
}

float iou_v6(float lbox[4], float rbox[4]) {
    float interBox[] = {
        (std::max)(lbox[0] - lbox[2] / 2.f , rbox[0] - rbox[2] / 2.f), //left
        (std::min)(lbox[0] + lbox[2] / 2.f , rbox[0] + rbox[2] / 2.f), //right
        (std::max)(lbox[1] - lbox[3] / 2.f , rbox[1] - rbox[3] / 2.f), //top
        (std::min)(lbox[1] + lbox[3] / 2.f , rbox[1] + rbox[3] / 2.f), //bottom
    };

    if (interBox[2] > interBox[3] || interBox[0] > interBox[1])
        return 0.0f;

    float interBoxS = (interBox[1] - interBox[0]) * (interBox[3] - interBox[2]);
    return interBoxS / (lbox[2] * lbox[3] + rbox[2] * rbox[3] - interBoxS);
}

bool cmp_v6(const Yolo_v6::Detection& a, const Yolo_v6::Detection& b) {
    return a.conf > b.conf;
}

void nms(std::vector<Yolo_v6::Detection>& res, float* output, float conf_thresh, float nms_thresh = 0.5) {
    int det_size = sizeof(Yolo_v6::Detection) / sizeof(float);
    std::map<float, std::vector<Yolo_v6::Detection>> m;
    for (int i = 0; i < output[0] && i < Yolo_v6::MAX_OUTPUT_BBOX_COUNT; i++) {
        if (output[1 + det_size * i + 4] <= conf_thresh) continue;
        Yolo_v6::Detection det;
        memcpy(&det, &output[1 + det_size * i], det_size * sizeof(float));
        if (m.count(det.class_id) == 0) m.emplace(det.class_id, std::vector<Yolo_v6::Detection>());
        m[det.class_id].push_back(det);
    }
    for (auto it = m.begin(); it != m.end(); it++) {
        //std::cout << it->second[0].class_id << " --- " << std::endl;
        auto& dets = it->second;
        std::sort(dets.begin(), dets.end(), cmp_v6);
        for (size_t m = 0; m < dets.size(); ++m) {
            auto& item = dets[m];
            res.push_back(item);
            for (size_t n = m + 1; n < dets.size(); ++n) {
                if (iou_v6(item.bbox, dets[n].bbox) > nms_thresh) {
                    dets.erase(dets.begin() + n);
                    --n;
                }
            }
        }
    }
}

static int get_width_v6(int x, float gw, int divisor = 8) {
    return int(ceil((x * gw) / divisor)) * divisor;
}

static int get_depth_v6(int x, float gd) {
    if (x == 1) return 1;
    int r = round(x * gd);
    if (x * gd - int(x * gd) == 0.5 && (int(x * gd) % 2) == 0) {
        --r;
    }
    return std::max<int>(r, 1);
}
yolov5_v6_inf::yolov5_v6_inf()
{
}

void yolov5_v6_inf::doInference_v6(IExecutionContext& context, cudaStream_t& stream, void **buffers, float* output, int batchSize) {
    // infer on the batch asynchronously, and DMA output back to host
    context.enqueue(batchSize, buffers, stream, nullptr);
    CUDA_CHECK(cudaMemcpyAsync(output, buffers[1], batchSize * OUTPUT_SIZE * sizeof(float), cudaMemcpyDeviceToHost, stream));
    cudaStreamSynchronize(stream);
}
int yolov5_v6_inf::init(trt_basic_config input_config)
{
    if (v6_dec.engine != nullptr)
    {
        std::cout << "模型" << input_config.engine_name << "已初始化，请勿重复调用！！" << std::endl;
        return 0;
    }
    else
    {
        cudaSetDevice(DEVICE);
        v6_config = input_config;
        std::ifstream file(input_config.engine_name, std::ios::binary);
        if (!file.good()) {
            std::cerr << "读取 " << input_config.engine_name << " 错误!" << std::endl;
            return -1;
        }
        v6_dec.trtModelStream = nullptr;
        size_t size = 0;
        file.seekg(0, file.end);
        size = file.tellg();
        file.seekg(0, file.beg);
        v6_dec.trtModelStream = new char[size];
        assert(v6_dec.trtModelStream);
        file.read(v6_dec.trtModelStream, size);
        file.close();
        //v6_dec.prob=new float[BATCH_SIZE * OUTPUT_SIZE_v6];
        v6_dec.runtime = createInferRuntime(gLogger);
        assert(v6_dec.runtime != nullptr);
        v6_dec.engine = v6_dec.runtime->deserializeCudaEngine(v6_dec.trtModelStream, size);
        assert(v6_dec.engine != nullptr);
        v6_dec.context = v6_dec.engine->createExecutionContext();
        assert(v6_dec.context != nullptr);
        delete[] v6_dec.trtModelStream;
        assert(v6_dec.engine->getNbBindings() == 2);
        // In order to bind the buffers, we need to know the names of the input and output tensors.
        // Note that indices are guaranteed to be less than IEngine::getNbBindings()
        auto name = v6_dec.engine->getBindingName(0);
        v6_dec.inputIndex = v6_dec.engine->getBindingIndex(INPUT_BLOB_NAME_v6);
        v6_dec.outputIndex = v6_dec.engine->getBindingIndex(OUTPUT_BLOB_NAME_v6);
        assert(v6_dec.inputIndex == 0);
        assert(v6_dec.outputIndex == 1);
        // Create GPU buffers on device
        CUDA_CHECK(cudaMalloc((void**)&v6_dec.buffers[v6_dec.inputIndex], BATCH_SIZE * 3 * v6_config.INPUT_H * v6_config.INPUT_W * sizeof(float)));
        cudaMalloc((void**)&v6_dec.buffers[v6_dec.outputIndex], BATCH_SIZE * OUTPUT_SIZE * sizeof(float));
        // Create stream
        //cudaStream_t stream;
        CUDA_CHECK(cudaStreamCreate(&v6_dec.stream));
        // prepare input data cache in pinned memory 
        CUDA_CHECK(cudaMallocHost(&v6_dec.img_host, MAX_IMAGE_INPUT_SIZE_THRESH * 3));
        // prepare input data cache in device memory
        CUDA_CHECK(cudaMalloc(&v6_dec.img_device, MAX_IMAGE_INPUT_SIZE_THRESH * 3));
        return 0;
    }
  
}
float* blobFromImage(cv::Mat& img) {
    cv::cvtColor(img, img, cv::COLOR_BGR2RGB);

    float* blob = new float[img.total() * 3];
    int channels = 3;
    int img_h = img.rows;
    int img_w = img.cols;
    for (size_t c = 0; c < channels; c++)
    {
        for (size_t h = 0; h < img_h; h++)
        {
            for (size_t w = 0; w < img_w; w++)
            {
                blob[c * img_w * img_h + h * img_w + w] =
                    (((float)img.at<cv::Vec3b>(h, w)[c]) / 255.0f);
            }
        }
    }
    return blob;
}
cv::Mat static_resize(cv::Mat& img) {

    float r = std::min(640 / (img.cols * 1.0), 640 / (img.rows * 1.0));
    int unpad_w = r * img.cols;
    int unpad_h = r * img.rows;
    cv::Mat re(unpad_h, unpad_w, CV_8UC3);
    cv::resize(img, re, re.size());
    cv::Mat out(640, 640, CV_8UC3, cv::Scalar(114, 114, 114));
    re.copyTo(out(cv::Rect(0, 0, re.cols, re.rows)));
    return out;
}
std::vector<inf_res> yolov5_v6_inf::do_infer(cv::Mat input)
{
    
    float scale=1;
    cv::Mat inputcopy;
    
    if (input.rows>3000|| input.cols > 3000)
    {
        if (input.rows >= input.cols)
        {
            scale =   3000.0/input.rows;
            cv::resize(input, inputcopy, cv::Size(input.cols * scale, input.rows * scale));
        }
        else
        {
            scale = 3000.0 / input.rows;
            cv::resize(input, inputcopy, cv::Size(input.cols * scale, input.rows * scale));
        }
    }
    else
    {
        input.copyTo(inputcopy);
    }


    int img_w = inputcopy.cols;
    int img_h = inputcopy.rows;
    //auto start = std::chrono::system_clock::now();
    float* buffer_idx = (float*)v6_dec.buffers[v6_dec.inputIndex];
    size_t  size_image = inputcopy.cols * inputcopy.rows * 3;
    //copy data to pinned memory
    memcpy(v6_dec.img_host, inputcopy.data, size_image);
    //copy data to device memory
    CUDA_CHECK(cudaMemcpyAsync(v6_dec.img_device, v6_dec.img_host, size_image, cudaMemcpyHostToDevice, v6_dec.stream));
    preprocess_kernel_img_v6(v6_dec.img_device, inputcopy.cols, inputcopy.rows, buffer_idx, v6_config.INPUT_W, v6_config.INPUT_H, v6_dec.stream);
    // Run inference
    CUDA_CHECK(cudaMemcpyAsync(v6_dec.img_host, v6_dec.img_device, size_image, cudaMemcpyDeviceToHost));
    doInference_v6(*v6_dec.context, v6_dec.stream, (void**)v6_dec.buffers, v6_dec.prob, BATCH_SIZE);
    std::vector<Yolo_v6::Detection> batch_res;
    nms(batch_res, &v6_dec.prob[0], v6_config.CONF_THRESH, v6_config.NMS_THRESH);
    std::vector<inf_res> res;
    if (batch_res.size()>0)
    {
        for (size_t j = 0; j < batch_res.size(); j++) {
            inf_res res_info_s;
            int x= int(get_rect_v6(img_w, img_h, batch_res[j].bbox).x / scale);
            int y= int(get_rect_v6(img_w, img_h, batch_res[j].bbox).y / scale);
            int w= int(get_rect_v6(img_w, img_h, batch_res[j].bbox).width / scale);
            int h= int(get_rect_v6(img_w, img_h, batch_res[j].bbox).height / scale);
            res_info_s.box.x = x >= 0 ? x : 0;
            res_info_s.box.y = y >= 0 ? y : 0;
            w = w >= 0 ? w : 0;
            h = h >= 0 ? h : 0;
            if (w>0&&h>0)
            {
                res_info_s.box.width = w + x <= input.cols ? w : input.cols - x;
                res_info_s.box.height = h + y <= input.rows ? h : input.rows - y;
                res_info_s.box_name = v6_config.classname[(int)batch_res[j].class_id];
                res_info_s.conf = batch_res[j].conf;
                cv::rectangle(inputcopy, get_rect_v6(img_w, img_h, batch_res[j].bbox), cv::Scalar(255, 0, 0), 1);
                //cv::putText(inputcopy, v6_config.classname[(int)batch_res[j].class_id], cv::Point(res_info_s.box.x, res_info_s.box.y - 1), cv::FONT_HERSHEY_PLAIN, 1.2, cv::Scalar(0xFF, 0xFF, 0xFF), 1);
                res.push_back(res_info_s);
            }
            
            
        }
    }
    return res;
}
yolov5_v6_inf::~yolov5_v6_inf()
{

    cudaStreamDestroy(v6_dec.stream);
    CUDA_CHECK(cudaFree(v6_dec.img_device));
    CUDA_CHECK(cudaFreeHost(v6_dec.img_host));
    CUDA_CHECK(cudaFree(v6_dec.buffers[v6_dec.inputIndex]));
    CUDA_CHECK(cudaFree(v6_dec.buffers[v6_dec.outputIndex]));
    // Destroy the engine
   //delete[] v6_dec.prob;
    v6_dec.context->destroy();
    v6_dec.engine->destroy();
    v6_dec.runtime->destroy();
}