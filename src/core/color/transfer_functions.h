#pragma once
#include <cmath>
#include <algorithm>

namespace hdrfixer::color {

// sRGB IEC 61966-2-1
inline constexpr double kSrgbLinearThreshold = 0.04045;
inline constexpr double kSrgbLinearScale = 12.92;
inline constexpr double kSrgbGammaOffset = 0.055;
inline constexpr double kSrgbGammaBase = 1.055;
inline constexpr double kSrgbGammaExponent = 2.4;
inline constexpr double kSrgbInvLinearThreshold = 0.0031308;

// PQ ST 2084
inline constexpr double kPqM1 = 2610.0 / 16384.0;
inline constexpr double kPqM2 = 128.0 * 2523.0 / 4096.0;
inline constexpr double kPqC1 = 3424.0 / 4096.0;
inline constexpr double kPqC2 = 32.0 * 2413.0 / 4096.0;
inline constexpr double kPqC3 = 32.0 * 2392.0 / 4096.0;
inline constexpr double kPqMaxNits = 10000.0;

double srgb_eotf(double v);
double srgb_inv_eotf(double l);
double pq_eotf(double v);       // returns nits
double pq_inv_eotf(double nits); // returns PQ signal
double gamma_eotf(double v, double gamma);
double gamma_inv_eotf(double l, double gamma);

} // namespace hdrfixer::color
