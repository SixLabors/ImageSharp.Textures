// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

// ReSharper disable InconsistentNaming
namespace SixLabors.ImageSharp.Textures.Formats.Dds.Emums
{
    /// <summary>
    /// Resource data formats which includes fully-typed and typeless formats.
    /// </summary>
    /// <remarks>
    /// Values taken from http://msdn.microsoft.com/en-us/library/windows/desktop/bb173059%28v=vs.85%29.aspx
    /// </remarks>
    internal enum DxgiFormat : uint
    {
        Unknown = 0,

        /// <summary>
        /// A four-component, 128-bit typeless format that supports 32 bits per channel including alpha.
        /// </summary>
        R32G32B32A32_Typeless = 1,

        /// <summary>
        /// A four-component, 128-bit floating-point format that supports 32 bits per channel including alpha.
        /// </summary>
        R32G32B32A32_Float = 2,

        /// <summary>
        /// A four-component, 128-bit unsigned-integer format that supports 32 bits per channel including alpha.
        /// </summary>
        R32G32B32A32_UInt = 3,

        /// <summary>
        /// A four-component, 128-bit signed-integer format that supports 32 bits per channel including alpha.
        /// </summary>
        R32G32B32A32_SInt = 4,

        /// <summary>
        /// A three-component, 96-bit typeless format that supports 32 bits per color channel.
        /// </summary>
        R32G32B32_Typeless = 5,

        /// <summary>
        /// A three-component, 96-bit floating-point format that supports 32 bits per color channel.
        /// </summary>
        R32G32B32_Float = 6,

        /// <summary>
        /// A three-component, 96-bit unsigned-integer format that supports 32 bits per color channel.
        /// </summary>
        R32G32B32_UInt = 7,

        /// <summary>
        /// A three-component, 96-bit signed-integer format that supports 32 bits per color channel.
        /// </summary>
        R32G32B32_SInt = 8,

        /// <summary>
        /// A four-component, 64-bit typeless format that supports 16 bits per channel including alpha.
        /// </summary>
        R16G16B16A16_Typeless = 9,

        /// <summary>
        /// A four-component, 64-bit floating-point format that supports 16 bits per channel including alpha.
        /// </summary>
        R16G16B16A16_Float = 10,

        /// <summary>
        /// A four-component, 64-bit unsigned-normalized-integer format that supports 16 bits per channel including alpha.
        /// </summary>
        R16G16B16A16_UNorm = 11,

        /// <summary>
        /// A four-component, 64-bit unsigned-integer format that supports 16 bits per channel including alpha.
        /// </summary>
        R16G16B16A16_UInt = 12,

        /// <summary>
        /// A four-component, 64-bit signed-normalized-integer format that supports 16 bits per channel including alpha.
        /// </summary>
        R16G16B16A16_SNorm = 13,

        /// <summary>
        /// A four-component, 64-bit signed-integer format that supports 16 bits per channel including alpha.
        /// </summary>
        R16G16B16A16_SInt = 14,

        /// <summary>
        /// A two-component, 64-bit typeless format that supports 32 bits for the red channel and 32 bits for the green channel.
        /// </summary>
        R32G32_Typeless = 15,

        /// <summary>
        /// A two-component, 64-bit floating-point format that supports 32 bits for the red channel and 32 bits for the green channel.
        /// </summary>
        R32G32_Float = 16,

        /// <summary>
        /// A two-component, 64-bit unsigned-integer format that supports 32 bits for the red channel and 32 bits for the green channel.
        /// </summary>
        R32G32_UInt = 17,

        /// <summary>
        /// A two-component, 64-bit signed-integer format that supports 32 bits for the red channel and 32 bits for the green channel.
        /// </summary>
        R32G32_SInt = 18,

        /// <summary>
        /// A two-component, 64-bit typeless format that supports 32 bits for the red channel, 8 bits for the green channel, and 24 bits are unused.
        /// </summary>
        R32G8X24_Typeless = 19,

