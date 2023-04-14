#pragma once
#ifndef YOLOV5_COMMON_H_
#define YOLOV5_COMMON_H_

#include <fstream>
#include <map>
#include <sstream>
#include <vector>
#include <opencv2/opencv.hpp>
#include "NvInfer.h"
#include "yololayer_v6.h"
using namespace nvinfer1;
//cv::Rect get_rect_v6(cv::Mat& img, float bbox[4]);
//float iou_v6(float lbox[4], float rbox[4]);
//bool cmp_v6(const Yolo_v6::Detection& a, const Yolo_v6::Detection& b);
//void nms_v6(std::vector<Yolo_v6::Detection>& res, float* output, float conf_thresh, float nms_thresh);
std::map<std::string, Weights> loadWeights_v6(const std::string file);
IScaleLayer* addBatchNorm2d_v6(INetworkDefinition* network, std::map<std::string, Weights>& weightMap, ITensor& input, std::string lname, float eps);
ILayer* convBlock_v6(INetworkDefinition* network, std::map<std::string, Weights>& weightMap, ITensor& input, int outch, int ksize, int s, int g, std::string lname);
ILayer* focus_v6(INetworkDefinition* network, std::map<std::string, Weights>& weightMap, ITensor& input, int inch, int outch, int ksize, std::string lname);
ILayer* bottleneck_v6(INetworkDefinition* network, std::map<std::string, Weights>& weightMap, ITensor& input, int c1, int c2, bool shortcut, int g, float e, std::string lname);
ILayer* bottleneckCSP_v6(INetworkDefinition* network, std::map<std::string, Weights>& weightMap, ITensor& input, int c1, int c2, int n, bool shortcut, int g, float e, std::string lname);
ILayer* C3_v6(INetworkDefinition* network, std::map<std::string, Weights>& weightMap, ITensor& input, int c1, int c2, int n, bool shortcut, int g, float e, std::string lname);
ILayer* SPP_v6(INetworkDefinition* network, std::map<std::string, Weights>& weightMap, ITensor& input, int c1, int c2, int k1, int k2, int k3, std::string lname);
ILayer* SPPF_v6(INetworkDefinition* network, std::map<std::string, Weights>& weightMap, ITensor& input, int c1, int c2, int k, std::string lname);
std::vector<std::vector<float>> getAnchors_v6(std::map<std::string, Weights>& weightMap, std::string lname);
IPluginV2Layer* addYoLoLayer_v6(INetworkDefinition* network, std::map<std::string, Weights>& weightMap, std::string lname, std::vector<IConvolutionLayer*> dets);
#endif
