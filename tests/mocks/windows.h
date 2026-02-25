#pragma once
// Mock windows.h for Linux test compilation

#ifndef NOMINMAX
#define NOMINMAX
#endif

#include <cstdint>
#include <cstdlib>
#include <cstring>
#include <cwchar>
#include <string>

// Basic Windows types
// IMPORTANT: On Linux, long is 64-bit (LP64). Windows HRESULT/LSTATUS are 32-bit.
// We must use fixed-width types to ensure SUCCEEDED/FAILED macros work correctly
// (e.g., E_FAIL 0x80004005 must be negative).
typedef uint32_t DWORD;
typedef int32_t LONG;
typedef int32_t LSTATUS;
typedef int32_t HRESULT;
typedef int BOOL;
typedef unsigned char BYTE;
typedef unsigned int UINT;
typedef unsigned int UINT32;
typedef unsigned short WORD;
typedef void* HANDLE;
typedef void* HMODULE;
typedef void* HINSTANCE;
typedef void* HWND;
typedef void* HICON;
typedef void* HMENU;
typedef void* HKEY;
typedef void* HDEVNOTIFY;
typedef wchar_t* PWSTR;
typedef wchar_t* LPWSTR;
typedef const wchar_t* PCWSTR;
typedef const wchar_t* LPCWSTR;
typedef char* LPSTR;
typedef unsigned long long ULONG_PTR;
typedef long long LONG_PTR;
typedef unsigned long WPARAM;
typedef long LPARAM;
typedef long LRESULT;

// Windows-specific integer types
#ifndef TRUE
#define TRUE 1
#endif
#ifndef FALSE
#define FALSE 0
#endif
#ifndef MAX_PATH
#define MAX_PATH 260
#endif

// GUID struct
struct GUID {
    uint32_t Data1;
    uint16_t Data2;
    uint16_t Data3;
    uint8_t Data4[8];
};
inline bool IsEqualGUID(const GUID& a, const GUID& b) {
    return memcmp(&a, &b, sizeof(GUID)) == 0;
}

// LUID - used extensively in display code
struct LUID {
    DWORD LowPart;
    LONG HighPart;
};

// HRESULT helpers
#define S_OK ((HRESULT)0L)
#define S_FALSE ((HRESULT)1L)
#define E_FAIL ((HRESULT)0x80004005L)
#define SUCCEEDED(hr) (((HRESULT)(hr)) >= 0)
#define FAILED(hr) (((HRESULT)(hr)) < 0)

// Error codes
#define ERROR_SUCCESS 0L
#define ERROR_ALREADY_EXISTS 183L
#define ERROR_FILE_NOT_FOUND 2L

// Registry constants
#define HKEY_LOCAL_MACHINE ((HKEY)(ULONG_PTR)((LONG)0x80000002))
#define HKEY_CURRENT_USER ((HKEY)(ULONG_PTR)((LONG)0x80000001))
#define KEY_READ 0x20019
#define KEY_WRITE 0x20006
#define KEY_NOTIFY 0x0010
#define REG_SZ 1
#define REG_EXPAND_SZ 2
#define REG_DWORD 4
#define REG_OPTION_NON_VOLATILE 0

// Registry function stubs
inline LSTATUS RegOpenKeyExW(HKEY, const wchar_t*, DWORD, DWORD, HKEY*) { return ERROR_FILE_NOT_FOUND; }
inline LSTATUS RegQueryValueExW(HKEY, const wchar_t*, DWORD*, DWORD*, BYTE*, DWORD*) { return ERROR_FILE_NOT_FOUND; }
inline LSTATUS RegCloseKey(HKEY) { return ERROR_SUCCESS; }
inline LSTATUS RegCreateKeyExW(HKEY, const wchar_t*, DWORD, void*, DWORD, DWORD, void*, HKEY*, DWORD*) { return ERROR_FILE_NOT_FOUND; }
inline LSTATUS RegSetValueExW(HKEY, const wchar_t*, DWORD, DWORD, const BYTE*, DWORD) { return ERROR_FILE_NOT_FOUND; }
inline LSTATUS RegNotifyChangeKeyValue(HKEY, BOOL, DWORD, HANDLE, BOOL) { return ERROR_SUCCESS; }
#define REG_NOTIFY_CHANGE_LAST_SET 0x00000004
#define REG_NOTIFY_CHANGE_NAME 0x00000001

