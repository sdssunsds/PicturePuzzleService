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
	int imgNO;//�����
	std::string location_str;//������
	std::string part_location_str;//����λ�ñ��
	std::string part_str;//�������
	std::string only_str;//Ψһ���
	box_inf rect;//��������
};
struct config_info
{
	std::string img_path;//��ͼ·��
	std::string train_type;//����
	std::string time;//ʱ��
	std::string save_path;//�洢·��
};
// �����Ľӿ���
class IExport
{
public:
	// ��ʼ��
	virtual int init(std::string engine_path)=0;

	// ����ֵ:ʧ�ܻ�ɹ�ָʾ
	virtual int run_procedure(const config_info input_info, const inputrect_4* cut_info, 
		const int cut_info_length, const int* length, const int length_num,int& progress)=0;
	virtual ~IExport() {}
};

extern "C" CPPLIBDLL_API IExport* __stdcall ExportObjectFactory();
extern "C" CPPLIBDLL_API void __stdcall DestroyExportObject(IExport* obj);

extern "C" CPPLIBDLL_API int __stdcall CallOnInit(IExport* obj, const char* engine_path);
extern "C" CPPLIBDLL_API int __stdcall Callcutimg(IExport* obj,const config_info input_info, const inputrect_4* cut_info, 
	int cut_info_length, const int* length, const int length_num, int& progress);
