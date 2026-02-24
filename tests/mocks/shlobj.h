#pragma once
#include "windows.h"

typedef void* REFKNOWNFOLDERID;

#define FOLDERID_LocalAppData ((REFKNOWNFOLDERID)0)
#define FOLDERID_RoamingAppData ((REFKNOWNFOLDERID)1)

inline HRESULT SHGetKnownFolderPath(REFKNOWNFOLDERID, DWORD, HANDLE, PWSTR*) {
    return E_FAIL; // Return failure so fallback to env var is used
}