// WideChar/MultiByte conversion stubs
#define CP_UTF8 65001

inline int WideCharToMultiByte(unsigned int, DWORD, const wchar_t* src, int srcLen,
                                char* dst, int dstLen, const char*, const char*) {
    if (!src) return 0;
    size_t len = (srcLen == -1) ? wcslen(src) + 1 : static_cast<size_t>(srcLen);
    if (!dst || dstLen == 0) return static_cast<int>(len);
    size_t to_copy = (static_cast<size_t>(dstLen) < len) ? static_cast<size_t>(dstLen) : len;
    for (size_t i = 0; i < to_copy; ++i) {
        dst[i] = static_cast<char>(src[i] & 0x7F);
    }
    return static_cast<int>(to_copy);
}

inline int MultiByteToWideChar(unsigned int, DWORD, const char* src, int srcLen,
                                wchar_t* dst, int dstLen) {
    if (!src) return 0;
    size_t len = (srcLen == -1) ? strlen(src) + 1 : static_cast<size_t>(srcLen);
    if (!dst || dstLen == 0) return static_cast<int>(len);
    size_t to_copy = (static_cast<size_t>(dstLen) < len) ? static_cast<size_t>(dstLen) : len;
    for (size_t i = 0; i < to_copy; ++i) {
        dst[i] = static_cast<wchar_t>(src[i]);
    }
    return static_cast<int>(to_copy);
}

// FormatMessage stub
#define FORMAT_MESSAGE_FROM_SYSTEM 0x00001000
#define FORMAT_MESSAGE_IGNORE_INSERTS 0x00000200
#define MAKELANGID(p, s) ((((WORD)(s)) << 10) | (WORD)(p))
#define LANG_NEUTRAL 0x00
#define SUBLANG_DEFAULT 0x01

inline DWORD FormatMessageA(DWORD, const void*, DWORD, DWORD, char* buf, DWORD size, void*) {
    if (buf && size > 0) {
        strncpy(buf, "mock error", size - 1);
        buf[size - 1] = '\0';
    }
    return 0;
}

// File/Path functions
inline DWORD GetTempPathW(DWORD nBufferLength, wchar_t* lpBuffer) {
    const wchar_t* tmp = L"/tmp/";
    if (lpBuffer && nBufferLength > 5) {
        wcscpy(lpBuffer, tmp);
        return 5;
    }
    return 6;
}

inline DWORD GetSystemDirectoryW(wchar_t* lpBuffer, UINT uSize) {
    const wchar_t* dir = L"/tmp/system32";
    if (lpBuffer && uSize > 13) {
        wcscpy(lpBuffer, dir);
        return 13;
    }
    return 14;
}

// Misc Windows API stubs
inline HANDLE CreateMutexW(void*, BOOL, const wchar_t*) { return (HANDLE)1; }
inline DWORD GetLastError() { return 0; }
inline BOOL CloseHandle(HANDLE) { return TRUE; }
inline HANDLE CreateEventW(void*, BOOL, BOOL, const wchar_t*) { return (HANDLE)1; }
inline BOOL SetEvent(HANDLE) { return TRUE; }
inline BOOL ResetEvent(HANDLE) { return TRUE; }
inline DWORD WaitForMultipleObjects(DWORD, const HANDLE*, BOOL, DWORD) { return 0; }
#define WAIT_OBJECT_0 0
#define WAIT_TIMEOUT 0x00000102L

// COM stubs
#define COINIT_APARTMENTTHREADED 0x2
inline HRESULT CoInitializeEx(void*, DWORD) { return S_OK; }
inline void CoUninitialize() {}
inline void CoTaskMemFree(void*) {}

// Window message constants
#define WM_APP 0x8000
#define WM_NULL 0x0000
#define WM_DESTROY 0x0002
#define WM_COMMAND 0x0111
#define WM_HOTKEY 0x0312
#define WM_RBUTTONUP 0x0205
#define WM_CONTEXTMENU 0x007B
#define WM_DEVICECHANGE 0x0219
#define WM_DISPLAYCHANGE 0x007E

