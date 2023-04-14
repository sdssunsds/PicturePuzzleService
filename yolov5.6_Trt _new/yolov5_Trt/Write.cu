#include <cuda_runtime.h>
#include <device_launch_parameters.h>
#include <stdio.h>
#include<vector>
#include <thrust/host_vector.h>
#include <thrust/device_vector.h>
#include <thrust/extrema.h>
#include <thrust/functional.h>
#include "write.h"
__global__ void get_float(uint8_t* data, float* float_data, const int h, const int w)
{
	const int n = threadIdx.x;
	const int ID = blockIdx.x * blockDim.x + n;
	if (ID < h)
	{
		//printf("blockIdx.x: %d ,blockDim.x: %d ,n:%d \n", blockIdx.x, blockDim.x, blockIdx.x * blockDim.x + n);
		for (int j = 0; j < w; j++)
		{
			float_data[ID * w + j]=float(data[ID * w + j]) / 255.0;
		//	float num= float(data[ID * w + j]) / 255.0;			
		//	if (num>0.5)
		//	{
		//		float_data[ID * w + j] = 1-tanh(1 - num);
		//	}
		//	else
		//	{
		//		float_data[ID * w + j] = num;
		//	}

		}
	}
}
__global__ void stand_img(float* data, double mean, double var, int h, int w)
{
	const int n = threadIdx.x;
	if (blockIdx.x * blockDim.x + n < h)
	{
		for (int j = 0; j < w; j++)
		{
			int  ID = (blockIdx.x * blockDim.x + n) * w + j;
			data[ID] = (data[ID] - mean) / var;
		}
	}

}
__global__ void get_res(float* data,bool* bool_data ,uint8_t* imgres, double min_num, double max_num, int h, int w)
{
	const int n = threadIdx.x;
	double handel = max_num - min_num;
	if (blockIdx.x * blockDim.x + n < h)
	{
		for (int j = 0; j < w; j++)
		{
			if (blockIdx.x * blockDim.x + n < h)
			{
				float f = (data[(blockIdx.x * blockDim.x + n) * w + j] - min_num) / handel;
				float k = 1 - f<0.4 ? 0.4:1 - f;
				k = bool_data[(blockIdx.x * blockDim.x + n) * w + j] ? k - 0.1 : k;
				imgres[(blockIdx.x * blockDim.x + n) * w + j] = (uchar)(round(tanh(f *5 *k) * 255));
				//imgres[(blockIdx.x * blockDim.x + n) * w + j] = (uchar)(round(tanh((data[(blockIdx.x * blockDim.x + n) * w + j] - min) / handel * 3) * 255));

			}
		}
	}
}
__global__ void get_bool(uchar* data,bool* output,int kernel_size,int h, int w)
{
	const int x =blockIdx.x*blockDim.x+ threadIdx.x;
	const int y = blockIdx.y;
	if (x<w)
	{		
		float l = float(data[x + y * w]);
		float k = float(data[x + y * w]);
		int xmin = x - kernel_size < 0 ? 0 : x - kernel_size;
		int xmax = x + kernel_size < w ? x+kernel_size : w;
		int ymin = y - kernel_size < 0 ? 0 : y - kernel_size;
		int ymax = y + kernel_size < h ? y+kernel_size : h;
		for (int i = xmin; i < xmax; i++)
		{
			for (int j = ymin; j < ymax; j++)
			{
				k = k + data[i + j * w];
			}
		}
		k = k / ((xmax - xmin) * (ymax - ymin));
		output[x + y * w] = bool(k-l >30);
	}
	
}
struct variance : std::unary_function<float, float>
{
	variance(float m) : mean(m) { } const float mean;
	__host__ __device__ float operator()(float data) const
	{
		return ::pow(data - mean, 2.0f);
	}
};

void write_new(cv::Mat& input, cv::Mat& output)
{
	const int h_h = input.rows;
	const int h_w = input.cols;
	const int threadnum = 1024;
	//创建显存并把图像复制进显存
	int blocksize = threadnum;
	int gridsize = (input.rows - 1) / threadnum + 1;
	dim3 grid_size(h_w/ threadnum, h_h);
	int  size_image = input.cols * input.rows;
	int size_image_fp = input.cols * input.rows * sizeof(float);
	uint8_t* data;
	cudaMalloc((void**)&data, size_image_fp);
	cudaMemcpy(data, input.data, size_image, cudaMemcpyHostToDevice);
	float* float_data;
	cudaMalloc((void**)&float_data, size_image_fp);
	bool* bool_data;
	cudaMalloc((void**)&bool_data, input.cols * input.rows * sizeof(bool));
	get_bool <<<grid_size, blocksize >>> (data, bool_data, 4, h_h, h_w);
	
	get_float <<<gridsize, blocksize >>> (data, float_data, h_h, h_w);
	thrust::device_ptr<float> img_D_vector(float_data);

	double mean = thrust::reduce(img_D_vector, img_D_vector + size_image) / (size_image);
	float var = thrust::transform_reduce(img_D_vector, img_D_vector + size_image, variance(mean), 0.0f, thrust::plus<double>())/ size_image;
	stand_img <<<gridsize, blocksize >>> (float_data, mean, var, h_h, h_w);
	auto max_num = thrust::max_element(img_D_vector, img_D_vector + size_image);
	double max_val = *max_num;
	auto min_num = thrust::min_element(img_D_vector, img_D_vector + size_image);
	double min_val = *min_num;
	//	//创建结果图像内存和显存
	uint8_t* res_img_d;
	uint8_t* res_data = (uint8_t*)malloc(size_image);
	cudaMalloc((void**)&res_img_d, size_image);
	get_res << <gridsize, blocksize >> > (float_data, bool_data, res_img_d, min_val, max_val, h_h, h_w);
	cudaMemcpy(res_data, res_img_d, size_image, cudaMemcpyDeviceToHost);
	cv::Mat image(h_h, h_w, CV_8UC1, res_data);
	//image.copyTo(output);
	output = image;
	//还差把数据赋值回图像
	cudaFree(data);
	cudaFree(bool_data);
	cudaFree(float_data);
	cudaFree(res_img_d);
}

