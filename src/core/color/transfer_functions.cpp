#include "transfer_functions.h"

namespace hdrfixer::color {

double srgb_eotf(double v) {
    v = std::clamp(v, 0.0, 1.0);
    if (v <= kSrgbLinearThreshold)
        return v / kSrgbLinearScale;
    return std::pow((v + kSrgbGammaOffset) / kSrgbGammaBase, kSrgbGammaExponent);
}

double srgb_inv_eotf(double l) {
    l = std::max(l, 0.0);
    if (l <= kSrgbInvLinearThreshold)
        return l * kSrgbLinearScale;
    return std::clamp(kSrgbGammaBase * std::pow(l, 1.0 / kSrgbGammaExponent) - kSrgbGammaOffset, 0.0, 1.0);
}

double pq_eotf(double v) {
    v = std::clamp(v, 0.0, 1.0);
    double vp = std::pow(v, 1.0 / kPqM2);
    double num = std::max(vp - kPqC1, 0.0);
    double den = kPqC2 - kPqC3 * vp;
    if (den <= 0.0) return 0.0;
    return kPqMaxNits * std::pow(num / den, 1.0 / kPqM1);
}

double pq_inv_eotf(double nits) {
    nits = std::clamp(nits, 0.0, kPqMaxNits);
    double y = std::pow(nits / kPqMaxNits, kPqM1);
    return std::pow((kPqC1 + kPqC2 * y) / (1.0 + kPqC3 * y), kPqM2);
}

double gamma_eotf(double v, double gamma) {
    if (v < 0.0) return 0.0;
    return std::pow(v, gamma);
}

double gamma_inv_eotf(double l, double gamma) {
    if (l < 0.0) return 0.0;
    return std::pow(l, 1.0 / gamma);
}

} // namespace hdrfixer::color