        /// <summary>
        /// A 32-bit floating-point component, and two unsigned-integer components (with an additional 32 bits). This format supports 32-bit depth, 8-bit stencil, and 24 bits are unused.
        /// </summary>
        D32_Float_S8X24_UInt = 20,

        /// <summary>
        /// A 32-bit floating-point component, and two typeless components (with an additional 32 bits). This format supports 32-bit red channel, 8 bits are unused, and 24 bits are unused.
        /// </summary>
        R32_Float_X8X24_Typeless = 21,

        /// <summary>
        /// A 32-bit typeless component, and two unsigned-integer components (with an additional 32 bits). This format has 32 bits unused, 8 bits for green channel, and 24 bits are unused.
        /// </summary>
        X32_Typeless_G8X24_UInt = 22,

        /// <summary>
        /// A four-component, 32-bit typeless format that supports 10 bits for each color and 2 bits for alpha.
        /// </summary>
        R10G10B10A2_Typeless = 23,

        /// <summary>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 10 bits for each color and 2 bits for alpha.
        /// </summary>
        R10G10B10A2_UNorm = 24,

        /// <summary>
        /// A four-component, 32-bit unsigned-integer format that supports 10 bits for each color and 2 bits for alpha.
        /// </summary>
        R10G10B10A2_UInt = 25,

        /// <summary>
        /// Three partial-precision floating-point numbers encoded into a single 32-bit value (a variant of s10e5, which is sign bit, 10-bit mantissa, and 5-bit biased (15) exponent).
        /// There are no sign bits, and there is a 5-bit biased (15) exponent for each channel, 6-bit mantissa for R and G, and a 5-bit mantissa for B.
        /// </summary>
        R11G11B10_Float = 26,

        /// <summary>
        /// A four-component, 32-bit typeless format that supports 8 bits per channel including alpha.
        /// </summary>
        R8G8B8A8_Typeless = 27,

        /// <summary>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits per channel including alpha.
        /// </summary>
        R8G8B8A8_UNorm = 28,

        /// <summary>
        /// A four-component, 32-bit unsigned-normalized integer sRGB format that supports 8 bits per channel including alpha.
        /// </summary>
        R8G8B8A8_UNorm_SRGB = 29,

        /// <summary>
        /// A four-component, 32-bit unsigned-integer format that supports 8 bits per channel including alpha.
        /// </summary>
        R8G8B8A8_UInt = 30,

        /// <summary>
        /// A four-component, 32-bit signed-normalized-integer format that supports 8 bits per channel including alpha.
        /// </summary>
        R8G8B8A8_SNorm = 31,

        /// <summary>
        /// A four-component, 32-bit signed-integer format that supports 8 bits per channel including alpha.
        /// </summary>
        R8G8B8A8_SInt = 32,

        /// <summary>
        /// A two-component, 32-bit typeless format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </summary>
        R16G16_Typeless = 33,

        /// <summary>
        /// A two-component, 32-bit floating-point format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </summary>
        R16G16_Float = 34,

        /// <summary>
        /// A two-component, 32-bit unsigned-normalized-integer format that supports 16 bits each for the green and red channels.
        /// </summary>
        R16G16_UNorm = 35,

        /// <summary>
        /// A two-component, 32-bit unsigned-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </summary>
        R16G16_UInt = 36,

        /// <summary>
        /// A two-component, 32-bit signed-normalized-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </summary>
        R16G16_SNorm = 37,

        /// <summary>
        /// A two-component, 32-bit signed-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
        /// </summary>
        R16G16_SInt = 38,

        /// <summary>
        /// A single-component, 32-bit typeless format that supports 32 bits for the red channel.
        /// </summary>
        R32_Typeless = 39,

        /// <summary>
        /// A single-component, 32-bit floating-point format that supports 32 bits for depth.
        /// </summary>
        D32_Float = 40,

        /// <summary>
        /// A single-component, 32-bit floating-point format that supports 32 bits for the red channel.
        /// </summary>
        R32_Float = 41,

        /// <summary>
        /// A single-component, 32-bit unsigned-integer format that supports 32 bits for the red channel.
        /// </summary>
        R32_UInt = 42,

