using Vortice.DXGI;
using static Vortice.DXGI.DXGI;

namespace HDRFixer.Core.Display;

public class DxgiDisplayDetector : IDisplayDetector
{
    public List<DisplayInfo> DetectDisplays()
    {
        var displays = new List<DisplayInfo>();
        using IDXGIFactory2 factory = CreateDXGIFactory1<IDXGIFactory2>();

        for (int adapterIndex = 0;
             factory.EnumAdapters1(adapterIndex, out IDXGIAdapter1? adapter).Success;
             adapterIndex++)
        {
            using var a = adapter!;
            var adapterDesc = a.Description1;
            if ((adapterDesc.Flags & AdapterFlags.Software) != AdapterFlags.None) continue;

            var gpuVendor = (uint)adapterDesc.VendorId switch
            {
                0x10DE => GpuVendor.Nvidia,
                0x1002 => GpuVendor.Amd,
                0x8086 => GpuVendor.Intel,
                _ => GpuVendor.Unknown
            };

            for (int outputIndex = 0;
                 a.EnumOutputs(outputIndex, out IDXGIOutput? output).Success;
                 outputIndex++)
            {
                using var o = output!;
                using var output6 = o.QueryInterfaceOrNull<IDXGIOutput6>();
                if (output6 == null) continue;

                var desc = output6.Description1;
                bool isHdr = desc.ColorSpace == ColorSpaceType.RgbFullG2084NoneP2020;

                displays.Add(new DisplayInfo
                {
                    DeviceName = desc.DeviceName,
                    IsHdrEnabled = isHdr,
                    BitsPerColor = (uint)desc.BitsPerColor,
                    MinLuminance = desc.MinLuminance,
                    MaxLuminance = desc.MaxLuminance,
                    MaxFullFrameLuminance = desc.MaxFullFrameLuminance,
                    RedPrimary = (desc.RedPrimary[0], desc.RedPrimary[1]),
                    GreenPrimary = (desc.GreenPrimary[0], desc.GreenPrimary[1]),
                    BluePrimary = (desc.BluePrimary[0], desc.BluePrimary[1]),
                    WhitePoint = (desc.WhitePoint[0], desc.WhitePoint[1]),
                    GpuVendor = gpuVendor,
                    GpuName = adapterDesc.Description
                });
            }
        }
        return displays;
    }
}
