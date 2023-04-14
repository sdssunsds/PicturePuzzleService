#include "CppLibDll.h"
#include "yolov5.h" // 实现了接口类的具体子类



extern "C" CPPLIBDLL_API IExport * __stdcall ExportObjectFactory()
{
	return new cutimg();
}

extern "C" CPPLIBDLL_API void __stdcall DestroyExportObject(IExport * obj)
{
	if (obj)
	{
		delete obj;
		obj = nullptr;
	}
}

extern "C" CPPLIBDLL_API int __stdcall CallOnInit(IExport * obj, const char* engine_path)
{
	if (obj) {
		return obj->init(engine_path);
	}
	else
	{
		return -1;
	}
}

extern "C" CPPLIBDLL_API int __stdcall Callcutimg(IExport * obj, const config_info input_info,
	const inputrect_4 * cut_info, const int cut_info_len, const int* height, const int height_len, int progress)
{
	std::cout << "调用dll" << std::endl;
	if (obj) {
		int res_no = obj->run_procedure(input_info, cut_info, cut_info_len, height, height_len, progress);
		return res_no;
	}
	else {
		return -1;
	}
}
