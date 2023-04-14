#pragma once
#ifndef _YOLOV5_H
#define _YOLOV5_H
#include "CppLibDll.h"
#include <iostream>
#include "yololayer.h"
#include<algorithm>
#include <thread>
#include <io.h>
#include <direct.h>
#include <map>
#include<mutex>
#include "height_config.h"
#include "preprocess.h"
#include "write.h"
#include "../cuda_utils.h"
#include "../logging.h"
#include "utils.h"
//#include "calibrator.h"
#define USE_FP16  // set USE_INT8 or USE_FP16 or USE_FP32
#define DEVICE 0  // GPU id
#define NMS_THRESH 0.4
#define NMS_THRESH2 0.2
#define CONF_THRESH 0.8
#define BATCH_SIZE 1
const int RE_W = 2048;
const int IN_H = 640;
const int RE_ratio = RE_W / 2048;
using namespace nvinfer1;
static const int OUTPUT_SIZE = Yolo::MAX_OUTPUT_BBOX_COUNT * sizeof(Yolo::Detection) / sizeof(float) + 1;
static std::vector <std::string > model_class{ "ct", "mjcg","pzq","cz"};
static std::map <std::string, int> class_num{ {"ct",1}, {"mjcg",1},{"pzq",1},{"cz",4} };
struct yolodetect
{
	char* trtModelStream;
	ICudaEngine* engine;
	IRuntime* runtime;
	IExecutionContext* context;
	cudaStream_t stream;
	void* buffers[2];
	int inputIndex;
	int outputIndex;
	float prob[BATCH_SIZE * OUTPUT_SIZE];
	uint8_t* img_host;
	uint8_t* img_device;

};
struct obj_st
{
	int rect_no;
	Yolo::Detection obj;
};
struct object_box
{
	std::string classname;
	float conf;
	float class_ID;
	cv::Rect rect;
};
struct part_info
{
	std::string classname;
	int base;
	int real;
};
class cutimg :public IExport
{
private:
	int kk = 0;
	std::string ranname;
	std::mutex lock;
	yolodetect yolodetection;
	std::string back_str;
	std::map<std::string, std::string> pathmap;
	float* data;
	float* prob;
	int count_img_t = 0;//拼接大图片数
	int count_img_s = 0;//小图片数
	int count_img_r = 0;//拼接好的rect图片数
	int IMG_W = 4096;
	int IMG_H = 2260;
	float ratio =  float(IN_H) /float(RE_W);
	cv::Mat rect_img;
	cv::Mat keep_img;
	std::string standard;
	std::vector<int> locvec;//记录模板图片长度
	cv::Mat totalMat_singel;//single是最终输出的
	cv::Mat totalMat_s;//totalMat_s是拼接的
	const std::vector<int> standart_h = {37700,35800};
	std::vector<object_box> train_detect;
public:
	cutimg();
	~cutimg();
	virtual int init(std::string engine_path) override;
	virtual int run_procedure(const config_info input_info, const inputrect_4* cut_info,
		const int cut_info_length, const int* length, const int length_num, int progress) override;
	std::string log;
private:
	void cut_resize_img(cv::Mat& input_img, std::vector<object_box> detect_box,
		int cut_info_length,  const config_info input_info, const inputrect_4* cut_info);
	void handle_img();
	int get_img();
	int checkandsleep(cv::Mat& input, int& sleeptimes);
	void doInference(IExecutionContext& context, cudaStream_t& stream, void** buffers, float* output, int batchSize);
};
#endif 