#pragma once
#include <string>
#include <vector>
#include <memory>

namespace hdrfixer::fixes {

enum class FixCategory {
    ToneCurve,
    SdrBrightness,
    PixelFormat,
    AutoHdr,
    IccConflict,
    EdidValidation,
    OledProtection
};

enum class FixState {
    NotApplied,
    Applied,
    Error,
    NotNeeded
};

struct FixResult {
    bool success;
    std::string message;
};

struct FixStatus {
    FixState state;
    std::string message;
};

struct IFix {
    virtual ~IFix() = default;
    virtual std::string name() const = 0;
    virtual std::string description() const = 0;
    virtual FixCategory category() const = 0;
    virtual FixResult apply() = 0;
    virtual FixResult revert() = 0;
    virtual FixStatus diagnose() = 0;
};

class FixEngine {
public:
    void register_fix(std::unique_ptr<IFix> fix);
    size_t fix_count() const;
    void apply_all();
    void revert_all();
    std::vector<FixStatus> diagnose_all();
    IFix* get_fix(const std::string& name);

private:
    std::vector<std::unique_ptr<IFix>> fixes_;
};

} // namespace hdrfixer::fixes
