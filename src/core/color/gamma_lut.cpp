#include "gamma_lut.h"
#include "transfer_functions.h"

namespace hdrfixer::color {

std::vector<double> generate_sdr_lut(int size) {
    std::vector<double> lut(size);
    for (int i = 0; i < size; ++i) {
        double input = static_cast<double>(i) / (size - 1);
        double linear = srgb_eotf(input);
        lut[i] = gamma_inv_eotf(linear, 2.2);
    }
    return lut;
}

std::vector<double> generate_hdr_lut(int size, double white_nits, double black_nits) {
    std::vector<double> lut(size);
    for (int i = 0; i < size; ++i) {
        double pq_input = static_cast<double>(i) / (size - 1);
        double nits = pq_eotf(pq_input);

        if (nits > white_nits) {
            lut[i] = pq_input; // passthrough above SDR range
            continue;
        }

        double normalized = (white_nits > 0.0) ? nits / white_nits : 0.0;
        double srgb_signal = srgb_inv_eotf(normalized);
        double corrected_nits = (white_nits - black_nits) * std::pow(srgb_signal, 2.2) + black_nits;
        lut[i] = pq_inv_eotf(corrected_nits);
    }
    return lut;
}

} // namespace hdrfixer::color
