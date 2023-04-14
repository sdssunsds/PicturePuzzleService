#pragma once
#define CPPLIBDLL_EXPORTS
#ifdef CPPLIBDLL_EXPORTS
#define CPPLIBDLL_API __declspec(dllexport)
#else
#define CPPLIBDLL_API __declspec(dllimport)
#endif
#include <string>
class location
{
public:
	// 初始化
	virtual int init(std::string engine_path) = 0;
	virtual float get_dis(std::string bmp_path, std::string range_png_path, int state);
};
extern "C" CPPLIBDLL_API location * __stdcall LocationFactory();
extern "C" CPPLIBDLL_API int __stdcall CallOnInit(location * obj, const char* engine_path);//engine文件路径
extern "C" CPPLIBDLL_API float __stdcall Callgetdis(location * obj, const char* bmp_path, const char* png_path, int state);
extern "C" CPPLIBDLL_API void __stdcall DestroyLocation(location * obj);