        /// <summary>
        /// A single-component, 32-bit signed-integer format that supports 32 bits for the red channel.
        /// </summary>
        R32_SInt = 43,

        /// <summary>
        /// A two-component, 32-bit typeless format that supports 24 bits for the red channel and 8 bits for the green channel.
        /// </summary>
        R24G8_Typeless = 44,

        /// <summary>
        /// A 32-bit z-buffer format that supports 24 bits for depth and 8 bits for stencil.
        /// </summary>
        D24_UNorm_S8_UInt = 45,

        /// <summary>
        /// A 32-bit format, that contains a 24 bit, single-component, unsigned-normalized integer, with an additional typeless 8 bits. This format has 24 bits red channel and 8 bits unused.
        /// </summary>
        R24_UNorm_X8_Typeless = 46,

        /// <summary>
        /// A 32-bit format, that contains a 24 bit, single-component, typeless format, with an additional 8 bit unsigned integer component. This format has 24 bits unused and 8 bits green channel.
        /// </summary>
        X24_Typeless_G8_UInt = 47,

        /// <summary>
        /// A two-component, 16-bit typeless format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </summary>
        R8G8_Typeless = 48,

        /// <summary>
        /// A two-component, 16-bit unsigned-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </summary>
        R8G8_UNorm = 49,

        /// <summary>
        /// A two-component, 16-bit unsigned-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </summary>
        R8G8_UInt = 50,

        /// <summary>
        /// A two-component, 16-bit signed-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </summary>
        R8G8_SNorm = 51,

        /// <summary>
        /// A two-component, 16-bit signed-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
        /// </summary>
        R8G8_SInt = 52,

        /// <summary>
        /// A single-component, 16-bit typeless format that supports 16 bits for the red channel.
        /// </summary>
        R16_Typeless = 53,

        /// <summary>
        /// A single-component, 16-bit floating-point format that supports 16 bits for the red channel.
        /// </summary>
        R16_Float = 54,

        /// <summary>
        /// A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for depth.
        /// </summary>
        D16_UNorm = 55,

        /// <summary>
        /// A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for the red channel.
        /// </summary>
        R16_UNorm = 56,

        /// <summary>
        /// A single-component, 16-bit unsigned-integer format that supports 16 bits for the red channel.
        /// </summary>
        R16_UInt = 57,

        /// <summary>
        /// A single-component, 16-bit signed-normalized-integer format that supports 16 bits for the red channel.
        /// </summary>
        R16_SNorm = 58,

        /// <summary>
        /// A single-component, 16-bit signed-integer format that supports 16 bits for the red channel.
        /// </summary>
        R16_SInt = 59,

        /// <summary>
        /// A single-component, 8-bit typeless format that supports 8 bits for the red channel.
        /// </summary>
        R8_Typeless = 60,

        /// <summary>
        /// A single-component, 8-bit unsigned-normalized-integer format that supports 8 bits for the red channel.
        /// </summary>
        R8_UNorm = 61,

        /// <summary>
        /// A single-component, 8-bit unsigned-integer format that supports 8 bits for the red channel.
        /// </summary>
        R8_UInt = 62,

        /// <summary>
        /// A single-component, 8-bit signed-normalized-integer format that supports 8 bits for the red channel.
        /// </summary>
        R8_SNorm = 63,

        /// <summary>
        /// A single-component, 8-bit signed-integer format that supports 8 bits for the red channel.
        /// </summary>
        R8_SInt = 64,

        /// <summary>
        /// A single-component, 8-bit unsigned-normalized-integer format for alpha only.
        /// </summary>
        A8_UNorm = 65,

        /// <summary>
        /// A single-component, 1-bit unsigned-normalized integer format that supports 1 bit for the red channel.
        /// </summary>
        R1_UNorm = 66,

        /// <summary>
        /// Three partial-precision floating-point numbers encoded into a single 32-bit value all sharing the same 5-bit exponent (variant of s10e5, which is sign bit, 10-bit mantissa, and 5-bit biased (15) exponent).
        /// There is no sign bit, and there is a shared 5-bit biased (15) exponent and a 9-bit mantissa for each channel.
        /// </summary>
        R9G9B9E5_SharedExp = 67,

