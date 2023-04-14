#include "axis_check.h"
void zero_fill(cv::Mat input)
{
	std::vector<int> h{ -1,0,1 };
	std::vector<int> l{ -1,0,1 };
	//列
	for (int i=0;i< input.cols;i++)
	{
		//行
		for (int j = 0; j < input.rows; j++)
		{
			int num = 0;
			int total = 0;
			for (auto hz: h)
			{
				for (auto lz : l)
				{
					if ((i + lz >= 0 && i + lz < input.cols)&& (j + hz >= 0 && j + hz < input.rows))
					{
						if (input.at<ushort>(j + hz, i + lz)!=0)
						{
							num++;
							total += input.at<ushort>(j + hz, i + lz);
						}
						
					}
				}
			}
			if (input.at<ushort>(j, i)==0&& num>=5)
			{
				input.at<ushort>(j, i) = ushort(total / num);
			}
			
		}
	}
}
cv::Point axis_check::drawLine(cv::Mat& image, double theta, double rho, cv::Scalar color,cv::Rect box)
{
	cv::Point res;
	res.y = camera_cy;
	if (theta < PI / 4. || theta > 3. * PI / 4.)// ~vertical line
	{
		cv::Point pt1((rho - box.y * sin(theta))/ cos(theta), box.y);
		cv::Point pt2((rho - (box.y+ box.height) * sin(theta)) / cos(theta), box.y + box.height);
		cv::line(image, pt1, pt2, cv::Scalar(255), 1);
		res.x = (rho - camera_cy * sin(theta)) / cos(theta);
	}
	else
	{
		cv::Point pt1(box.x, rho / sin(theta));
		cv::Point pt2(box.x+ box.width, (rho - (box.x + box.width) * cos(theta)) / sin(theta));
		cv::line(image, pt1, pt2, color, 1);
		res.x= (rho - camera_cy * sin(theta)) / cos(theta);
	}
	cv::circle(image, res, 2, cv::Scalar(0,  255,0), -1);
	return res;
}
cv::Point3d axis_check::trans(ushort d,int x,int y)
{
	cv::Point3d p;
	p.z = double(d) / camera_factor;
	p.x = (x - camera_cx) * p.z / camera_fx;
	p.y = (y - camera_cy) * p.z / camera_fy;
	return p;
}
int axis_check::init(std::string engine_path)
{
	trt_basic_config axis_config = { "v5",0.5,0.3 ,640,640,"axis_s_640.engine",{"axis","CTK","YK"}};
	if (!engine_path.empty())
	{
		axis_config.engine_name = engine_path;
	}
	int state=axis_infer.init(axis_config);
	if (state==-1)
	{
		init_state = -1;
		return -1;
	}
	std::cout << "初始化成功" << std::endl;
	return 0;
}
float axis_check::get_dis(std::string bmp_path, std::string range_png_path,int state=0)
{
	
	if (init_state==-1)
	{
		std::cout << "未找到模型，请检查！！！" << std::endl;
		return -2000;
	}
	std::vector<cv::Point3d> axis_ptv;
	cv::Mat img = cv::imread(bmp_path);
	std::cout << "png名称为:" << range_png_path << std::endl;
	cv::Mat rang_img = cv::imread(range_png_path,-1);
	std::cout << "png名称为:" << range_png_path << std::endl;
	if (img.empty()|| rang_img.empty())
	{
		std::cout << "未读取到相关图像，请检查！！！" << std::endl;
		if (img.empty())
		{
			std::cout << "bmp名称为:"<< bmp_path << std::endl;
		}
		if (rang_img.empty())
		{
			std::cout << "png名称为:" << range_png_path << std::endl;
		}
		
	}
	zero_fill(rang_img);
	std::cout << "png名称为:" << range_png_path << std::endl;
	double max, min;
	cv::minMaxLoc(rang_img, &min, &max);
	double scale = 255.0 / (max - min);

	std::vector<inf_res> detect_res=axis_infer.do_infer(img);
	if (detect_res.size() == 0)
	{
		std::cout << "未检测到目标。" << std::endl;
		return -10000;
	}

	if (state==0)
	{
		inf_res axis;
		int target_num = 0;
		for (auto s : detect_res)
		{
			if (s.box_name=="axis")
			{
				if (target_num==0)
				{
					axis = s;
				}
				else
				{
					if (axis.conf<s.conf)
					{
						axis = s;
					}
				}
				target_num++;
			}
		}
		if (target_num!=0)
		{
			cv::Mat img_axis, rang_imgcopy;
			rang_img(axis.box).copyTo(rang_imgcopy);
			//img(res[0].box).copyTo(img_axis);
			cv::Mat rang_trans, rang_trans2;

			cv::minMaxLoc(rang_imgcopy, &min, &max);
			rang_imgcopy.convertTo(rang_trans, CV_8UC1, scale);
			cv::minMaxLoc(rang_imgcopy, &min, &max);
			scale = 255.0 / (max - min);
			cv::Mat new_rang_imgcopy = rang_imgcopy - min;
			new_rang_imgcopy.convertTo(rang_trans2, CV_8UC1, scale);
			cv::cvtColor(rang_trans2, rang_trans2, cv::COLOR_GRAY2BGR);
			cv::Rect box =axis.box;
			std::vector<std::vector<cv::Point>> contours;
			std::vector<cv::Vec4i> hierarchy;
			std::cout << "找轮廓" << std::endl;
			findContours(rang_trans, contours, hierarchy, cv::RETR_EXTERNAL, cv::CHAIN_APPROX_SIMPLE, cv::Point());
			std::cout << "找完" << std::endl;
			if (contours.size() > 0)
			{
				//计算最大面积的轮廓
				int id = 0, max_area = 0;
				for (int i = 0; i < contours.size(); i++)
				{
					double area = cv::contourArea(contours[i]);
					if (area > max_area)
					{
						id = i;
						max_area = area;
					}
				}

				std::cout << "生成mask" << std::endl;
				cv::Mat mask = cv::Mat::zeros(rang_trans.size(), CV_8UC1);
				cv::drawContours(mask, contours, id, 255, -1);
				std::cout << "绘制mask" << std::endl;


				std::vector<cv::Point3d> base_line;
				std::vector<cv::Point> img_line;
				for (int m = box.y; m < box.y + box.height; m++)
				{

					//计算横向最小z值及其坐标
					cv::Point3d base_point(0, 0, 1000000);
					cv::Point img_point(-1, -1);
					for (int n = box.x; n < box.x + box.width; n++)
					{
						ushort d;
						if (mask.ptr<uchar>(m - box.y)[n - box.x] == 0)
						{
							d = 0;
						}
						else
						{
							d = rang_img.ptr<ushort>(m)[n];
						}

						if (d == 0)
						{
							continue;
						}
						else
						{
							cv::Point3d p = trans(d, n, m);
							if (p.z < base_point.z)
							{
								base_point = p;
								img_point = cv::Point(n, m);
							}
						}

					}
					if (base_point.z != 1000000 && img_point.x != -1)
					{
						base_line.push_back(base_point);
						img_line.push_back(img_point);
						cv::circle(img, img_point, 1, cv::Scalar(0, 0, 255), -1);
					}

				}
				std::cout << "绘制轴线" << std::endl;
				cv::Vec4f line;
				cv::fitLine(img_line,
					line,
					CV_DIST_HUBER,
					0,
					0.01,
					0.01);
				double cos_theta = line[0];
				double sin_theta = line[1];
				double x0 = line[2], y0 = line[3];

				double phi = atan2(sin_theta, cos_theta) + PI / 2.0;
				double rho = y0 * cos_theta - x0 * sin_theta;
				cv::Point center = drawLine(img, phi, rho, cv::Scalar(0, 0, 255), box);
				cv::imwrite(bmp_path.substr(0, bmp_path.size() - 4) + "_res.jpg", img);
				std::cout << "绘制点" << std::endl;
				cv::Point3d res = trans(rang_img.ptr<ushort>(center.y)[center.x], center.x, center.y);
				return res.x;
			}
		
		}
		else
		{

			return -10000;
		}
	}
	else if (state==1)
	{
		inf_res box,kd;
		int target_num_box = 0, target_num_kd=0;
		for (auto s : detect_res)
		{
			if (s.box_name == "CTK")
			{
				if (target_num_box == 0)
				{
					box = s;
				}
				else
				{
					if (box.conf < s.conf)
					{
						box = s;
					}
				}
				target_num_box++;
			}
			else if (s.box_name == "YK")
			{
				if (target_num_kd == 0)
				{
					kd = s;
				}
				else
				{
					if (kd.conf < s.conf)
					{
						kd = s;
					}
				}
				target_num_kd++;
			}
		}
		if (target_num_box>0&& target_num_kd>0)
		{
			if ((box.box&kd.box).area()>0)
			{
				cv::Point center_point{ kd.box.x+ kd.box.width/2, kd.box.y + kd.box.height / 2 };
				cv::Rect expend_area;
				expend_area.x = std::max(kd.box.x - 5, 0);
				expend_area.y = std::max(kd.box.y - 5, 0);
				expend_area.width = std::min(kd.box.x+ kd.box.width + 5, rang_img.cols-1)- expend_area.x;
				expend_area.height = std::min(kd.box.y + kd.box.height + 5, rang_img.rows-1) - expend_area.y;
				cv::Rect effect_box{ kd.box.x -expend_area.x ,  kd.box.y-expend_area.y, kd.box.width, kd.box.height};
				cv::Mat effect_area = rang_img(expend_area);
				double total=0;
				int num = 0;
				for (int i=0;i< effect_area.cols;i++)
				{
					for (int j = 0;  j< effect_area.rows; j++)
					{
						bool in = effect_box.contains(cv::Point(i, j));
						ushort d= effect_area.ptr<ushort>(j)[i];
						if ((!in)&& d != 0)
						{
							num++;
							total += d;
						}
					}
				}
				double average = total / num;
				cv::Point3d res = trans(average, center_point.x, center_point.y);
				return res.x;
			}
			else
			{

				return -10000;
			}
		}
		else
		{

			return -10000;
		}
	}

} 