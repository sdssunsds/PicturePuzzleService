#pragma once
#include <string>
#include "opencv2/opencv.hpp"
#include <vector>
#include <map>
struct pt
{
	int x;
	int y;
	friend bool operator < (const struct pt& k1, const struct pt& k2);
};
struct trt_basic_config
{
	std::string modeltype;
	float NMS_THRESH;
	float CONF_THRESH;
	int INPUT_H;
	int INPUT_W;
	std::string engine_name;
	std::vector<std::string> classname;
};
struct inf_res
{
	cv::Rect box;
	std::string box_name;
	float conf;
};
struct infer_class_config
{
	int num_classes;
	int input_batch;
	int input_channel;
	int input_width;
	int input_height;	
	std::vector<float> meanf;
	std::vector<float> stdf;
	std::string engine_name;
	std::vector<std::string> classname;
};
struct infer_class_info
{
	float conf;
	std::string class_name;
};
inline bool operator < (const struct pt& a, const struct pt& b)
{
	return (a.x < b.x) || (a.x == b.x && a.y < b.y);
}

struct box_info
{
	char name[50];//boxµÄÃû×ÖÈçÂÝË¿
	int state;//boxµÄ×´Ì¬
	int x;
	int y;
	int w;
	int h;
};
struct box_info_str
{
	std::string name;//boxµÄÃû×ÖÈçÂÝË¿
	int state;//boxµÄ×´Ì¬
	int x;
	int y;
	int w;
	int h;
};
struct input_task
{
	int task_list[10];
	int imgNO;//³µÏáºÅ
	char location_str[50];//ÇøÓò±àºÅ
	char part_location_str[50];//²¿¼þÎ»ÖÃ±àºÅ
	char part_str[50];//²¿¼þ±àºÅ
	char only_str[50];//Î¨Ò»±àºÅ
	int x;
	int y;
	int w;
	int h;
};
struct input_struct
{
	int task_list[10];//ÈÎÎñÁÐ±í
	int imgNO;//ÁÐ³µ±àºÅ
	char location_str[50];//boxµÄ×´Ì¬
	char part_location_str[50];//²¿¼þÎ»ÖÃ±àºÅ
	char part_str[50];//²¿¼þ±àºÅ
	char only_str[50];//Î¨Ò»±àÂë
	char img_path[200];//Í¼ÏñÂ·¾¶
};
struct model_struct
{
	char class_name[50];//ÖÖÀàÃû³Æ
	int x;
	int y;
	int w;
	int h;
};
enum state_enum
{
	//ÎÞ·¨¼ì²â
	UnSafecheck = -1,
	//Õý³£
	Normal = 0,
	//Òì³££¨Í¨ÓÃ£©
	Abnormity = 1,
	//ÂÝË¿¶ªÊ§
	Screw_missing = 2,
	//ÂÝË¿ËÉ¶¯
	Screw_loose = 3,
	//ÌúË¿¶ÏÁÑ
	Broken_wire = 4,
	//»®ºÛ
	Scratch_marks = 5,
	//ÒìÎï
	Foreign_body =6,
	//ÓÍÎ»Òì³£
	Abnormal_oil_level =7,
	//ÓÍÒº»ë×Ç
	Oil_turbidity =8,
	//ÓÍÎÛ
	YW=9,
	//²¿¼þ¶ªÊ§
	BJDS=10
};
enum task_enum
{
	//¼ì²éÂÝË¿£¬°üº¬ÂÝË¿¶ªÊ§
	check_screw = 0,
	//¼ì²éÒìÎï
	check_Foreign_body = 1,
	//¼ì²éÌúË¿¶ÏÁÑ
	check_locking_wire = 2,
	//¼ì²éÓÍÎ»±í
	check_oil_level = 3,
	//»®ºÛ
	check_scratch = 4,
	//ÈöÉ³¹Ü
	sashazui_check=5,
	//Ò×¶ªÊ§²¿¼þ¼ì²â£¬»¬¿é½ºÄàµÈ
	getloss_check=6,
	//µ×°åÂÝË¿ËÉ¶¯¼ì²â
	screw_loose_=7
};