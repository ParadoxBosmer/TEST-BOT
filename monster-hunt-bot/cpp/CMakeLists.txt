cmake_minimum_required(VERSION 3.10)
project(MonsterHuntBotTemplate)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Find libcurl
find_package(CURL REQUIRED)

# Add nlohmann/json (header-only library)
# You can download json.hpp from: https://github.com/nlohmann/json/releases
# Place it in the same directory as this CMakeLists.txt
include_directories(${CMAKE_CURRENT_SOURCE_DIR})

# Source files
set(SOURCES
    bot_template.cpp
)

# Create executable
add_executable(bot_template ${SOURCES})

# Link libraries
target_link_libraries(bot_template PRIVATE CURL::libcurl)

# Installation
install(TARGETS bot_template DESTINATION bin)
