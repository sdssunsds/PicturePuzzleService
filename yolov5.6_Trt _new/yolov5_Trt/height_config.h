#pragma once
#include <vector>
#include <map>
#include <string>
struct part_h
{
    int h;
    std::string part_name;
    std::string class_name;
};
const std::vector<part_h> height_map1{ {0,"ct","ct"},{1100,"pzq","pzq"},{3942,"tx1","tx"},{4820,"zxj_u","zxj"},{5600,"cz1","cz"},{9200,"cz2","cz"} ,
    {10300,"zxj_d","zxj"} ,{10950,"tx2","tx"},{29600,"zxj_u","zxj"} ,{30636,"cz1","cz"} ,{34178,"cz2","cz"} ,{35280,"zxj_d","zxj"} , {37700,"mjcg","mjcg"} };
const std::vector<part_h> height_map2{ {0,"ct","ct"},{2827,"zxj_u","zxj"},{3670,"cz1","cz"},{7250,"cz2","cz"} ,{8445,"zxj_d","zxj"} ,{27700,"zxj_u","zxj"} ,
    {28730,"cz3","cz"} ,{32300,"cz4","cz"} ,{33400,"zxj_d","zxj"} ,{35800,"mjcg","mjcg"} };
const std::vector<part_h> height_map3{ {0,"ct","ct"},{2827,"zxj_u","zxj"},{3670,"cz1","cz"},{7250,"cz2","cz"} ,{8445,"zxj_d","zxj"} ,{27700,"zxj_u","zxj"} ,
    {28730,"cz3","cz"} ,{32300,"cz4","cz"} ,{33400,"zxj_d","zxj"} ,{35800,"mjcg","mjcg"} };
const std::vector<int> height1{0, 1116 ,4820 ,10310 ,29598, 35280, 37700};
const std::vector<int> height2{0, 2827 ,8445 ,27700 ,33400 ,35800 };
const std::vector<int> height3{0, 2827, 8445, 27700, 33400, 35800 };
const std::vector<std::string> part1{ "010032701",  "010032618",  "010032801",  "020032619",  "020032801",  "030032620" };
const std::vector<std::string> part3{ "010032618",  "010032801",  "020032619",  "020032801",  "030032620"};
