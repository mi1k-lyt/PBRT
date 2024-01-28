# PBRT
PBR实验框架

## How to Start
Linux ： 编译环境：gcc 7.5
```
git clone --recursive 'https...'
cd PBRT
mkdir build && cd build
cmake ..
make -j4
```
Windows ： 编译环境：MSVC
```
git clone --recursive 'https...'
cd PBRT
mkdir build && cd build
cmake .. -G "Visual Studio 17 2022" -A x64
cmake --build . --config Release
```

