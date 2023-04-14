#include "../yolov5_Trt/CppLibDll.h"
#pragma comment(lib, "../x64/Debug/yolov5_Trt.lib") // 隐式调用
#include<windows.h>
#include <iostream>
#include <io.h>
#include<fstream>

using namespace std;
void getFiles(string path, vector<string>& files)
{
    //文件句柄
    intptr_t   hFile = 0;
    //文件信息
    struct _finddata_t fileinfo;
    string p;
    if ((hFile = _findfirst(p.assign(path).append("/*").c_str(), &fileinfo)) != -1)
    {
        do
        {
            //如果是目录,迭代之
            ////如果不是,加入列表
            if ((fileinfo.attrib & _A_SUBDIR))
            {
                files.push_back(p.assign(path).append("/").append(fileinfo.name));
            }
        } while (_findnext(hFile, &fileinfo) == 0);
        _findclose(hFile);
    }
}
int main()
{
    std::string train_type = "380AL_";
    std::string time = "2021-11-30_15:49";
    IExport* p = ExportObjectFactory();
    std::string model_path = "../model/concat_yolov560_m_640_4_1031.engine";
    p->init(model_path);
    std::string path = "C:\\Users\\wang\\Desktop\\";
    std::string savepath = path;
    std::vector<int> train_model = {2638};//2590
    for (auto s : train_model)
    {
        config_info input_info;
        strcpy_s(input_info.img_path, (path+std::to_string(s)).c_str());
        strcpy_s(input_info.train_type, (train_type + std::to_string(s)).c_str());
        time = ""; //std::to_string(i);
        strcpy_s(input_info.time, time.c_str());
        strcpy_s(input_info.save_path, (savepath + std::to_string(s)+"/").c_str());
        int cut_info_length = 1;
        int height[8] = { 37674,35788,35874,35820,35840,35876,35840,35794 };
        int height_len = 8;
        int progress = 0;
        inputrect_4 cut_info;
        p->run_procedure(input_info, &cut_info, cut_info_length, height, height_len, progress);
    }
    return 0;
}