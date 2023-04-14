#pragma once
#include "axis.h"
#include <iostream>
#include "opencv.hpp"
#include "v6/yolov5_v6.h"
const float PI = 3.1415926;


class axis_check :public location
{
public:
	const double camera_factor = 20;
	const double camera_cx = 6.0578506e+02;
	const double camera_cy = 4.9607508e+02;
	const double camera_fx = 1.1858181e+03;
	const double camera_fy = 1.1860013e+03;
	int   init(std::string engine_path);
	float get_dis(std::string bmp_path, std::string range_png_path,int state);
private:
	int init_state = 0;
	cv::Point drawLine(cv::Mat& image, double theta, double rho, cv::Scalar color, cv::Rect box);
	cv::Point3d trans(ushort d, int x, int y);
	yolov5_v6_inf axis_infer;
};