// Window functions stubs
#define GWLP_USERDATA (-21)
#define WS_POPUP 0x80000000L
#define HWND_MESSAGE ((HWND)-3)
#define IDI_APPLICATION ((const wchar_t*)32512)
#define MF_STRING 0x00000000
#define MF_SEPARATOR 0x00000800
#define MF_CHECKED 0x00000008
#define TPM_RIGHTBUTTON 0x0002
#define MB_OK 0x00000000
#define MB_ICONERROR 0x00000010
#define MB_ICONINFORMATION 0x00000040
#define MOD_CONTROL 0x0002
#define MOD_SHIFT 0x0004
#define _TRUNCATE ((size_t)-1)
#define CALLBACK
#define WINAPI
typedef LRESULT (*WNDPROC)(HWND, UINT, WPARAM, LPARAM);

struct WNDCLASSEXW {
    UINT cbSize;
    UINT style;
    WNDPROC lpfnWndProc;
    int cbClsExtra;
    int cbWndExtra;
    HINSTANCE hInstance;
    HICON hIcon;
    void* hCursor;
    void* hbrBackground;
    const wchar_t* lpszMenuName;
    const wchar_t* lpszClassName;
    HICON hIconSm;
};

struct POINT { LONG x; LONG y; };
struct MSG { HWND hwnd; UINT message; WPARAM wParam; LPARAM lParam; DWORD time; POINT pt; };

inline BOOL RegisterClassExW(const WNDCLASSEXW*) { return TRUE; }
inline HWND CreateWindowExW(DWORD, const wchar_t*, const wchar_t*, DWORD, int, int, int, int, HWND, HMENU, HINSTANCE, void*) { return (HWND)1; }
inline BOOL DestroyWindow(HWND) { return TRUE; }
inline LONG_PTR SetWindowLongPtrW(HWND, int, LONG_PTR) { return 0; }
inline LONG_PTR GetWindowLongPtrW(HWND, int) { return 0; }
inline BOOL GetMessage(MSG*, HWND, UINT, UINT) { return FALSE; }
inline BOOL TranslateMessage(const MSG*) { return TRUE; }
inline LRESULT DispatchMessage(const MSG*) { return 0; }
inline LRESULT DefWindowProcW(HWND, UINT, WPARAM, LPARAM) { return 0; }
inline BOOL PostMessage(HWND, UINT, WPARAM, LPARAM) { return TRUE; }
inline void PostQuitMessage(int) {}
inline BOOL SetForegroundWindow(HWND) { return TRUE; }
inline BOOL GetCursorPos(POINT*) { return TRUE; }
inline HICON LoadIconW(HINSTANCE, const wchar_t*) { return nullptr; }
inline BOOL RegisterHotKey(HWND, int, UINT, UINT) { return TRUE; }
inline BOOL UnregisterHotKey(HWND, int) { return TRUE; }
inline UINT RegisterWindowMessageW(const wchar_t*) { return WM_APP + 100; }
inline int MessageBoxW(HWND, const wchar_t*, const wchar_t*, UINT) { return 0; }
inline HMENU CreatePopupMenu() { return (HMENU)1; }
inline BOOL AppendMenuW(HMENU, UINT, UINT, const wchar_t*) { return TRUE; }
inline BOOL TrackPopupMenu(HMENU, UINT, int, int, int, HWND, void*) { return TRUE; }
inline BOOL DestroyMenu(HMENU) { return TRUE; }

// wcsncpy_s / wcscpy_s stubs
inline int wcscpy_s(wchar_t* dst, const wchar_t* src) {
    wcscpy(dst, src);
    return 0;
}
inline int wcsncpy_s(wchar_t* dst, const wchar_t* src, size_t count) {
    wcsncpy(dst, src, count);
    return 0;
}

// Shell API
#define NIF_ICON 0x00000002
#define NIF_MESSAGE 0x00000001
#define NIF_TIP 0x00000004
#define NIF_INFO 0x00000010
#define NIM_ADD 0x00000000
#define NIM_MODIFY 0x00000001
#define NIM_DELETE 0x00000002
#define NIIF_INFO 0x00000001

struct NOTIFYICONDATAW {
    DWORD cbSize;
    HWND hWnd;
    UINT uID;
    UINT uFlags;
    UINT uCallbackMessage;
    HICON hIcon;
    wchar_t szTip[128];
    DWORD dwState;
    DWORD dwStateMask;
    wchar_t szInfo[256];
    union { UINT uTimeout; UINT uVersion; };
    wchar_t szInfoTitle[64];
    DWORD dwInfoFlags;
};

