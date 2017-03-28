cmake_minimum_required(VERSION 3.5)

project(GTANativePlugin)

set(USE_CONAN OFF CACHE BOOL "Use the conan package manager")
if(USE_CONAN)
	include(${CMAKE_BINARY_DIR}/conanbuildinfo.cmake)
	conan_basic_setup()
endif()

SET(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} ${CMAKE_CURRENT_SOURCE_DIR}/cmake/)

#add_subdirectory(deps)
add_subdirectory(src)