function Cubemap-NvDxt {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    Write-Output "`nCubemap-NvDxt - Processing`n"

    .(Join-Path $PSScriptRoot nvdxt.exe) -list faces.lst -cubeMap -output (Join-Path $SaveFolder "cubemap-mips.dds")
    .(Join-Path $PSScriptRoot nvdxt.exe) -list faces.lst -cubeMap -nomipmap -output (Join-Path $SaveFolder "cubemap-no-mips.dds")

}

function Flat-NvDxt {

    Param ([string]$LoadFolder, [string]$SaveFolder, [string]$Format)

    Push-Location $LoadFolder

    Write-Output "`nFlat-NvDxt - Processing Format $Format`n"

    return

    .(Join-Path $PSScriptRoot nvdxt.exe) -file flat-pot-alpha-8.png -$Format -output (Join-Path $SaveFolder "flat-pot-alpha-mips-$Format.dds")
    .(Join-Path $PSScriptRoot nvdxt.exe) -file flat-pot-alpha.png -$Format -nomipmap -output (Join-Path $SaveFolder "flat-pot-alpha-no-mips-$Format.dds")
    .(Join-Path $PSScriptRoot nvdxt.exe) -file flat-pot-no-alpha.png -$Format -output (Join-Path $SaveFolder "flat-pot-no-alpha-mips-$Format.dds")
    .(Join-Path $PSScriptRoot nvdxt.exe) -file flat-pot-no-alpha.png -$Format -nomipmap -output (Join-Path $SaveFolder "flat-pot-no-alpha-no-mips-$Format.dds")

}

function Flat-TexConv {

    Param ([string]$LoadFolder, [string]$SaveFolder, [string]$Format)

    Push-Location $LoadFolder

    Write-Output "`nFlat-TexConv - Processing Format $Format`n"

    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-$Format" -ft dds "flat-pot-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-$Format" -ft dds "flat- pot-no-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-$Format" -ft dds "flat-rect-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-$Format" -ft dds "flat-rect-no-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-$Format" -ft dds "flat-square-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-$Format" -ft dds "flat-square-no-alpha.png"

    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-dx10-$Format" -ft dds -dx10 "flat-pot-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-dx10-$Format" -ft dds -dx10 "flat-pot-no-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-dx10-$Format" -ft dds -dx10 "flat-rect-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-dx10-$Format" -ft dds -dx10 "flat-rect-no-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-dx10-$Format" -ft dds -dx10 "flat-square-alpha.png"
    .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx "-dx10-$Format" -ft dds -dx10 "flat-square-no-alpha.png"

}

function Volume-NvDxt {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    Write-Output "`nVolume-NvDxt - Processing`n"

    .(Join-Path $PSScriptRoot nvdxt.exe) -list slices.lst -volumeMap -output (Join-Path $SaveFolder "volume-mips.dds")
    .(Join-Path $PSScriptRoot nvdxt.exe) -list slices.lst -volumeMap -nomipmap -output (Join-Path $SaveFolder "volume-no-mips.dds")

}

Push-Location $PSScriptRoot

try {

    $CubemapLoadFolder = Resolve-Path -Path ..\Images\Baseline\Cubemap
    $CubemapDdsSaveFolder = Resolve-Path -Path ..\Images\Input\Dds\Cubemap

    $FlatLoadFolder = Resolve-Path -Path ..\Images\Baseline\Flat
    $FlatDdsSaveFolder = Resolve-Path -Path ..\Images\Input\Dds\Flat

    $VolumeLoadFolder = Resolve-Path -Path ..\Images\Baseline\Volume
    $VolumeDdsSaveFolder = Resolve-Path -Path ..\Images\Input\Dds\Volume

    Cubemap-NvDxt -LoadFolder $CubemapLoadFolder -SaveFolder $CubemapDdsSaveFolder

    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format dxt1c
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format dxt1a
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format dxt3
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format dxt5
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format u1555
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format u4444
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format u565
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format u8888
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format u888
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format u555
    # Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format p8c
    # Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format p8a
    # Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format p4c
    # Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format p4a
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format a8
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format cxv8u8
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format v8u8
    # Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format v16u16
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format A8L8
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format fp32x4
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format fp32
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format fp16x4
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format dxt5nm
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format 3Dc
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format g16r16
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format g16r16f

    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32G32B32A32_FLOAT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32G32B32A32_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32G32B32A32_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32G32B32_FLOAT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32G32B32_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32G32B32_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16B16A16_FLOAT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16B16A16_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16B16A16_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16B16A16_SNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16B16A16_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32G32_FLOAT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32G32_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32G32_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R10G10B10A2_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R10G10B10A2_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R11G11B10_FLOAT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8B8A8_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8B8A8_UNORM_SRGB
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8B8A8_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8B8A8_SNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8B8A8_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16_FLOAT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16_SNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16G16_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32_FLOAT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R32_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8_SNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16_FLOAT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16_SNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R16_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8_UINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8_SNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8_SINT
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format A8_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R9G9B9E5_SHAREDEXP
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R8G8_B8G8_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format G8R8_G8B8_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC1_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC1_UNORM_SRGB
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC2_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC2_UNORM_SRGB
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC3_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC3_UNORM_SRGB
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC4_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC4_SNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC5_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC5_SNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format B5G6R5_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format B5G5R5A1_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format B8G8R8A8_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format B8G8R8X8_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format R10G10B10_XR_BIAS_A2_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format B8G8R8A8_UNORM_SRGB
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format B8G8R8X8_UNORM_SRGB
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC6H_UF16
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC6H_SF16
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC7_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BC7_UNORM_SRGB
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format AYUV
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format Y410
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format Y416
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format YUY2
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format Y210
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format Y216
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format B4G4R4A4_UNORM
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format DXT1
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format DXT2
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format DXT3
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format DXT4
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format DXT5
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format RGBA
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BGRA
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format FP16
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format FP32
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BPTC
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder -Format BPTC_FLOAT

    Volume-NvDxt -LoadFolder $VolumeLoadFolder -SaveFolder $VolumeDdsSaveFolder

} finally {
    Push-Location $PSScriptRoot
}


