#pragma once
#include "windows.h"

struct DXGI_ADAPTER_DESC1 {
    wchar_t Description[128];
    UINT VendorId;
    UINT DeviceId;
    UINT SubSysId;
    UINT Revision;
    size_t DedicatedVideoMemory;
    size_t DedicatedSystemMemory;
    size_t SharedSystemMemory;
    LUID AdapterLuid;
    UINT Flags;
};

struct DXGI_OUTPUT_DESC1 {
    wchar_t DeviceName[32];
    int DesktopCoordinates[4];
    BOOL AttachedToDesktop;
    int Rotation;
    HINSTANCE Monitor;
    UINT BitsPerColor;
    int ColorSpace;
    float RedPrimary[2];
    float GreenPrimary[2];
    float BluePrimary[2];
    float WhitePoint[2];
    float MinLuminance;
    float MaxLuminance;
    float MaxFullFrameLuminance;
};
