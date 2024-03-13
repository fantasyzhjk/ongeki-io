// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//On unix make sure to compile using -ldl and -pthread flags.

//Set this value accordingly to your workspace settings
#if defined(_WIN32)
#define PathToLibrary "bin\\x64\\Release\\net8.0\\win-x64\\publish\\MU3Input.dll"
#elif defined(__APPLE__)
#define PathToLibrary "./bin/Release/net8.0/osx-x64/publish/MU3Input.dylib"
#else
#define PathToLibrary "./bin/Release/net8.0/linux-x64/publish/MU3Input.so"
#endif

#ifdef _WIN32
#include "windows.h"
#define symLoad GetProcAddress
#pragma comment (lib, "ole32.lib")
#else
#include "dlfcn.h"
#include <unistd.h>
#define symLoad dlsym
#define CoTaskMemFree free
#endif

#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>

#ifndef F_OK
#define F_OK    0
#endif

int main()
{

    // Call sum function defined in C# shared library
    #ifdef _WIN32
        HINSTANCE handle = LoadLibraryA(PathToLibrary);
    #else
        void *handle = dlopen(PathToLibrary, RTLD_LAZY);
    #endif

    typedef uint32_t(*init_f)(void);
    init_f init = (init_f)symLoad(handle, "mu3_io_init");
    typedef uint32_t(*poll_f)(void);
    poll_f poll = (poll_f)symLoad(handle, "mu3_io_poll");
    typedef uint16_t(*version_f)(void);
    version_f version = (version_f)symLoad(handle, "mu3_io_get_api_version");
    typedef void(*lever_f)(short*);
    lever_f lever = (lever_f)symLoad(handle, "mu3_io_get_lever");
    typedef void(*setled_f)(uint32_t*);
    setled_f setled = (setled_f)symLoad(handle, "mu3_io_set_led");

    uint32_t result = init();
    printf("init: %d\n", result);
    printf("version: %d\n", version());
    // uint8_t btn = 0;
    // lever(&btn);
    // printf("%d\n", btn);


    // CoreRT libraries do not support unloading
    // See https://github.com/dotnet/corert/issues/7887
    return 0;
}
