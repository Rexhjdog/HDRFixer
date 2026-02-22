using System.Runtime.InteropServices;
using Vortice.DXGI;
using static Vortice.DXGI.DXGI;

namespace HDRFixer.Core.Display;

public class DxgiDisplayDetector : IDisplayDetector
{
    public List<DisplayInfo> DetectDisplays()
    {
        var displays = new List<DisplayInfo>();
        var configPaths = QueryConfigPaths();

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

                var info = new DisplayInfo
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
                    GpuName = adapterDesc.Description,
                    AdapterLuidLow = adapterDesc.Luid.LowPart,
                    AdapterLuidHigh = adapterDesc.Luid.HighPart
                };

                // Match with DisplayConfig path
                var match = configPaths.FirstOrDefault(p => p.DeviceName == info.DeviceName);
                if (match != null)
                {
                    info.MonitorName = match.MonitorFriendlyName;
                    info.SourceId = match.SourceId;
                    info.TargetId = match.TargetId;
                    info.SdrWhiteLevelNits = match.SdrWhiteLevel;
                }

                displays.Add(info);
            }
        }
        return displays;
    }

    private class ConfigPathInfo
    {
        public string DeviceName { get; set; } = string.Empty;
        public string MonitorFriendlyName { get; set; } = string.Empty;
        public uint SourceId { get; set; }
        public uint TargetId { get; set; }
        public float SdrWhiteLevel { get; set; }
    }

    private List<ConfigPathInfo> QueryConfigPaths()
    {
        var results = new List<ConfigPathInfo>();
        int error = DisplayConfigNativeMethods.GetDisplayConfigBufferSizes(DisplayConfigNativeMethods.QDC_ONLY_ACTIVE_PATHS, out uint pathCount, out uint modeCount);
        if (error != 0) return results;

        var paths = new DisplayConfigNativeMethods.DISPLAYCONFIG_PATH_INFO[pathCount];
        error = DisplayConfigNativeMethods.QueryDisplayConfig(DisplayConfigNativeMethods.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, IntPtr.Zero, IntPtr.Zero);
        if (error != 0) return results;

        for (int i = 0; i < pathCount; i++)
        {
            var sourceName = new DisplayConfigNativeMethods.DISPLAYCONFIG_SOURCE_DEVICE_NAME
            {
                header = new DisplayConfigNativeMethods.DISPLAYCONFIG_DEVICE_INFO_HEADER
                {
                    type = DisplayConfigNativeMethods.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME,
                    size = (uint)Marshal.SizeOf<DisplayConfigNativeMethods.DISPLAYCONFIG_SOURCE_DEVICE_NAME>(),
                    adapterId = paths[i].sourceInfo.adapterId,
                    id = paths[i].sourceInfo.id
                }
            };

            var targetName = new DisplayConfigNativeMethods.DISPLAYCONFIG_TARGET_DEVICE_NAME
            {
                header = new DisplayConfigNativeMethods.DISPLAYCONFIG_DEVICE_INFO_HEADER
                {
                    type = DisplayConfigNativeMethods.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME,
                    size = (uint)Marshal.SizeOf<DisplayConfigNativeMethods.DISPLAYCONFIG_TARGET_DEVICE_NAME>(),
                    adapterId = paths[i].targetInfo.adapterId,
                    id = paths[i].targetInfo.id
                }
            };

            var whiteLevel = new DisplayConfigNativeMethods.DISPLAYCONFIG_SDR_WHITE_LEVEL
            {
                header = new DisplayConfigNativeMethods.DISPLAYCONFIG_DEVICE_INFO_HEADER
                {
                    type = DisplayConfigNativeMethods.DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL,
                    size = (uint)Marshal.SizeOf<DisplayConfigNativeMethods.DISPLAYCONFIG_SDR_WHITE_LEVEL>(),
                    adapterId = paths[i].targetInfo.adapterId,
                    id = paths[i].targetInfo.id
                }
            };

            var pathInfo = new ConfigPathInfo { SourceId = paths[i].sourceInfo.id, TargetId = paths[i].targetInfo.id };

            if (DisplayConfigNativeMethods.DisplayConfigGetDeviceInfo(ref sourceName) == 0)
                pathInfo.DeviceName = sourceName.viewGdiDeviceName;

            if (DisplayConfigNativeMethods.DisplayConfigGetDeviceInfo(ref targetName) == 0)
                pathInfo.MonitorFriendlyName = targetName.monitorFriendlyDeviceName;

            if (DisplayConfigNativeMethods.DisplayConfigGetDeviceInfo(ref whiteLevel) == 0)
                pathInfo.SdrWhiteLevel = (whiteLevel.SDRWhiteLevel / 1000f) * 80f;

            results.Add(pathInfo);
        }

        return results;
    }
}
