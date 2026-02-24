#pragma once

namespace Microsoft { namespace WRL {
    template<typename T>
    class ComPtr {
    public:
        ComPtr() : ptr_(nullptr) {}
        ~ComPtr() {}
        T* operator->() { return ptr_; }
        T** operator&() { return &ptr_; }
        T* Get() { return ptr_; }
        void Reset() { ptr_ = nullptr; }
        template<typename U>
        HRESULT As(ComPtr<U>*) { return E_FAIL; }
    private:
        T* ptr_;
    };
}}

#define IID_PPV_ARGS(pp) __uuidof(**(pp)), (void**)(pp)
#define __uuidof(x) GUID{}
inline HRESULT CreateDXGIFactory1(GUID, void**) { return E_FAIL; }
