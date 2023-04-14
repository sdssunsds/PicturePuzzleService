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
	char name[50];//box����������˿
	int state;//box��״̬
	int x;
	int y;
	int w;
	int h;
};
struct box_info_str
{
	std::string name;//box����������˿
	int state;//box��״̬
	int x;
	int y;
	int w;
	int h;
};
struct input_task
{
	int task_list[10];
	int imgNO;//�����
	char location_str[50];//������
	char part_location_str[50];//����λ�ñ��
	char part_str[50];//�������
	char only_str[50];//Ψһ���
	int x;
	int y;
	int w;
	int h;
};
struct input_struct
{
	int task_list[10];//�����б�
	int imgNO;//�г����
	char location_str[50];//box��״̬
	char part_location_str[50];//����λ�ñ��
	char part_str[50];//�������
	char only_str[50];//Ψһ����
	char img_path[200];//ͼ��·��
};
struct model_struct
{
	char class_name[50];//��������
	int x;
	int y;
	int w;
	int h;
};
enum state_enum
{
	//�޷����
	UnSafecheck = -1,
	//����
	Normal = 0,
	//�쳣��ͨ�ã�
	Abnormity = 1,
	//��˿��ʧ
	Screw_missing = 2,
	//��˿�ɶ�
	Screw_loose = 3,
	//��˿����
	Broken_wire = 4,
	//����
	Scratch_marks = 5,
	//����
	Foreign_body =6,
	//��λ�쳣
	Abnormal_oil_level =7,
	//��Һ����
	Oil_turbidity =8,
	//����
	YW=9,
	//������ʧ
	BJDS=10
};
enum task_enum
{
	//�����˿��������˿��ʧ
	check_screw = 0,
	//�������
	check_Foreign_body = 1,
	//�����˿����
	check_locking_wire = 2,
	//�����λ��
	check_oil_level = 3,
	//����
	check_scratch = 4,
	//��ɳ��
	sashazui_check=5,
	//�׶�ʧ������⣬���齺���
	getloss_check=6,
	//�װ���˿�ɶ����
	screw_loose_=7
};