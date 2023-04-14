#include "yolov5.h"

#define DEVICE 0  // GPU id
#define writeing 1
#define NMS_THRESH 0.4
#define CONF_THRESH 0.7
#define BATCH_SIZE 1
#define MAX_IMAGE_INPUT_SIZE_THRESH 5000 * 5000 // ensure it exceed the maximum size in the input images !
// stuff we know about the network and the input/output blobs
static const int INPUT_H = Yolo::INPUT_H;
static const int INPUT_W = Yolo::INPUT_W;
//static const int CLASS_NUM = Yolo::CLASS_NUM;
//float ratio = 1;
const char* INPUT_BLOB_NAME = "data";
const char* OUTPUT_BLOB_NAME = "prob";
static Logger gLogger;

bool exists(const std::string& name)
{
    std::ifstream f(name.c_str());
    return f.good();
}
object_box box_trans(Yolo::Detection input,int h,int rew)
{
    float trans = float(rew) / float(INPUT_H);
    object_box res;
    res.class_ID = int(input.class_id);
    res.classname = model_class[int(input.class_id)];
    res.conf = input.conf;
    res.rect.x = (input.bbox[0] - input.bbox[2]/ 2.f)* trans;
    res.rect.y = (input.bbox[1] - input.bbox[3]/ 2.f) * trans +h;
    res.rect.width = input.bbox[2] * trans;
    res.rect.height = input.bbox[3] * trans;
    return res;
}


