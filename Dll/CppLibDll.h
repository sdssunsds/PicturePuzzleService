#define CPPLIBDLL_EXPORTS
#ifdef CPPLIBDLL_EXPORTS
#define CPPLIBDLL_API __declspec(dllexport)
#else
#define CPPLIBDLL_API __declspec(dllimport)
#endif
#pragma  once
#include <string>
struct box_inf
{
	int x;
	int y;
	int w;
	int h;
};
struct inputrect_4
{
	int imgNO;//车厢号
	std::string location_str;//区域编号
	std::string part_location_str;//部件位置编号
	std::string part_str;//部件编号
	std::string only_str;//唯一编号
	box_inf rect;//方框坐标
};
struct config_info
{
	std::string img_path;//读图路径
	std::string train_type;//车型
	std::string time;//时间
	std::string save_path;//存储路径
};
// 导出的接口类
class IExport
{
public:
	// 初始化
	virtual int init(std::string engine_path)=0;

	// 返回值:失败或成功指示
	virtual int run_procedure(const config_info input_info, const inputrect_4* cut_info, 
		const int cut_info_length, const int* length, const int length_num,int& progress)=0;
	virtual ~IExport() {}
};

extern "C" CPPLIBDLL_API IExport* __stdcall ExportObjectFactory();
extern "C" CPPLIBDLL_API void __stdcall DestroyExportObject(IExport* obj);

extern "C" CPPLIBDLL_API int __stdcall CallOnInit(IExport* obj, const char* engine_path);
extern "C" CPPLIBDLL_API int __stdcall Callcutimg(IExport* obj,const config_info input_info, const inputrect_4* cut_info, 
	int cut_info_length, const int* length, const int length_num, int& progress);
