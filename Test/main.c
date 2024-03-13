// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// On unix make sure to compile using -ldl and -pthread flags.

#define PathToLibrary "..\\MU3Input\\bin\\x64\\Release\\net8.0\\win-x64\\publish\\MU3Input.dll"

#include "windows.h"
#pragma comment(lib, "ole32.lib")

#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>

#define mu3_io_get_api_version uint16_t (*)(void)
#define mu3_io_init uint32_t (*)(void)
#define mu3_io_poll uint32_t (*)(void)
#define mu3_io_get_opbtns void (*)(uint8_t *)
#define mu3_io_get_gamebtns void (*)(uint8_t *, uint8_t *)
#define mu3_io_get_lever void (*)(uint16_t *)
#define mu3_io_set_led void (*)(uint32_t)

#define aime_io_get_api_version uint16_t (*)(void)
#define aime_io_init uint32_t (*)(void)
#define aime_io_nfc_poll uint32_t (*)(uint8_t)
#define aime_io_nfc_get_felica_id uint32_t (*)(uint8_t, uint64_t *)
#define aime_io_nfc_get_felica_pm uint32_t (*)(uint8_t, uint64_t *)
#define aime_io_nfc_get_felica_system_code uint32_t (*)(uint8_t, uint16_t *)
#define aime_io_nfc_get_aime_id uint32_t (*)(uint8_t, uint8_t *, uint64_t)
#define aime_io_led_set_color void (*)(uint8_t, uint8_t, uint8_t, uint8_t)

typedef struct
{
    uint16_t (*get_api_version)(void);
    uint32_t (*init)(void);
    uint32_t (*poll)(void);
    void (*get_opbtns)(uint8_t *);
    void (*get_gamebtns)(uint8_t *, uint8_t *);
    void (*get_lever)(uint16_t *);
    void (*set_led)(uint32_t);
} mu3;

typedef struct
{
    uint16_t (*get_api_version)(void);
    uint32_t (*init)(void);
    uint32_t (*nfc_poll)(uint8_t);
    uint32_t (*nfc_get_felica_id)(uint8_t, uint64_t *);
    uint32_t (*nfc_get_felica_pm)(uint8_t, uint64_t *);
    uint32_t (*nfc_get_felica_system_code)(uint8_t, uint16_t *);
    uint32_t (*nfc_get_aime_id)(uint8_t, uint8_t *, uint64_t);
    void (*led_set_color)(uint8_t, uint8_t, uint8_t, uint8_t);
} aime;

void init(HINSTANCE handle, mu3 *mu3, aime *aime)
{
    mu3->get_api_version = (mu3_io_get_api_version)GetProcAddress(handle, "mu3_io_get_api_version");
    mu3->init = (mu3_io_init)GetProcAddress(handle, "mu3_io_init");
    mu3->poll = (mu3_io_poll)GetProcAddress(handle, "mu3_io_poll");
    mu3->get_opbtns = (mu3_io_get_opbtns)GetProcAddress(handle, "mu3_io_get_opbtns");
    mu3->get_gamebtns = (mu3_io_get_gamebtns)GetProcAddress(handle, "mu3_io_get_gamebtns");
    mu3->get_lever = (mu3_io_get_lever)GetProcAddress(handle, "mu3_io_get_lever");
    mu3->set_led = (mu3_io_set_led)GetProcAddress(handle, "mu3_io_set_led");

    aime->get_api_version = (aime_io_get_api_version)GetProcAddress(handle, "aime_io_get_api_version");
    aime->init = (aime_io_init)GetProcAddress(handle, "aime_io_init");
    aime->nfc_poll = (aime_io_nfc_poll)GetProcAddress(handle, "aime_io_nfc_poll");
    aime->nfc_get_felica_id = (aime_io_nfc_get_felica_id)GetProcAddress(handle, "aime_io_nfc_get_felica_id");
    aime->nfc_get_felica_pm = (aime_io_nfc_get_felica_pm)GetProcAddress(handle, "aime_io_nfc_get_felica_pm");
    aime->nfc_get_felica_system_code = (aime_io_nfc_get_felica_system_code)GetProcAddress(handle, "aime_io_nfc_get_felica_system_code");
    aime->nfc_get_aime_id = (aime_io_nfc_get_aime_id)GetProcAddress(handle, "aime_io_nfc_get_aime_id");
    aime->led_set_color = (aime_io_led_set_color)GetProcAddress(handle, "aime_io_led_set_color");
}

int main()
{

    // Call sum function defined in C# shared library
    HINSTANCE handle = LoadLibraryA(PathToLibrary);

    mu3 mu3;
    aime aime;
    init(handle, &mu3, &aime);

    mu3.init();
    aime.init();
    printf("mu3_version: %d\n", mu3.get_api_version());
    printf("aime_version: %d\n", aime.get_api_version());

    uint8_t left = 0, right = 0;
    uint64_t id = 0;
    uint16_t code = 0;
    uint8_t aimeid[10] = {0};

    while (1)
    {
        mu3.poll();
        mu3.get_gamebtns(&left, &right);
        aime.nfc_get_felica_id(0, &id);
        aime.nfc_get_felica_pm(0, &id);
        aime.nfc_get_felica_system_code(0, &code);
        aime.nfc_get_aime_id(0, aimeid, 10);
    }
    return 0;
}