float iou_v4(float lbox[4], float rbox[4]) {
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
bool cmp(const Yolo::Detection& a, const Yolo::Detection& b) {
    return a.conf > b.conf;
}
void nms(std::vector<Yolo::Detection>& res, float* output, float conf_thresh, float nms_thresh = 0.5) {
    int det_size = sizeof(Yolo::Detection) / sizeof(float);
    std::map<float, std::vector<Yolo::Detection>> m;
    for (int i = 0; i < output[0] && i < Yolo::MAX_OUTPUT_BBOX_COUNT; i++) {
        if (output[1 + det_size * i + 4] <= conf_thresh) continue;
        Yolo::Detection det;
        memcpy(&det, &output[1 + det_size * i], det_size * sizeof(float));
        if (m.count(det.class_id) == 0) m.emplace(det.class_id, std::vector<Yolo::Detection>());
        m[det.class_id].push_back(det);
    }
    for (auto it = m.begin(); it != m.end(); it++) {
        //std::cout << it->second[0].class_id << " --- " << std::endl;
        auto& dets = it->second;
        std::sort(dets.begin(), dets.end(), cmp);
        for (size_t m = 0; m < dets.size(); ++m) {
            auto& item = dets[m];
            res.push_back(item);
            for (size_t n = m + 1; n < dets.size(); ++n) {
                if (iou_v4(item.bbox, dets[n].bbox) > nms_thresh) {
                    dets.erase(dets.begin() + n);
                    --n;
                }
            }
        }
    }
}
cutimg::cutimg()
{

}
cutimg::~cutimg()
{
    delete[]data;
    delete[]prob;
    cudaStreamDestroy(yolodetection.stream);
    CUDA_CHECK(cudaFree(yolodetection.buffers[yolodetection.inputIndex]));
    CUDA_CHECK(cudaFree(yolodetection.buffers[yolodetection.outputIndex]));


    // Destroy the engine
    yolodetection.context->destroy();
    yolodetection.engine->destroy();
    yolodetection.runtime->destroy();
}
int cutimg::init(std::string engine_path)
{
    std::cout << "正在初始化......" << std::endl;
    data = new float[BATCH_SIZE * 3 * INPUT_H * INPUT_W];
    prob = new float[BATCH_SIZE * OUTPUT_SIZE];
    yolodetection.trtModelStream = nullptr; engine_path;
    std::ifstream file(engine_path, std::ios::binary);
    if (!file.good()) {
        std::cerr << "read " << engine_path << " error!" << std::endl;
        return -1;
    }
    size_t size = 0;
    file.seekg(0, file.end);
    size = file.tellg();
    file.seekg(0, file.beg);
    yolodetection.trtModelStream = new char[size];
    assert(yolodetection.trtModelStream);
    file.read(yolodetection.trtModelStream, size);
    file.close();
    yolodetection.runtime = createInferRuntime(gLogger);
    assert(yolodetection.runtime != nullptr);
    yolodetection.engine = yolodetection.runtime->deserializeCudaEngine(yolodetection.trtModelStream, size);
    assert(yolodetection.engine != nullptr);
    yolodetection.context = yolodetection.engine->createExecutionContext();
    assert(yolodetection.context != nullptr);
    delete[] yolodetection.trtModelStream;
    assert(yolodetection.engine->getNbBindings() == 2);
    // In order to bind the buffers, we need to know the names of the input and output tensors.
    // Note that indices are guaranteed to be less than IEngine::getNbBindings()


    yolodetection.inputIndex = yolodetection.engine->getBindingIndex(INPUT_BLOB_NAME);
    yolodetection.outputIndex = yolodetection.engine->getBindingIndex(OUTPUT_BLOB_NAME);
    assert(yolodetection.inputIndex == 0);
    assert(yolodetection.outputIndex == 1);
    // Create GPU buffers on device
    CUDA_CHECK(cudaMalloc(&yolodetection.buffers[yolodetection.inputIndex], BATCH_SIZE * 3 * INPUT_H * INPUT_W * sizeof(float)));
    CUDA_CHECK(cudaMalloc(&yolodetection.buffers[yolodetection.outputIndex], BATCH_SIZE * OUTPUT_SIZE * sizeof(float)));
    // Create stream
    //cudaStream_t stream;
    CUDA_CHECK(cudaStreamCreate(&yolodetection.stream));
    CUDA_CHECK(cudaMallocHost(&yolodetection.img_host, MAX_IMAGE_INPUT_SIZE_THRESH * 3));
    // prepare input data cache in device memory
    CUDA_CHECK(cudaMalloc(&yolodetection.img_device, MAX_IMAGE_INPUT_SIZE_THRESH * 3));

    std::cout << "初始化完成。" << std::endl;
    return 0;
}
void cutimg::doInference(IExecutionContext& context, cudaStream_t& stream, void** buffers, float* output, int batchSize) {
    context.enqueue(batchSize, buffers, stream, nullptr);
    CUDA_CHECK(cudaMemcpyAsync(output, buffers[1], batchSize * OUTPUT_SIZE * sizeof(float), cudaMemcpyDeviceToHost, stream));
    cudaStreamSynchronize(stream);
}
int cutimg::checkandsleep(cv::Mat& input, int& sleeptimes)
{
    if (input.empty())
    {
        Sleep(1000);
        sleeptimes++;
    }
    else
    {
        sleeptimes = 0;
    }
    if (sleeptimes == 0)
    {
        return 0;
    }
    else if (sleeptimes >=3)
    {
        return -1;
    }
    else
    {
        return 1;
    }
}
int cutimg::get_img()
{
    int stay = 0;
    rect_img.release();
    keep_img.copyTo(rect_img);
    cv::Mat concimg;
    cv::Mat readimg;
    
    if (keep_img.empty())
    {

        if (count_img_s == 0)
        {
            back_str = ".jpg";
            std::string imgpath = standard.substr(0, standard.size() - std::to_string(count_img_s).size()) + std::to_string(count_img_s) + back_str;
            std::cout << "读取图片号码：" << count_img_s << std::endl;
        read1:readimg = cv::imread(imgpath,0);
            if (readimg.empty())
            {
                back_str = ".bmp";
                imgpath = standard.substr(0, standard.size() - std::to_string(count_img_s).size()) + std::to_string(count_img_s) + back_str;
                readimg = cv::imread(imgpath, 0);
            }

            if (count_img_s == 0)
            {
                IMG_W = readimg.cols;
                IMG_H = readimg.rows;
                ratio = float(RE_W) / float(IMG_W);
            }
            int tsat = checkandsleep(readimg, stay);
            if (tsat == -1)
            {
                std::cout << "超时，停止拼接切图任务" << std::endl;
                return -1;
            }
            else if (tsat == 1)
            {
                std::cout << "等待图像传输:" << imgpath << std::endl;
                goto read1;
            }
        }
        else
        {
            count_img_s++;
            std::string imgpath = standard.substr(0, standard.size() - std::to_string(count_img_s).size()) + std::to_string(count_img_s) + ".jpg";
        read2: readimg = cv::imread(imgpath, 0);
            std::cout << "读取图片号码：" << count_img_s << std::endl;
            int tsat = checkandsleep(readimg, stay);
            if (tsat == -1)
            {
                std::cout << "超时，停止拼接切图任务" << std::endl;
                return -1;
            }
            else if (tsat == 1)
            {
                std::cout << "等待图像传输:" << imgpath << std::endl;
                goto read2;
            }
        }
        cv::Mat resize_img;
        assert(!readimg.empty());
        cv::resize(readimg, resize_img,cv::Size(), ratio,  ratio);
        resize_img.copyTo(concimg);
    }
    else
    {
        if (keep_img.rows < RE_W)
        {
            count_img_s++;
        read3:readimg = cv::imread(standard.substr(0, standard.size() - std::to_string(count_img_s).size()) + std::to_string(count_img_s) + ".jpg", 0);
            std::cout << "读取图片号码：" << count_img_s << std::endl;
            int tsat = checkandsleep(readimg, stay);
            if (tsat == -1)
            {
                std::cout << "超时，停止拼接切图任务" << std::endl;
                return -1;
            }
            else if (tsat == 1)
            {
                std::cout << "等待图像传输" << std::endl;
                goto read3;
            }
            cv::Mat resize_img;
            cv::resize(readimg, resize_img, cv::Size(), ratio, ratio);
            cv::vconcat(keep_img, resize_img, concimg);
        }
        else
        {
            keep_img.copyTo(concimg);
        }

    }

    conch:int h = concimg.rows;

    if (RE_W > h)
    {
        count_img_s++;
    read4:readimg = cv::imread(standard.substr(0, standard.size() - std::to_string(count_img_s).size()) + std::to_string(count_img_s) + ".jpg", 0);
        std::cout << "读取图片号码：" << count_img_s << std::endl;
        int tsat = checkandsleep(readimg, stay);
        if (tsat == -1)
        {
            std::cout << "超时，停止拼接切图任务" << std::endl;
            return -1;
        }
        else if (tsat == 1)
        {
            std::cout << "等待图像传输" << std::endl;
            goto read4;
        }
        cv::Mat resize_img;
        cv::resize(readimg, resize_img, cv::Size(), ratio,ratio);
        cv::vconcat(concimg, resize_img, concimg);
        goto conch;
    }
    else
    {

        if (RE_W == h)
        {
            keep_img.release();
            concimg.copyTo(rect_img);
            concimg(cv::Range(RE_W / 2, h), cv::Range::all()).copyTo(keep_img);
        }
        else
        {
            keep_img.release();
            concimg(cv::Range(0, RE_W), cv::Range::all()).copyTo(rect_img);
            concimg(cv::Range(RE_W / 2, h), cv::Range::all()).copyTo(keep_img);
        }
        concimg.release();
    }
    return 0;
}
float IOU(object_box& A, object_box& B)
{
    // 左上右下坐标(x1,y1,x2,y2)
    float big = (A.rect | B.rect).area();
    float small = (A.rect & B.rect).area();
    if (small==0)
    {
        return 0.0;
    }
    else
    {
        return small/ big;
    }
}

void cut_img(cv::Mat& img,int cut_info_length,int progress, const config_info input_info, const inputrect_4* cut_info)
{
    cv::Mat write_img;
    if (writeing)
    {
        write_new(img, write_img);
    }
    else
    {
        write_img = img;
    }
    int count_img_t = progress + 1;
    std::cout << "正在切割第" << std::to_string(count_img_t) << "张图片请等待......" << std::endl;
    std::cout << "共需切割：" << cut_info_length << "张。" << std::endl;
    for (int cut = 0; cut < cut_info_length; cut++)
    {
        if (cut_info[cut].imgNO == count_img_t)
        {
            if (_access((std::string(input_info.save_path) + "\\" + cut_info[cut].part_str).c_str(), 0) != 0)
            {

                int rere = _mkdir((std::string(input_info.save_path) + "\\" + std::string(cut_info[cut].part_str)).c_str());
            }

            std::string save_img_name = std::string(input_info.save_path) + "\\" + std::string(cut_info[cut].part_str) + "\\" + std::string(input_info.train_type) + "_000" + std::to_string(cut_info[cut].imgNO) + "_" +
                std::string(cut_info[cut].location_str) + "_" + std::string(cut_info[cut].part_location_str) + "_" + std::string(cut_info[cut].part_str) + "_" +
                std::string(cut_info[cut].only_str) + "_" + std::string(input_info.time) + ".jpg";
            if (cut_info[cut].y* RE_ratio + cut_info[cut].h * RE_ratio <= img.rows && cut_info[cut].x * RE_ratio + cut_info[cut].w * RE_ratio <= img.cols)
            {
                inputrect_4 single_struct;
                char full[200];
                char full50[200];
                memset(full, '\0', 200 * sizeof(char));
                memset(full50, '\0', 50 * sizeof(char));
                single_struct.imgNO = cut_info[cut].imgNO;
                bool save_state = cv::imwrite(save_img_name, write_img(cv::Rect(cut_info[cut].x * RE_ratio, cut_info[cut].y * RE_ratio, cut_info[cut].w * RE_ratio, cut_info[cut].h * RE_ratio)));
                if (!save_state)
                {
                    std::cout << save_img_name<<std::endl;
                    std::cout << "存储失败！请检查存储名称！" << std::endl;
                }
            }
            else
            {
                std::cout << "坐标错误，请复查模板坐标！模板唯一编号为：" << cut_info[cut].only_str << std::endl;
                std::cout << "最大纵向坐标y+h=" << cut_info[cut].y * RE_ratio + cut_info[cut].h * RE_ratio << "," << "图像最大h=" << img.rows << std::endl;
                std::cout << "最大横向坐标x+w=" << cut_info[cut].x * RE_ratio + cut_info[cut].w * RE_ratio << "," << "图像最大w=" << img.cols << std::endl;
            }
        }

    }
    std::cout << "切割完成。" << std::endl;
    if (_access((std::string(input_info.save_path) + "\\" + "common_"+ std::to_string(count_img_t)).c_str(), 0) != 0)
    {

        int rere = _mkdir((std::string(input_info.save_path) + "\\"  +"common_" + std::to_string(count_img_t)).c_str());
    }
    std::cout << "完成拼图：" << std::string(input_info.save_path) + "\\" + "common_" + std::to_string(count_img_t) + "\\" + std::string(input_info.train_type) + "_" + std::to_string(count_img_t) + ".jpg" << std::endl;
    bool store_state= cv::imwrite(std::string(input_info.save_path) + "\\" + "common_" + std::to_string(count_img_t) + "\\" + std::string(input_info.train_type) + "_" + "common" + "_" + std::to_string(count_img_t) + ".jpg", write_img);
    store_state = cv::imwrite(std::string(input_info.save_path) + "\\" + std::string(input_info.train_type) + "_" + "common" + "_" + std::to_string(count_img_t) + ".jpg", write_img);

}
bool sort_score(object_box& box1, object_box& box2)
{
    return (box1.conf > box2.conf);
}
bool sort_height(object_box& box1, object_box& box2)
{
    return (box1.rect.y < box2.rect.y);
}
std::vector<object_box> nms_fuc(std::vector<object_box> vec)
{
    std::vector<object_box> res;
    std::vector<std::vector<object_box>> store_box(model_class.size());
    for (auto s:vec)
    {
        store_box[s.class_ID].push_back(s);
    }
    for (auto s: store_box)
    {
        if (s.size()==0)
        {
            continue;
        }
        else if (s.size() == 1)
        {
            res.push_back(s[0]);
        }
        else
        {
            std::vector<bool> pass_box(s.size(),false);
            std::sort(s.begin(), s.end(), sort_score);
            std::vector<object_box> res_s;
            for (size_t i = 0; i < s.size(); i++)
            {
                if (pass_box[i])
                {
                    continue;
                }
                else
                {
                    object_box x = s[i];
                    x.rect.height = x.rect.height + 1000;
                    for (size_t j = i+1; j < s.size(); j++)
                    {
                        if (pass_box[j])
                        {
                            continue;
                        }
                        else
                        {
                            object_box xy = s[j];
                            xy.rect.height = s[j].rect.height + 1000;
                            float iou_res = IOU(x, xy);
                            if (iou_res > 0)
                            {
                                x.rect = x.rect | s[j].rect;
                                pass_box[j] = true;
                            }
                        }
                    }
                    res_s.push_back(x);
                }
               
            }
            res.insert(res.end(), res_s.begin(), res_s.end());
        }

    }
    std::sort(res.begin(), res.end(), sort_height);
    return res;
}
void cutimg::cut_resize_img(cv::Mat& input_img,  std::vector<object_box> detect_box,
    int cut_info_length, const config_info input_info, const inputrect_4* cut_info)
{
    int real_end_h = 0;
    std::vector<part_h> height_map;
    std::vector<int> hv;
    std::vector<std::string> partname;
    if (detect_box[0].classname=="ct"&& detect_box[1].classname == "pzq")
    {
        height_map = height_map1;
        hv = height1;
        partname = part1;
    }
    else
    {
        height_map = height_map3;
        hv = height3;
        partname = part3;
    }
    //std::vector<object_box> nms_box = nms_fuc(detect_box);
    std::map<std::string,std::vector<object_box>> handel_box;
    std::map<std::string,std::vector<bool>> h_pass;
    for (auto s : class_num)
    {
        std::vector<object_box> class_box;
        for (auto ss: detect_box)
        {
            if (ss.classname==s.first)
            {
                class_box.push_back(ss);
            }
        }
        if (class_box.size()<=s.second)
        {
            handel_box[s.first] = class_box;
        }
        else
        {
            if (s.second==1)
            {
                std::sort(class_box.begin(), class_box.end(), sort_score);
                std::vector<object_box> res_class(class_box.begin(), class_box.begin() + s.second);
                handel_box[s.first] = res_class;
            }
            else
            {
                std::sort(class_box.begin(), class_box.end(), sort_score);
                std::vector<object_box> res_class(class_box.begin(), class_box.begin() + s.second + 1);
                std::sort(res_class.begin(), res_class.end(), sort_height);
                handel_box[s.first] = res_class;
            }

        }
    }
    for (auto s : handel_box)
    {
        h_pass[s.first] = std::vector<bool>(s.second.size(), false);
    }
    std::vector<part_info> get_pair;
    int empty_head = 0;
    get_pair.push_back({"ct", 0, 0});
    
    for (int i=0;i< height_map.size();i++)
    {
        if (handel_box.find(height_map[i].class_name) != handel_box.end())
        {
            std::vector<object_box> s_class = handel_box[height_map[i].class_name];

            if (int(s_class.size())!= class_num[height_map[i].class_name])
            {
                int dis_min = 1000000;
                int id = 0;
                for (int j = 0; j < s_class.size(); j++)
                {
                    int dis = abs((height_map[i].h - get_pair[get_pair.size() - 1].base) - (s_class[j].rect.y - get_pair[get_pair.size() - 1].real));
                    if (dis < dis_min)
                    {
                        dis_min = dis;
                        id = j;
                    }
                }
                if (std::abs(dis_min)>1000)
                {
                    continue;
                }
                if (h_pass[height_map[i].class_name][id])
                {
                    int ll = 0;
                    for (int ii = 0; ii < get_pair.size(); ii++)
                    {
                        if (get_pair[ii].real == s_class[id].rect.y)
                        {
                            ll = ii;
                        }
                        get_pair.erase(get_pair.begin() + ll);
                    }
                }
                else
                {
                    h_pass[height_map[i].class_name][id] = true;
                }
                if (s_class[id].rect.y - get_pair[get_pair.size() - 1].real > 0)
                {
                    get_pair.push_back({ height_map[i].part_name, height_map[i].h, s_class[id].rect.y });
                }
                
            }
            //选出最合适的那个
            else
            {
                if (height_map[i].part_name == "ct")
                {
                    get_pair[0].real = s_class[0].rect.y;
                    empty_head = s_class[0].rect.y;
                    
                }
                else if (height_map[i].part_name == "mjcg")
                {
                    real_end_h = s_class[0].rect.y;
                    get_pair.push_back({ height_map[i].part_name, height_map[i].h, s_class[0].rect.y });
                }
                else
                {
                    for (int s=0;s< s_class.size();s++)
                    {
                        if (!h_pass[height_map[i].class_name][s]&& s_class[s].rect.y - get_pair[get_pair.size()-1].real>0)
                        {
                            get_pair.push_back({ height_map[i].part_name, height_map[i].h, s_class[s].rect.y });
                            h_pass[height_map[i].class_name][s] = true;
                            break;
                        }
                    }
                }
            }
           
        }

    }

    std::vector<cv::Mat> concate_img_vec; 
    for (int i = 1; i < get_pair.size(); i++)
    {
        int h_s= get_pair[i].base - get_pair[i - 1].base;
        cv::Rect cut_area;
        cut_area.x = 0;
        cut_area.width = RE_W;
        cut_area.y = get_pair[i - 1].real;
        cut_area.height = get_pair[i].real - get_pair[i-1].real;
        cv::Mat cutimg;
        input_img(cut_area).copyTo(cutimg);
        if (cutimg.empty())
        {
            std::cout << "wrong!" << std::endl;
        }
        assert(!cutimg.empty());

        cv::resize(cutimg, cutimg, cv::Size(RE_W, h_s));
        concate_img_vec.push_back(cutimg);
    }
    cv::Mat concate;
    cv::vconcat(concate_img_vec, concate);
    cv::Mat cut_release;
    totalMat_s(cv::Rect{ 0,real_end_h,totalMat_s.cols,totalMat_s.rows - real_end_h }).copyTo(cut_release);
    totalMat_s.release();
    cut_release.copyTo(totalMat_s);
    cv::Mat write_img;

    write_new(concate, write_img);
    std::cout << "正在切割第" << std::to_string(count_img_t) << "张图片请等待......" << std::endl;
    std::cout << "共需切割：" << cut_info_length << "张。" << std::endl;
    for (int i=1;i<hv.size();i++)
    {
        cv::Rect box;
        box.x = 0;
        box.y = hv[i - 1];
        box.width = concate.cols;
        box.height= hv[i]-hv[i - 1];
        std::string part_name_local = partname[i - 1].substr(partname[i - 1].size() - 4, partname[i - 1].size());
        std::string part_name_nm = partname[i - 1].substr(0, 3);
        if (_access((std::string(input_info.save_path) +  part_name_local).c_str(), 0) != 0)
        {

            int rere = _mkdir((std::string(input_info.save_path) + "\\" + part_name_local).c_str());
        }

        std::string save_img_name = std::string(input_info.save_path) +  part_name_local + "\\" + std::string(input_info.train_type)+"_000"+std::to_string(count_img_t) + "_" + part_name_nm + "_M03"  + "_"+ part_name_local +
            "_6000" + std::to_string(count_img_t) + partname[i - 1] +"010010000" + "_" + std::string(input_info.time) + ".jpg";
        std::cout <<"正在存储：" <<save_img_name << std::endl;
        cv::imwrite(save_img_name, write_img(box));
    }
    if (_access((std::string(input_info.save_path) +  "common_" + std::to_string(count_img_t)).c_str(), 0) != 0)
    {

        int rere = _mkdir((std::string(input_info.save_path)  + "common_" + std::to_string(count_img_t)).c_str());
        cv::imwrite(std::string(input_info.save_path) +  "common_" + std::to_string(count_img_t)+ "\\" + std::string(input_info.train_type) + "_common_" + std::to_string(count_img_t) + ".jpg", write_img);
    }
    cv::imwrite(std::string(input_info.save_path) +  std::string(input_info.train_type) + "_common_" + std::to_string(count_img_t) + ".jpg", write_img);
    concate.release();
}


void cutimg::handle_img()
{
    cv::Mat re;

    if (totalMat_s.empty())
    {
        rect_img.copyTo(totalMat_s);       
    }
    else
    {
        totalMat_s.push_back(rect_img(cv::Range(RE_W / 2, RE_W), cv::Range::all()));
    }
    float* buffer_idx = (float*)yolodetection.buffers[yolodetection.inputIndex];
    size_t  size_image = rect_img.cols * rect_img.rows * 3;
    //copy data to pinned memory
    cv::Mat bgr_gray;
    cv::cvtColor(rect_img, bgr_gray, cv::COLOR_GRAY2BGR);
    memcpy(yolodetection.img_host, bgr_gray.data, size_image);
    //copy data to device memory
    CUDA_CHECK(cudaMemcpyAsync(yolodetection.img_device, yolodetection.img_host, size_image, cudaMemcpyHostToDevice, yolodetection.stream));
    preprocess_kernel_img(yolodetection.img_device, rect_img.cols, rect_img.rows, buffer_idx, INPUT_W, INPUT_H, yolodetection.stream);
    // Run inference
    CUDA_CHECK(cudaMemcpyAsync(yolodetection.img_host, yolodetection.img_device, size_image, cudaMemcpyDeviceToHost));
}
int cutimg::run_procedure(const config_info input_info, const inputrect_4* cut_info, const int cut_info_length, const int* height, const int height_len, int progress)
{
    bool cutimg = true;
    std::cout << "img_path:" << input_info.img_path << std::endl;
    std::cout << "save_path:" << input_info.save_path << std::endl;
    std::cout << "time:" << input_info.time << std::endl;
    std::cout << "train_type:" << input_info.train_type << std::endl;
    standard = std::string(input_info.img_path) + "/" + "10000";
    std::cout << "获取的图像路径为：" << standard << std::endl;
    if (progress<0)
    {
        progress = 0;
    }
    else if (progress>16)
    {
        std::cout << "输入车厢号参数有误请重新输入！" << std::endl;
        return -1;
    }
    cudaSetDevice(DEVICE);
    std::cout << "开始进行拼接切图任务" << std::endl;
    bool train_top = false;
    int top_h = 0;
    bool train_end = false;
    bool check_end = true;
    while (1)
    {
        
        if (get_img() == 0)
        {
            handle_img();
            cv::Mat t1 = totalMat_singel;
            cv::Mat t2 = totalMat_s;
            cv::Mat t3 = rect_img;
            doInference(*yolodetection.context, yolodetection.stream, (void**)yolodetection.buffers,
                prob, BATCH_SIZE);
            std::vector<Yolo::Detection> batch_res;
            nms(batch_res, prob, CONF_THRESH, NMS_THRESH);
            
            if (batch_res.size() != 0)
            {
                check_end = false;
                for (auto yoloinfo : batch_res)
                {
                    int h =totalMat_s.rows-RE_W;

                    object_box box = box_trans(yoloinfo,h, RE_W);
                    
                    if (box.classname=="mjcg"|| box.classname == "ct")
                    {
                        if (box.classname == "ct")
                        {
                            train_top = 1;
                            top_h = box.rect.y;
                        }
                        else if(box.classname == "mjcg")
                        {
                            if (train_top&& top_h+5000< box.rect.y)
                            {
                               
                                if (train_end ==0)
                                {
                                    std::cout << "密接车钩检测完毕" << std::endl;
                                    train_end = 1;
                                    //end_h = box.rect.y;
                                }                               
                            }
                            else
                            {
                                train_detect.clear();
                                box.classname = "ct";
                                train_top = 1;
                                top_h = box.rect.y;
                            }
                        }
                    }
                    train_detect.push_back(box);
                }
               
            }
            else
            {
                check_end = true;
            }
            //符合条件开始拼切图
            if (train_top&& train_end&& check_end)
            {
                count_img_t++;
                train_detect = nms_fuc(train_detect);
                sort(train_detect.begin(), train_detect.end(), sort_height);
                if (train_detect[0].classname!="ct")
                {
                    object_box ct_base = { "ct",0,0,{0,0,0,0}};
                    train_detect.emplace(train_detect.begin(), ct_base);
                }
                std::vector<object_box> now_stage, new_stage;
                int train_detect_num = train_detect.size();
                int mjcg_id = 0;
                for (int i=0;i< train_detect_num;i++)
                {
                    int trans = train_detect_num - i - 1;
                    if (train_detect[trans].classname == "mjcg")
                    {
                        mjcg_id = trans;
                        break;
                    }
                }
                for (int i = 0; i < train_detect_num; i++)
                {
                    if (i<= mjcg_id)
                    {
                        now_stage.push_back(train_detect[i]);
                    }
                    else
                    {
                        new_stage.push_back(train_detect[i]);
                    }
                }
                cut_resize_img(totalMat_s, now_stage, cut_info_length,input_info, cut_info);
                train_end = false;
                top_h = 0;
                train_detect.clear();
                train_detect.insert(train_detect.end(), new_stage.begin(), new_stage.end());
                lock.lock();
                lock.unlock();
            }            
        }
        else
        {

            if (train_top && train_end)
            {
                count_img_t++;
                train_detect = nms_fuc(train_detect);
                sort(train_detect.begin(), train_detect.end(), sort_height);
                if (train_detect[0].classname != "ct")
                {
                    object_box ct_base = { "ct",0,0,{0,0,0,0} };
                    train_detect.emplace(train_detect.begin(), ct_base);
                }
                std::vector<object_box> now_stage, new_stage;
                int train_detect_num = train_detect.size();
                int mjcg_id = 0;
                for (int i = 0; i < train_detect_num; i++)
                {
                    int trans = train_detect_num - i - 1;
                    if (train_detect[trans].classname == "mjcg")
                    {
                        mjcg_id = trans;
                        break;
                    }
                }
                for (int i = 0; i < train_detect_num; i++)
                {
                    if (i <= mjcg_id)
                    {
                        now_stage.push_back(train_detect[i]);
                    }
                    else
                    {
                        new_stage.push_back(train_detect[i]);
                    }
                }
                //std::thread thire(cut_resize_img, std::ref(totalMat_s), now_stage, cut_info_length, progress, input_info, cut_info);
                cv::Mat test_img = totalMat_s;
                cut_resize_img(totalMat_s, now_stage, cut_info_length, input_info, cut_info);
                train_end = false;
                top_h = 0;
                //end_h = 0;
                train_detect.clear();
                train_detect.insert(train_detect.end(), new_stage.begin(), new_stage.end());
                std::cout << "图片等待超时！" << std::endl;
                log = "图片等待超时！";
            }
            train_end = 0;
            train_top = 0;
            train_detect.clear();
            break;
        }
        //释放空间
    }

    standard = "";
    rect_img.release();
    keep_img.release();
    count_img_t = 0;//拼接大图片数
    count_img_s = 0;//小图片数
    count_img_r = 0;//拼接好的rect图片数
    totalMat_singel.release();
    totalMat_s.release();
    std::cout << "任务完成，拼切完毕。" << std::endl;
    return 0;
}