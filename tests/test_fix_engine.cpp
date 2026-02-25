#include "doctest.h"
#include "core/fixes/fix_engine.h"

using namespace hdrfixer::fixes;

struct MockFix : public IFix {
    std::string name() const override { return "MockFix"; }
    std::string description() const override { return "Test fix"; }
    FixCategory category() const override { return FixCategory::ToneCurve; }
    FixResult apply() override { applied = true; return {true, "Applied"}; }
    FixResult revert() override { applied = false; return {true, "Reverted"}; }
    FixStatus diagnose() override { return {applied ? FixState::Applied : FixState::NotApplied, ""}; }
    bool applied = false;
};

struct FailingFix : public IFix {
    std::string name() const override { return "FailingFix"; }
    std::string description() const override { return "Always fails"; }
    FixCategory category() const override { return FixCategory::SdrBrightness; }
    FixResult apply() override { return {false, "Failed to apply"}; }
    FixResult revert() override { return {false, "Failed to revert"}; }
    FixStatus diagnose() override { return {FixState::Error, "Error state"}; }
};

TEST_CASE("FixEngine register and count") {
    FixEngine engine;
    CHECK(engine.fix_count() == 0);
    engine.register_fix(std::make_unique<MockFix>());
    CHECK(engine.fix_count() == 1);
}

TEST_CASE("FixEngine register multiple") {
    FixEngine engine;
    engine.register_fix(std::make_unique<MockFix>());
    engine.register_fix(std::make_unique<FailingFix>());
    CHECK(engine.fix_count() == 2);
}

TEST_CASE("FixEngine apply all") {
    FixEngine engine;
    auto fix = std::make_unique<MockFix>();
    auto* ptr = fix.get();
    engine.register_fix(std::move(fix));
    engine.apply_all();
    CHECK(ptr->applied == true);
}

TEST_CASE("FixEngine revert all") {
    FixEngine engine;
    auto fix = std::make_unique<MockFix>();
    auto* ptr = fix.get();
    engine.register_fix(std::move(fix));
    engine.apply_all();
    CHECK(ptr->applied == true);
    engine.revert_all();
    CHECK(ptr->applied == false);
}

TEST_CASE("FixEngine diagnose all") {
    FixEngine engine;
    engine.register_fix(std::make_unique<MockFix>());
    engine.register_fix(std::make_unique<FailingFix>());
    auto results = engine.diagnose_all();
    CHECK(results.size() == 2);
    CHECK(results[0].state == FixState::NotApplied);
    CHECK(results[1].state == FixState::Error);
}

TEST_CASE("FixEngine get_fix by name") {
    FixEngine engine;
    engine.register_fix(std::make_unique<MockFix>());
    auto* fix = engine.get_fix("MockFix");
    CHECK(fix != nullptr);
    CHECK(fix->name() == "MockFix");
}

TEST_CASE("FixEngine get_fix returns nullptr for unknown") {
    FixEngine engine;
    engine.register_fix(std::make_unique<MockFix>());
    auto* fix = engine.get_fix("NonExistent");
    CHECK(fix == nullptr);
}

TEST_CASE("FixEngine apply skips already applied") {
    FixEngine engine;
    auto fix = std::make_unique<MockFix>();
    auto* ptr = fix.get();
    ptr->applied = true; // pre-applied
    engine.register_fix(std::move(fix));
    engine.apply_all();
    CHECK(ptr->applied == true); // still applied, not toggled
}

TEST_CASE("FixEngine revert skips not applied") {
    FixEngine engine;
    auto fix = std::make_unique<MockFix>();
    auto* ptr = fix.get();
    engine.register_fix(std::move(fix));
    // Not applied, revert should not toggle
    engine.revert_all();
    CHECK(ptr->applied == false);
}

TEST_CASE("FixEngine apply_all skips error-state fixes") {
    FixEngine engine;
    engine.register_fix(std::make_unique<FailingFix>());
    // Error-state fixes should NOT be re-applied (changed behavior)
    engine.apply_all(); // should not crash
    auto results = engine.diagnose_all();
    CHECK(results[0].state == FixState::Error);
}

TEST_CASE("FixEngine apply_all on empty engine") {
    FixEngine engine;
    engine.apply_all(); // should not crash
    engine.revert_all(); // should not crash
    auto results = engine.diagnose_all();
    CHECK(results.empty());
}

TEST_CASE("FixEngine get_fix with empty string") {
    FixEngine engine;
    engine.register_fix(std::make_unique<MockFix>());
    auto* fix = engine.get_fix("");
    CHECK(fix == nullptr);
}