        /// <summary>
        /// A four-component, 32-bit unsigned-normalized-integer format. This packed RGB format is analogous to the UYVY format. Each 32-bit block describes a pair of pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated,
        /// and the G8 values are unique to each pixel. Width must be even.
        /// </summary>
        R8G8_B8G8_UNorm = 68,

        /// <summary>
        /// A four-component, 32-bit unsigned-normalized-integer format. This packed RGB format is analogous to the YUY2 format. Each 32-bit block describes a pair of pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated,
        /// and the G8 values are unique to each pixel. Width must be even.
        /// </summary>
        G8R8_G8B8_UNorm = 69,

        /// <summary>
        /// Four-component typeless block-compression format.
        /// </summary>
        BC1_Typeless = 70,

        /// <summary>
        /// Four-component block-compression format.
        /// </summary>
        BC1_UNorm = 71,

        /// <summary>
        /// Four-component block-compression format for sRGB data.
        /// </summary>
        BC1_UNorm_SRGB = 72,

        /// <summary>
        /// Four-component typeless block-compression format.
        /// </summary>
        BC2_Typeless = 73,

        /// <summary>
        /// Four-component block-compression format.
        /// </summary>
        BC2_UNorm = 74,

        /// <summary>
        /// Four-component block-compression format for sRGB data.
        /// </summary>
        BC2_UNorm_SRGB = 75,

        /// <summary>
        /// Four-component typeless block-compression format.
        /// </summary>
        BC3_Typeless = 76,

        /// <summary>
        /// Four-component block-compression format.
        /// </summary>
        BC3_UNorm = 77,

        /// <summary>
        /// Four-component block-compression format for sRGB data.
        /// </summary>
        BC3_UNorm_SRGB = 78,

        /// <summary>
        /// One-component typeless block-compression format.
        /// </summary>
        BC4_Typeless = 79,

        /// <summary>
        /// One-component block-compression format.
        /// </summary>
        BC4_UNorm = 80,

        /// <summary>
        /// One-component block-compression format.
        /// </summary>
        BC4_SNorm = 81,

        /// <summary>
        /// Two-component typeless block-compression format.
        /// </summary>
        BC5_Typeless = 82,

        /// <summary>
        /// Two-component block-compression format.
        /// </summary>
        BC5_UNorm = 83,

        /// <summary>
        /// Two-component block-compression format.
        /// </summary>
        BC5_SNorm = 84,

        /// <summary>
        /// A three-component, 16-bit unsigned-normalized-integer format that supports 5 bits for blue, 6 bits for green, and 5 bits for red.
        /// Direct3D 10 through Direct3D 11:  This value is defined for DXGI. However, Direct3D 10, 10.1, or 11 devices do not support this format.
        /// Direct3D 11.1:  This value is not supported until Windows 8.
        /// </summary>
        B5G6R5_UNorm = 85,

        /// <summary>
        /// A four-component, 16-bit unsigned-normalized-integer format that supports 5 bits for each color channel and 1-bit alpha.
        /// Direct3D 10 through Direct3D 11:  This value is defined for DXGI. However, Direct3D 10, 10.1, or 11 devices do not support this format.
        /// Direct3D 11.1:  This value is not supported until Windows 8.
        /// </summary>
        B5G5R5A1_UNorm = 86,

        /// <summary>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8-bit alpha.
        /// </summary>
        B8G8R8A8_UNorm = 87,

        /// <summary>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8 bits unused.
        /// </summary>
        B8G8R8X8_UNorm = 88,

        /// <summary>
        /// A four-component, 32-bit 2.8-biased fixed-point format that supports 10 bits for each color channel and 2-bit alpha.
        /// </summary>
        R10G10B10_XR_BIAS_A2_UNorm = 89,

        /// <summary>
        /// A four-component, 32-bit typeless format that supports 8 bits for each channel including alpha.
        /// </summary>
        B8G8R8A8_Typeless = 90,

