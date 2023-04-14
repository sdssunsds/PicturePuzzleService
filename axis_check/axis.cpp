#include "axis.h"
#include "axis_check.h"
float location::get_dis(std::string bmp_path, std::string range_png_path,int state)
{
	return 0;
}
extern "C" CPPLIBDLL_API location * __stdcall LocationFactory()
{
	return new axis_check();
}
extern "C" CPPLIBDLL_API int __stdcall CallOnInit(location * obj,const char* engine_path)
{
	if (obj)
	{
		obj->init(engine_path);
		return 0;
	}
	else
	{
		return -1;
	}
}
extern "C" CPPLIBDLL_API float __stdcall Callgetdis(location * obj, const char* bmp_path, const char* png_path, int state)
{
	return obj->get_dis(bmp_path, png_path,state);
}
extern "C" CPPLIBDLL_API void __stdcall DestroyLocation(location * obj)
{
	if (obj)
	{
		delete obj;
		obj = nullptr;
	}
}