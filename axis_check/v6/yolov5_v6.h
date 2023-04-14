#pragma once
#ifndef _YOLOV5_V6_H
#define _YOLOV5_V6_H
#include <iostream>
#include <chrono>
#include <cmath>
#include <string>
#include "../cuda_utils.h"
#include "../logging.h"
#include "../../basic_struct.h"
#include "NvInfer.h"
#include "utils_v6.h"
#include "calibrator_v6.h"
#include "preprocess_v6.h"
#include "yololayer_v6.h"
#define BATCH_SIZE 1
using namespace nvinfer1;
static const int OUTPUT_SIZE = Yolo_v6::MAX_OUTPUT_BBOX_COUNT * sizeof(Yolo_v6::Detection) / sizeof(float) + 1;

struct yolodetect
{
	char* trtModelStream;
	ICudaEngine* engine;
	IRuntime* runtime;
	IExecutionContext* context;
	cudaStream_t stream;
	float* buffers[2];
	int inputIndex;
	int outputIndex;
	float prob[BATCH_SIZE* OUTPUT_SIZE];
	float* output;
	uint8_t* img_host;
	uint8_t* img_device;
};
class yolov5_v6_inf
{
public:
	trt_basic_config v6_config;
private:
	yolodetect v6_dec;
public:
	yolov5_v6_inf();
	~yolov5_v6_inf();
	int init(trt_basic_config input_config);//≥ı ºªØ
	std::vector<inf_res> do_infer(cv::Mat input);
private:
	void doInference_v6(IExecutionContext& context, cudaStream_t& stream, void** buffers, float* output, int batchSize);
	cv::Rect get_rect_v6(int w, int h , float bbox[4]);
};
#endif