        /// <summary>
        /// A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each channel including alpha.
        /// </summary>
        B8G8R8A8_UNorm_SRGB = 91,

        /// <summary>
        /// A four-component, 32-bit typeless format that supports 8 bits for each color channel, and 8 bits are unused.
        /// </summary>
        B8G8R8X8_Typeless = 92,

        /// <summary>
        /// A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each color channel, and 8 bits are unused.
        /// </summary>
        B8G8R8X8_UNorm_SRGB = 93,

        /// <summary>
        /// A typeless block-compression format.
        /// </summary>
        BC6H_Typeless = 94,

        /// <summary>
        /// A block-compression format.
        /// </summary>
        BC6H_UF16 = 95,

        /// <summary>
        /// A block-compression format.
        /// </summary>
        BC6H_SF16 = 96,

        /// <summary>
        /// A typeless block-compression format.
        /// </summary>
        BC7_Typeless = 97,

        /// <summary>
        /// A block-compression format.
        /// </summary>
        BC7_UNorm = 98,

        /// <summary>
        /// A block-compression format.
        /// </summary>
        BC7_UNorm_SRGB = 99,

        /// <summary>
        /// Most common YUV 4:4:4 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT.
        /// For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT.
        /// By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT.
        /// Supported view types are SRV, RTV, and UAV. One view provides a straightforward mapping of the entire surface.
        /// The mapping to the view channel is V->R8, U->G8, Y->B8, and A->A8.
        /// </summary>
        AYUV = 100,

        /// <summary>
        /// 10-bit per channel packed YUV 4:4:4 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R10G10B10A2_UNORM and DXGI_FORMAT_R10G10B10A2_UINT.
        /// For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT.
        /// By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R10G10B10A2_UNORM and DXGI_FORMAT_R10G10B10A2_UINT.
        /// Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface.
        /// The mapping to the view channel is U->R10, Y->G10, V->B10, and A->A2.
        /// </summary>
        Y410 = 101,

        /// <summary>
        /// 16-bit per channel packed YUV 4:4:4 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT.
        /// Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface.
        /// The mapping to the view channel is U->R16, Y->G16, V->B16, and A->A16.
        /// </summary>
        Y416 = 102,

        /// <summary>
        /// Most common YUV 4:2:0 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R8_UNORM and DXGI_FORMAT_R8_UINT.
        /// Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format are DXGI_FORMAT_R8G8_UNORM and DXGI_FORMAT_R8G8_UINT.
        /// Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y->R8.
        /// For chrominance data view, the mapping to the view channel is U->R8 and V->G8.
        /// </summary>
        NV12 = 103,

        /// <summary>
        /// 10-bit per channel planar YUV 4:2:0 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R16_UNORM and DXGI_FORMAT_R16_UINT.
        /// The runtime does not enforce whether the lowest 6 bits are 0 (given that this video resource format is a 10-bit format that uses 16 bits).
        /// If required, application shader code would have to enforce this manually. From the runtime's point of view, DXGI_FORMAT_P010 is no different than DXGI_FORMAT_P016.
        /// Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format are DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT.
        /// For UAVs, an additional valid chrominance data view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y->R16.
        /// For chrominance data view, the mapping to the view channel is U->R16 and V->G16.
        /// </summary>
        P010 = 104,

        /// <summary>
        /// 16-bit per channel planar YUV 4:2:0 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R16_UNORM and DXGI_FORMAT_R16_UINT.
        /// Valid chrominance data view formats (width and height are each 1/2 of luminance view) for this video resource format are DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT.
        /// For UAVs, an additional valid chrominance data view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R16G16_UNORM and DXGI_FORMAT_R16G16_UINT. Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y->R16.
        /// For chrominance data view, the mapping to the view channel is U->R16 and V->G16.
        /// </summary>
        P016 = 105,