inline BOOL Shell_NotifyIconW(DWORD, NOTIFYICONDATAW*) { return TRUE; }

// Device notification
#define DBT_DEVICEARRIVAL 0x8000
#define DBT_DEVICEREMOVECOMPLETE 0x8004
#define DBT_DEVTYP_DEVICEINTERFACE 0x00000005
#define DEVICE_NOTIFY_WINDOW_HANDLE 0

struct DEV_BROADCAST_HDR {
    DWORD dbch_size;
    DWORD dbch_devicetype;
    DWORD dbch_reserved;
};

struct DEV_BROADCAST_DEVICEINTERFACE {
    DWORD dbcc_size;
    DWORD dbcc_devicetype;
    DWORD dbcc_reserved;
    GUID dbcc_classguid;
    wchar_t dbcc_name[1];
};

inline HDEVNOTIFY RegisterDeviceNotificationW(HWND, void*, DWORD) { return (HDEVNOTIFY)1; }
inline BOOL UnregisterDeviceNotification(HDEVNOTIFY) { return TRUE; }

// OutputDebugString
inline void OutputDebugStringA(const char*) {}

// localtime_s (Linux uses localtime_r)
#include <ctime>
inline int localtime_s(std::tm* result, const std::time_t* timer) {
    auto* r = localtime_r(timer, result);
    return r ? 0 : -1;
}

// _wgetenv
inline const wchar_t* _wgetenv(const wchar_t*) { return nullptr; }

// _putenv_s
inline int _putenv_s(const char* name, const char* value) {
    if (value && value[0]) {
        setenv(name, value, 1);
    } else {
        unsetenv(name);
    }
    return 0;
}

// ICM/color profile stubs
typedef int WCS_PROFILE_MANAGEMENT_SCOPE;
#define WCS_PROFILE_MANAGEMENT_SCOPE_CURRENT_USER 0
inline BOOL InstallColorProfileW(void*, const wchar_t*) { return FALSE; }
inline BOOL UninstallColorProfileW(void*, const wchar_t*, BOOL) { return FALSE; }
inline HMODULE GetModuleHandleW(const wchar_t*) { return nullptr; }
inline HMODULE LoadLibraryW(const wchar_t*) { return nullptr; }
inline void* GetProcAddress(HMODULE, const char*) { return nullptr; }

// LOWORD macro
#define LOWORD(l) ((WORD)((DWORD_PTR)(l) & 0xffff))
typedef ULONG_PTR DWORD_PTR;

// DXGI stubs (minimal)
#define DXGI_COLOR_SPACE_RGB_FULL_G2084_NONE_P2020 12
#define DXGI_ADAPTER_FLAG_SOFTWARE 2
#define DXGI_ERROR_NOT_FOUND ((HRESULT)0x887A0002L)

// DisplayConfig stubs
#define QDC_ONLY_ACTIVE_PATHS 2
#define DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL 0xFFFFFFFF

struct DISPLAYCONFIG_DEVICE_INFO_HEADER {
    DWORD type;
    DWORD size;
    LUID adapterId;
    DWORD id;
};

struct DISPLAYCONFIG_SDR_WHITE_LEVEL {
    DISPLAYCONFIG_DEVICE_INFO_HEADER header;
    DWORD SDRWhiteLevel;
};

struct DISPLAYCONFIG_TARGET_INFO {
    LUID adapterId;
    DWORD id;
};

struct DISPLAYCONFIG_SOURCE_INFO {
    LUID adapterId;
    DWORD id;
};

struct DISPLAYCONFIG_PATH_INFO {
    DISPLAYCONFIG_SOURCE_INFO sourceInfo;
    DISPLAYCONFIG_TARGET_INFO targetInfo;
};

struct DISPLAYCONFIG_MODE_INFO {};

inline LONG GetDisplayConfigBufferSizes(DWORD, UINT32*, UINT32*) { return ERROR_FILE_NOT_FOUND; }
inline LONG QueryDisplayConfig(DWORD, UINT32*, DISPLAYCONFIG_PATH_INFO*, UINT32*, DISPLAYCONFIG_MODE_INFO*, void*) { return ERROR_FILE_NOT_FOUND; }
inline LONG DisplayConfigGetDeviceInfo(DISPLAYCONFIG_DEVICE_INFO_HEADER*) { return ERROR_FILE_NOT_FOUND; }

// std::round needed for display_config.h
#include <cmath>
