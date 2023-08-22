cmake_minimum_required(VERSION 3.19.2)
project(PBRTEST)

set(EXECUTABLE_OUTPUT_PATH ${CMAKE_CURRENT_SOURCE_DIR}/bin)
set(LIBRARY_OUTPUT_PATH ${CMAKE_CURRENT_SOURCE_DIR}/bin)

set(VENDOR_PATH ${CMAKE_CURRENT_SOURCE_DIR}/vendor)
set(SRC_PATH ${CMAKE_CURRENT_SOURCE_DIR}/src)
set(UTILS_PATH ${SRC_PATH}/utils)
set(SHADER_PATH ${SRC_PATH}/shaders)

add_subdirectory(${VENDOR_PATH}/GLFW)

if(WIN32)
    set(LIBS opengl32 assimp freetype irrKlang)
    add_definitions(-D_CRT_SECURE_NO_WARNINGS)
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
)

include_directories(
    ${VENDOR_PATH}/glad/include
    ${VENDOR_PATH}/GLFW/include
    ${VENDOR_PATH}/stb_image
    ${VENDOR_PATH}/glm
    ${SRC_PATH}
    ${SRC_PATH}/shaders
)

file(GLOB_RECURSE FILES 
    ${SRC_PATH}/*.cpp
    ${SRC_PATH}/*.h
    ${SHADER_PATH}/*.vs
    ${SHADER_PATH}/*.fs
)

add_executable(PBRT
    ${FILES}
)

target_link_libraries(PBRT
    glfw3
    ${LIBS}
)