        /// <summary>
        /// 8-bit per channel planar YUV 4:2:0 video resource format. This format is subsampled where each pixel has its own Y value, but each 2x2 pixel block shares a single U and V value.
        /// The runtime requires that the width and height of all resources that are created with this format are multiples of 2. The runtime also requires that the left, right, top, and bottom members of any RECT that are used for this format are multiples of 2.
        /// This format differs from DXGI_FORMAT_NV12 in that the layout of the data within the resource is completely opaque to applications.
        /// Applications cannot use the CPU to map the resource and then access the data within the resource. You cannot use shaders with this format.
        /// Because of this behavior, legacy hardware that supports a non-NV12 4:2:0 layout (for example, YV12, and so on) can be used.
        /// Also, new hardware that has a 4:2:0 implementation better than NV12 can be used when the application does not need the data to be in a standard layout.
        /// </summary>
        Opaque_420 = 106,

        /// <summary>
        /// Most common YUV 4:2:2 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT.
        /// For UAVs, an additional valid view format is DXGI_FORMAT_R32_UINT. By using DXGI_FORMAT_R32_UINT for UAVs, you can both read and write as opposed to just write for DXGI_FORMAT_R8G8B8A8_UNORM and DXGI_FORMAT_R8G8B8A8_UINT.
        /// Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0->R8, U0->G8,Y1->B8,and V0->A8.
        /// </summary>
        YUY2 = 107,

        /// <summary>
        /// 10-bit per channel packed YUV 4:2:2 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT.
        /// The runtime does not enforce whether the lowest 6 bits are 0 (given that this video resource format is a 10-bit format that uses 16 bits).
        /// If required, application shader code would have to enforce this manually. From the runtime's point of view, DXGI_FORMAT_Y210 is no different than DXGI_FORMAT_Y216.
        /// Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0->R16, U->G16, Y1->B16, and V->A16.
        /// </summary>
        Y210 = 108,

        /// <summary>
        /// 16-bit per channel packed YUV 4:2:2 video resource format. Valid view formats for this video resource format are DXGI_FORMAT_R16G16B16A16_UNORM and DXGI_FORMAT_R16G16B16A16_UINT.
        /// Supported view types are SRV and UAV. One view provides a straightforward mapping of the entire surface. The mapping to the view channel is Y0->R16, U->G16, Y1->B16, and V->A16.
        /// </summary>
        Y216 = 109,

        /// <summary>
        /// Most common planar YUV 4:1:1 video resource format. Valid luminance data view formats for this video resource format are DXGI_FORMAT_R8_UNORM and DXGI_FORMAT_R8_UINT.
        /// Valid chrominance data view formats (width and height are each 1/4 of luminance view) for this video resource format are DXGI_FORMAT_R8G8_UNORM and DXGI_FORMAT_R8G8_UINT.
        /// Supported view types are SRV, RTV, and UAV. For luminance data view, the mapping to the view channel is Y->R8. For chrominance data view, the mapping to the view channel is U->R8 and V->G8.
        /// </summary>
        NV11 = 110,

        /// <summary>
        /// 4-bit palletized YUV format that is commonly used for DVD subpicture.
        /// </summary>
        AI44 = 111,

        /// <summary>
        /// 4-bit palletized YUV format that is commonly used for DVD subpicture.
        /// </summary>
        IA44 = 112,

        /// <summary>
        /// 8-bit palletized format that is used for palletized RGB data when the processor processes ISDB-T data and for palletized YUV data when the processor processes BluRay data.
        /// </summary>
        P8 = 113,

        /// <summary>
        /// 8-bit palletized format with 8 bits of alpha that is used for palletized YUV data when the processor processes BluRay data.
        /// </summary>
        A8P8 = 114,

        /// <summary>
        /// A four-component, 16-bit unsigned-normalized integer format that supports 4 bits for each channel including alpha.
        /// </summary>
        B4G4R4A4_UNorm = 115,

        /// <summary>
        /// A video format; an 8-bit version of a hybrid planar 4:2:2 format.
        /// </summary>
        P208 = 130,

        /// <summary>
        /// An 8 bit YCbCrA 4:4 rendering format.
        /// </summary>
        V208 = 131,

        /// <summary>
        /// An 8 bit YCbCrA 4:4:4:4 rendering format.
        /// </summary>
        V408 = 132,
    }
}
