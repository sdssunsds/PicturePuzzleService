#pragma once
#define CPPLIBDLL_EXPORTS
#ifdef CPPLIBDLL_EXPORTS
#define CPPLIBDLL_API __declspec(dllexport)
#else
#define CPPLIBDLL_API __declspec(dllimport)
#endif
#pragma  once
#include<string>
#include<vector>
struct inputrect_4
{
	//int task_list[10];
	int imgNO;//车厢号
	char location_str[50];//区域编号
	char part_location_str[50];//部件位置编号
	char part_str[50];//部件编号
	char only_str[50];//唯一编号
	int x;
	int y;
	int w;
	int h;
};

struct config_info
{
	char img_path[200] = { '\0' };//读图路径
	char train_type[50] = { '\0' };//车型
	char time[50] = { '\0' };//时间
	char save_path[200] = { '\0' };//存储路径
};
// 导出的接口类
class IExport
{
public:
	// 初始化
	virtual int init(std::string engine_path) = 0;
	// 返回值:失败或成功指示
	virtual  int run_procedure(const config_info input_info, const inputrect_4* cut_info,
		const int cut_info_length, const int* length, const int length_num, int progress) = 0;
};
extern "C" CPPLIBDLL_API IExport * __stdcall ExportObjectFactory();
extern "C" CPPLIBDLL_API void __stdcall DestroyExportObject(IExport * obj);

extern "C" CPPLIBDLL_API int __stdcall CallOnInit(IExport * obj, const char* engine_path);//engine文件路径
extern "C" CPPLIBDLL_API int __stdcall Callcutimg(IExport * obj, const config_info input_info,
	const inputrect_4 * cut_info, const int cut_info_len, const int* height, const int height_len, int progress);