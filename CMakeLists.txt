cmake_minimum_required(VERSION 3.19.2)
project(PBRTEST)

set(EXECUTABLE_OUTPUT_PATH ${CMAKE_CURRENT_SOURCE_DIR}/bin)
set(LIBRARY_OUTPUT_PATH ${CMAKE_CURRENT_SOURCE_DIR}/bin)

set(VENDOR_PATH ${CMAKE_CURRENT_SOURCE_DIR}/vendor)
set(SRC_PATH ${CMAKE_CURRENT_SOURCE_DIR}/src)
set(UTILS_PATH ${SRC_PATH}/utils)
set(SHADER_PATH ${SRC_PATH}/shaders)

add_subdirectory(${VENDOR_PATH}/GLFW)
add_subdirectory(${VENDOR_PATH}/assimp)

if(WIN32)
elseif(UNIX AND NOT APPLE)
    set(LIBS X11 Xrandr Xinerama Xi Xxf86vm Xcursor GL dl pthread freetype )
endif()


add_library(STB_IMAGE ${VENDOR_PATH}/stb_image/stb_image.cpp)
set(LIBS ${LIBS} STB_IMAGE)

add_library(GLAD ${VENDOR_PATH}/glad/src/glad.c)
set(LIBS ${LIBS} GLAD)

configure_file(${SRC_PATH}/utils/root_directory.h.in ${SRC_PATH}/utils/root_directory.h)

link_directories(
    ${LIBRARY_OUTPUT_PATH}
    ${LIBRARY_OUTPUT_PATH}/Debug
    ${LIBRARY_OUTPUT_PATH}/Release
    ${LIBRARY_OUTPUT_PATH}/RelWithDebInfo
)

include_directories(
    ${VENDOR_PATH}/glad/include
    ${VENDOR_PATH}/GLFW/include
    ${VENDOR_PATH}/stb_image
    ${VENDOR_PATH}/glm
    ${VENDOR_PATH}/imgui
    ${VENDOR_PATH}/imgui/backends
    ${VENDOR_PATH}/assimp/include
    ${SRC_PATH}
    ${SRC_PATH}/shaders
    ${SRC_PATH}/utils
)

file(GLOB_RECURSE FILES 
    ${SRC_PATH}/*.cpp
    ${SRC_PATH}/*.h
    ${SHADER_PATH}/*.vs
    ${SHADER_PATH}/*.fs
)

file(GLOB_RECURSE IMGUI_FILES
    ${VENDOR_PATH}/imgui/imconfig.h
    ${VENDOR_PATH}/imgui/imgui.h
    ${VENDOR_PATH}/imgui/imgui.cpp
    ${VENDOR_PATH}/imgui/imgui_draw.cpp
    ${VENDOR_PATH}/imgui/imgui_internal.h
    ${VENDOR_PATH}/imgui/imgui_tables.cpp
    ${VENDOR_PATH}/imgui/imgui_widgets.cpp
    ${VENDOR_PATH}/imgui/imstb_rectpack.h
    ${VENDOR_PATH}/imgui/imstb_textedit.h
    ${VENDOR_PATH}/imgui/imstb_truetype.h
    ${VENDOR_PATH}/imgui/imgui_demo.cpp
)

file(GLOB_RECURSE IMGUI_IMPL_FILES
    ${VENDOR_PATH}/imgui/backends/imgui_impl_glfw.h
    ${VENDOR_PATH}/imgui/backends/imgui_impl_opengl3.h
    ${VENDOR_PATH}/imgui/backends/imgui_impl_glfw.cpp
    ${VENDOR_PATH}/imgui/backends/imgui_impl_opengl3.cpp
)

add_library(IMGUI ${IMGUI_FILES})
set(LIBS ${LIBS} IMGUI)

add_library(IMGUI_IMPL ${IMGUI_IMPL_FILES})
set(LIBS ${LIBS} IMGUI_IMPL)

add_executable(PBRT
    ${FILES}
)

target_link_libraries(PBRT
    assimp
    glfw3.lib
    ${LIBS}
)
