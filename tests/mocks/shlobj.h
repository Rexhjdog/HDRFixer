#pragma once
#include <string>

typedef long HRESULT;
typedef void* REFKNOWNFOLDERID;
typedef unsigned long DWORD;
typedef void* HANDLE;
typedef wchar_t* PWSTR;

#define S_OK 0L
#define SUCCEEDED(hr) (((HRESULT)(hr)) >= 0)
#define FOLDERID_LocalAppData (REFKNOWNFOLDERID)0

inline HRESULT SHGetKnownFolderPath(REFKNOWNFOLDERID rfid, DWORD dwFlags, HANDLE hToken, PWSTR *ppszPath) {
    return -1; // Return failure so fallback is used
}

inline void CoTaskMemFree(void* pv) {}
