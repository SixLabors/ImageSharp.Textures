function Cubemap-NvDxt {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    Write-Output "`nCubemap-NvDxt - Processing`n"

    .(Join-Path $PSScriptRoot nvdxt.exe) -list faces.lst -cubeMap -output (Join-Path $SaveFolder "cubemap has-mips.dds")
    .(Join-Path $PSScriptRoot nvdxt.exe) -list faces.lst -cubeMap -nomipmap -output (Join-Path $SaveFolder "cubemap no-mips.dds")

}

function Cubemap-ToKtx {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    Write-Output "`nCubemap-ToKtx - Processing`n"

    .(Join-Path $PSScriptRoot toktx.exe) --cubemap --upper_left_maps_to_s0t0 --automipmap (Join-Path $SaveFolder "cubemap ul has-mips.ktx") "cubemap-positive-x.ppm" "cubemap-negative-x.ppm" "cubemap-positive-y.ppm" "cubemap-negative-y.ppm" "cubemap-positive-z.ppm" "cubemap-negative-z.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --cubemap --upper_left_maps_to_s0t0 (Join-Path $SaveFolder "cubemap ul no-mips.ktx") "cubemap-positive-x.ppm" "cubemap-negative-x.ppm" "cubemap-positive-y.ppm" "cubemap-negative-y.ppm" "cubemap-positive-z.ppm" "cubemap-negative-z.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --cubemap --lower_left_maps_to_s0t0 --automipmap (Join-Path $SaveFolder "cubemap ll has-mips.ktx") "cubemap-positive-x.ppm" "cubemap-negative-x.ppm" "cubemap-positive-y.ppm" "cubemap-negative-y.ppm" "cubemap-positive-z.ppm" "cubemap-negative-z.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --cubemap --lower_left_maps_to_s0t0 (Join-Path $SaveFolder "cubemap ll no-mips.ktx") "cubemap-positive-x.ppm" "cubemap-negative-x.ppm" "cubemap-positive-y.ppm" "cubemap-negative-y.ppm" "cubemap-positive-z.ppm" "cubemap-negative-z.ppm"
    
}


function Cubemap-Pvr {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    $Formats = @("PVRTC1_2", "PVRTC1_4", "PVRTC1_2_RGB", "PVRTC1_4_RGB", "PVRTC2_2", "PVRTC2_4", "ETC1", "BC1", "BC2", "BC3", "UYVY", "YUY2", "1BPP", "RGBE9995", "RGBG8888", "GRGB8888", "ETC2_RGB", "ETC2_RGBA", "ETC2_RGB_A1", "EAC_R11", "EAC_RG11")
    $Types = @("UB", "UBN", "SB", "SBN", "US", "USN", "SS", "SSN", "UI", "UIN", "SI", "SIN", "UF", "SF")
    $ColourSpaces = @("lRGB", "sRGB")

    foreach ($CurrentFormat in $Formats) 
    {
        foreach ($CurrentType in $Types) 
        {
            foreach ($CurrentColourSpace in $ColourSpaces) 
            {
                $ProcessFormat = "$CurrentFormat,$CurrentType,$CurrentColourSpace"
                $FileFormat = "$($CurrentFormat + "_" + $CurrentType + "_" + $CurrentColourSpace)"

                $Speed = "";
                if ($CurrentFormat.StartsWith('PVR')) {
                    $Speed = "pvrtcfastest"
                }
                if ($CurrentFormat.StartsWith('ETC')) {
                    $Speed = "etcfast"
                }
                if ($CurrentFormat.StartsWith('ASTC')) {
                    $Speed = "astcfast"
                }
                
                Write-Output "`nCubemap-Pvr - Processing Format $ProcessFormat`n"

                if ($Speed.Length > 0) 
                {
     
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -cube -q $Speed-i "cubemap-positive-x.png","cubemap-negative-x.png","cubemap-positive-y.png","cubemap-negative-y.png","cubemap-positive-z.png","cubemap-negative-z.png" -o (Join-Path $SaveFolder "cubemap has-mips $FileFormat.pvr") -m -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -cube -q $Speed -i "cubemap-positive-x.png","cubemap-negative-x.png","cubemap-positive-y.png","cubemap-negative-y.png","cubemap-positive-z.png","cubemap-negative-z.png" -o (Join-Path $SaveFolder "cubemap no-mips $FileFormat.pvr") -f $ProcessFormat

                } 
                else 
                {

                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -cube -i "cubemap-positive-x.png","cubemap-negative-x.png","cubemap-positive-y.png","cubemap-negative-y.png","cubemap-positive-z.png","cubemap-negative-z.png" -o (Join-Path $SaveFolder "cubemap has-mips $FileFormat.pvr") -m -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -cube -i "cubemap-positive-x.png","cubemap-negative-x.png","cubemap-positive-y.png","cubemap-negative-y.png","cubemap-positive-z.png","cubemap-negative-z.png" -o (Join-Path $SaveFolder "cubemap no-mips $FileFormat.pvr") -f $ProcessFormat

                }

            }
        }
    }

}

function Flat-NvDxt {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    $Formats = @("dxt1c", "dxt1a", "dxt3", "dxt5", "u1555", "u4444", "u565", "u8888", "u888", "u555", "a8", "cxv8u8", "v8u8", "A8L8", "fp32x4", "fp32", "fp16x4", "dxt5nm", "3Dc", "g16r16", "g16r16f")

    foreach ($Format in $Formats) 
    {

        Write-Output "`nFlat-NvDxt - Processing Format $Format`n"

        .(Join-Path $PSScriptRoot nvdxt.exe) -file flat-pot-alpha.png -$Format -output (Join-Path $SaveFolder "flat-pot-alpha has-mips $Format.dds")
        .(Join-Path $PSScriptRoot nvdxt.exe) -file flat-pot-alpha.png -$Format -nomipmap -output (Join-Path $SaveFolder "flat-pot-alpha no-mips $Format.dds")
        .(Join-Path $PSScriptRoot nvdxt.exe) -file flat-pot-no-alpha.png -$Format -output (Join-Path $SaveFolder "flat-pot-no-alpha has-mips $Format.dds")
        .(Join-Path $PSScriptRoot nvdxt.exe) -file flat-pot-no-alpha.png -$Format -nomipmap -output (Join-Path $SaveFolder "flat-pot-no-alpha no-mips $Format.dds")

    }
}

function Flat-ToKtx {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    Write-Output "`nFlat-ToKtx - Processing`n"

    .(Join-Path $PSScriptRoot toktx.exe) --upper_left_maps_to_s0t0 --automipmap (Join-Path $SaveFolder "flat-pot-alpha ul has-mips.ktx") "flat-pot-alpha.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --upper_left_maps_to_s0t0 (Join-Path $SaveFolder "flat-pot-alpha ul no-mips.ktx") "flat-pot-alpha.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --lower_left_maps_to_s0t0 --automipmap (Join-Path $SaveFolder "flat-pot-alpha ll has-mips.ktx") "flat-pot-alpha.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --lower_left_maps_to_s0t0 (Join-Path $SaveFolder "flat-pot-alpha ll no-mips.ktx") "flat-pot-alpha.ppm"

    .(Join-Path $PSScriptRoot toktx.exe) --upper_left_maps_to_s0t0 --automipmap (Join-Path $SaveFolder "flat-rect-alpha ul has-mips.ktx") "flat-rect-alpha.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --upper_left_maps_to_s0t0 (Join-Path $SaveFolder "flat-rect-alpha ul no-mips.ktx") "flat-rect-alpha.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --lower_left_maps_to_s0t0 --automipmap (Join-Path $SaveFolder "flat-rect-alpha ll has-mips.ktx") "flat-rect-alpha.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --lower_left_maps_to_s0t0 (Join-Path $SaveFolder "flat-rect-alpha ll no-mips.ktx") "flat-rect-alpha.ppm"

    .(Join-Path $PSScriptRoot toktx.exe) --upper_left_maps_to_s0t0 --automipmap (Join-Path $SaveFolder "flat-square-alpha ul has-mips.ktx") "flat-square-alpha.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --upper_left_maps_to_s0t0 (Join-Path $SaveFolder "flat-square-alpha ul no-mips.ktx") "flat-square-alpha.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --lower_left_maps_to_s0t0 --automipmap (Join-Path $SaveFolder "flat-square-alpha ll has-mips.ktx") "flat-square-alpha.ppm"
    .(Join-Path $PSScriptRoot toktx.exe) --lower_left_maps_to_s0t0 (Join-Path $SaveFolder "flat-square-alpha ll no-mips.ktx") "flat-square-alpha.ppm"
    
}

function Flat-Basis {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    Write-Output "`nFlat-Basis - Processing`n"

    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-pot-alpha.png" -mipmap -output_file (Join-Path $SaveFolder "flat-pot-alpha has-mips.basis")
    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-pot-alpha.png" -output_file (Join-Path $SaveFolder "flat-pot-alpha no-mips.basis")
    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-pot-no-alpha.png" -mipmap -output_file (Join-Path $SaveFolder "flat-pot-alpha has-mips.basis")
    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-pot-no-alpha.png" -output_file (Join-Path $SaveFolder "flat-pot-no-alpha no-mips.basis")

    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-rect-alpha.png" -mipmap -output_file (Join-Path $SaveFolder "flat-rect-alpha has-mips.basis")
    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-rect-alpha.png" -output_file (Join-Path $SaveFolder "flat-rect-alpha no-mips.basis")
    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-rect-no-alpha.png" -mipmap -output_file (Join-Path $SaveFolder "flat-rect-alpha has-mips.basis")
    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-rect-no-alpha.png" -output_file (Join-Path $SaveFolder "flat-rect-no-alpha no-mips.basis")

    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-square-alpha.png" -mipmap -output_file (Join-Path $SaveFolder "flat-square-alpha has-mips.basis")
    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-square-alpha.png" -output_file (Join-Path $SaveFolder "flat-square-alpha no-mips.basis")
    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-square-no-alpha.png" -mipmap -output_file (Join-Path $SaveFolder "flat-square-alpha has-mips.basis")
    .(Join-Path $PSScriptRoot basisu.exe) -file "flat-square-no-alpha.png" -output_file (Join-Path $SaveFolder "flat-square-no-alpha no-mips.basis")
  
}

function Flat-TexConv {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    $Formats = @("R32G32B32A32_FLOAT", "R32G32B32A32_UINT", "R32G32B32A32_SINT", "R32G32B32_FLOAT", "R32G32B32_UINT", "R32G32B32_SINT", "R16G16B16A16_FLOAT", "R16G16B16A16_UNORM", "R16G16B16A16_UINT", "R16G16B16A16_SNORM", "R16G16B16A16_SINT", "R32G32_FLOAT", "R32G32_UINT", "R32G32_SINT", "R10G10B10A2_UNORM", "R10G10B10A2_UINT", "R11G11B10_FLOAT", "R8G8B8A8_UNORM", "R8G8B8A8_UNORM_SRGB", "R8G8B8A8_UINT", "R8G8B8A8_SNORM", "R8G8B8A8_SINT", "R16G16_FLOAT", "R16G16_UNORM", "R16G16_UINT", "R16G16_SNORM", "R16G16_SINT", "R32_FLOAT", "R32_UINT", "R32_SINT", "R8G8_UNORM", "R8G8_UINT", "R8G8_SNORM", "R8G8_SINT", "R16_FLOAT", "R16_UNORM", "R16_UINT", "R16_SNORM", "R16_SINT", "R8_UNORM", "R8_UINT", "R8_SNORM", "R8_SINT", "A8_UNORM", "R9G9B9E5_SHAREDEXP", "R8G8_B8G8_UNORM", "G8R8_G8B8_UNORM", "BC1_UNORM", "BC1_UNORM_SRGB", "BC2_UNORM", "BC2_UNORM_SRGB", "BC3_UNORM", "BC3_UNORM_SRGB", "BC4_UNORM", "BC4_SNORM", "BC5_UNORM", "BC5_SNORM", "B5G6R5_UNORM", "B5G5R5A1_UNORM", "B8G8R8A8_UNORM", "B8G8R8X8_UNORM", "R10G10B10_XR_BIAS_A2_UNORM", "B8G8R8A8_UNORM_SRGB", "B8G8R8X8_UNORM_SRGB", "BC6H_UF16", "BC6H_SF16", "BC7_UNORM", "BC7_UNORM_SRGB", "AYUV", "Y410", "Y416", "YUY2", "Y210", "Y216", "B4G4R4A4_UNORM", "DXT1", "DXT2", "DXT3", "DXT4", "DXT5", "RGBA", "BGRA", "FP16", "FP32", "BPTC", "BPTC_FLOAT")

    foreach ($Format in $Formats) 
    {

        Write-Output "`nFlat-TexConv - Processing Format $Format`n"

        $Suffix = "$(" " + $Format)"

        .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx $Suffix -ft dds "flat-pot-alpha.png"
        .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx $Suffix -ft dds "flat-pot-no-alpha.png"

        $SuffixDx10 = "$(" dx10 " + $Format)"

        .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx $SuffixDx10 -ft dds -dx10 "flat-pot-alpha.png"
        .(Join-Path $PSScriptRoot texconv.exe) -f $Format -o $SaveFolder -y -sx $SuffixDx10 -ft dds -dx10 "flat-pot-no-alpha.png"

    }

    # Get-ChildItem *.DDS | Rename-Item -newname { $_.name -replace '.DDS','.dds' }

}


function Flat-Pvr {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    $Formats = @("PVRTC1_2", "PVRTC1_4", "PVRTC1_2_RGB", "PVRTC1_4_RGB", "PVRTC2_2", "PVRTC2_4", "ETC1", "BC1", "BC2", "BC3", "UYVY", "YUY2", "1BPP", "RGBE9995", "RGBG8888", "GRGB8888", "ETC2_RGB", "ETC2_RGBA", "ETC2_RGB_A1", "EAC_R11", "EAC_RG11")
    $Types = @("UB", "UBN", "SB", "SBN", "US", "USN", "SS", "SSN", "UI", "UIN", "SI", "SIN", "UF", "SF")
    $ColourSpaces = @("lRGB", "sRGB")

    foreach ($CurrentFormat in $Formats) 
    {
        foreach ($CurrentType in $Types) 
        {
            foreach ($CurrentColourSpace in $ColourSpaces) 
            {
                $ProcessFormat = "$CurrentFormat,$CurrentType,$CurrentColourSpace"
                $FileFormat = "$($CurrentFormat + "_" + $CurrentType + "_" + $CurrentColourSpace)"

                $Speed = "";
                if ($CurrentFormat.StartsWith('PVR')) {
                    $Speed = "pvrtcfastest"
                }
                if ($CurrentFormat.StartsWith('ETC')) {
                    $Speed = "etcfast"
                }
                if ($CurrentFormat.StartsWith('ASTC')) {
                    $Speed = "astcfast"
                }
                
                Write-Output "`nFlat-Pvr - Processing Format $ProcessFormat`n"

                
                if ($Speed.Length > 0) 
                {
     
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -q $Speed -i "flat-pot-alpha.png" -o (Join-Path $SaveFolder "flat-pot-alpha has-mips $FileFormat.pvr") -m -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -q $Speed -i "flat-pot-alpha.png" -o (Join-Path $SaveFolder "flat-pot-alpha no-mips $FileFormat.pvr") -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -q $Speed -i "flat pot-no-alpha.png" -o (Join-Path $SaveFolder "flat-pot-no-alpha has-mips $FileFormat.pvr") -m -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -q $Speed -i "flat-pot-no-alpha.png" -o (Join-Path $SaveFolder "flat-pot-no-alpha no-mips $FileFormat.pvr") -f $ProcessFormat
            
                } 
                else
                {

                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -i "flat-pot-alpha.png" -o (Join-Path $SaveFolder "flat-pot-alpha has-mips $FileFormat.pvr") -m -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -i "flat-pot-alpha.png" -o (Join-Path $SaveFolder "flat-pot-alpha no-mips $FileFormat.pvr") -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -i "flat-pot-no-alpha.png" -o (Join-Path $SaveFolder "flat-pot-no-alpha has-mips $FileFormat.pvr") -m -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -i "flat-pot-no-alpha.png" -o (Join-Path $SaveFolder "flat-pot-no-alpha no-mips $FileFormat.pvr") -f $ProcessFormat
            

                }

            }
        }
    }

}

function Volume-NvDxt {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    Write-Output "`nVolume-NvDxt - Processing`n"

    .(Join-Path $PSScriptRoot nvdxt.exe) -list slices.lst -volumeMap -output (Join-Path $SaveFolder "volume has-mips.dds")
    .(Join-Path $PSScriptRoot nvdxt.exe) -list slices.lst -volumeMap -nomipmap -output (Join-Path $SaveFolder "volume no-mips.dds")

}

function Volume-Pvr {

    Param ([string]$LoadFolder, [string]$SaveFolder)

    Push-Location $LoadFolder

    $Formats = @("PVRTC1_2", "PVRTC1_4", "PVRTC1_2_RGB", "PVRTC1_4_RGB", "PVRTC2_2", "PVRTC2_4", "ETC1", "BC1", "BC2", "BC3", "UYVY", "YUY2", "1BPP", "RGBE9995", "RGBG8888", "GRGB8888", "ETC2_RGB", "ETC2_RGBA", "ETC2_RGB_A1", "EAC_R11", "EAC_RG11")
    $Types = @("UB", "UBN", "SB", "SBN", "US", "USN", "SS", "SSN", "UI", "UIN", "SI", "SIN", "UF", "SF")
    $ColourSpaces = @("lRGB", "sRGB")

    foreach ($CurrentFormat in $Formats) 
    {
        foreach ($CurrentType in $Types) 
        {
            foreach ($CurrentColourSpace in $ColourSpaces) 
            {
                $ProcessFormat = "$CurrentFormat,$CurrentType,$CurrentColourSpace"
                $FileFormat = "$($CurrentFormat + "_" + $CurrentType + "_" + $CurrentColourSpace)"

                $Speed = "";
                if ($CurrentFormat.StartsWith('PVR')) {
                    $Speed = "pvrtcfastest"
                }
                if ($CurrentFormat.StartsWith('ETC')) {
                    $Speed = "etcfast"
                }
                if ($CurrentFormat.StartsWith('ASTC')) {
                    $Speed = "astcfast"
                }
                
                Write-Output "`nVolume-Pvr - Processing Format $ProcessFormat`n"
     
                if ($Speed.Length > 0) 
                {
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -array -q $Speed -i "volume-slice-1.png","volume-slice-2.png","volume-slice-3.png","volume-slice-4.png","volume-slice-5.png","volume-slice-6.png","volume-slice-7.png","volume-slice-8.png","volume-slice-9.png","volume-slice-10.png","volume-slice-11.png","volume-slice-12.png","volume-slice-13.png","volume-slice-14.png","volume-slice-15.png","volume-slice-16.png","volume-slice-17.png","volume-slice-18.png","volume-slice-19.png","volume-slice-20.png","volume-slice-21.png","volume-slice-22.png","volume-slice-23.png","volume-slice-24.png","volume-slice-25.png","volume-slice-26.png","volume-slice-27.png","volume-slice-28.png","volume-slice-29.png","volume-slice-30.png","volume-slice-31.png","volume-slice-32.png","volume-slice-33.png","volume-slice-34.png","volume-slice-35.png","volume-slice-36.png","volume-slice-37.png","volume-slice-38.png","volume-slice-39.png","volume-slice-40.png","volume-slice-41.png","volume-slice-42.png","volume-slice-43.png","volume-slice-44.png","volume-slice-45.png","volume-slice-46.png","volume-slice-47.png","volume-slice-48.png","volume-slice-49.png","volume-slice-50.png","volume-slice-51.png","volume-slice-52.png","volume-slice-53.png","volume-slice-54.png","volume-slice-55.png","volume-slice-56.png","volume-slice-57.png","volume-slice-58.png","volume-slice-59.png","volume-slice-60.png","volume-slice-61.png","volume-slice-62.png","volume-slice-63.png","volume-slice-64.png","volume-slice-65.png","volume-slice-66.png","volume-slice-67.png","volume-slice-68.png","volume-slice-69.png","volume-slice-70.png","volume-slice-71.png","volume-slice-72.png","volume-slice-73.png","volume-slice-74.png","volume-slice-75.png","volume-slice-76.png","volume-slice-77.png","volume-slice-78.png","volume-slice-79.png","volume-slice-80.png","volume-slice-81.png","volume-slice-82.png","volume-slice-83.png","volume-slice-84.png","volume-slice-85.png","volume-slice-86.png","volume-slice-87.png","volume-slice-88.png","volume-slice-89.png","volume-slice-90.png","volume-slice-91.png","volume-slice-92.png","volume-slice-93.png","volume-slice-94.png","volume-slice-95.png","volume-slice-96.png","volume-slice-97.png","volume-slice-98.png","volume-slice-99.png","volume-slice-100.png","volume-slice-101.png","volume-slice-102.png","volume-slice-103.png","volume-slice-104.png","volume-slice-105.png","volume-slice-106.png","volume-slice-107.png","volume-slice-108.png","volume-slice-109.png","volume-slice-110.png","volume-slice-111.png","volume-slice-112.png","volume-slice-113.png","volume-slice-114.png","volume-slice-115.png","volume-slice-116.png","volume-slice-117.png","volume-slice-118.png","volume-slice-119.png","volume-slice-120.png","volume-slice-121.png","volume-slice-122.png","volume-slice-123.png","volume-slice-124.png","volume-slice-125.png","volume-slice-126.png","volume-slice-127.png","volume-slice-128.png","volume-slice-129.png","volume-slice-130.png","volume-slice-131.png","volume-slice-132.png","volume-slice-133.png","volume-slice-134.png","volume-slice-135.png","volume-slice-136.png","volume-slice-137.png","volume-slice-138.png","volume-slice-139.png","volume-slice-140.png","volume-slice-141.png","volume-slice-142.png","volume-slice-143.png","volume-slice-144.png","volume-slice-145.png","volume-slice-146.png","volume-slice-147.png","volume-slice-148.png","volume-slice-149.png","volume-slice-150.png","volume-slice-151.png","volume-slice-152.png","volume-slice-153.png","volume-slice-154.png","volume-slice-155.png","volume-slice-156.png","volume-slice-157.png","volume-slice-158.png","volume-slice-159.png","volume-slice-160.png","volume-slice-161.png","volume-slice-162.png","volume-slice-163.png","volume-slice-164.png","volume-slice-165.png","volume-slice-166.png","volume-slice-167.png","volume-slice-168.png","volume-slice-169.png","volume-slice-170.png","volume-slice-171.png","volume-slice-172.png","volume-slice-173.png","volume-slice-174.png","volume-slice-175.png","volume-slice-176.png","volume-slice-177.png","volume-slice-178.png","volume-slice-179.png","volume-slice-180.png","volume-slice-181.png","volume-slice-182.png","volume-slice-183.png","volume-slice-184.png","volume-slice-185.png","volume-slice-186.png","volume-slice-187.png","volume-slice-188.png","volume-slice-189.png","volume-slice-190.png","volume-slice-191.png","volume-slice-192.png","volume-slice-193.png","volume-slice-194.png","volume-slice-195.png","volume-slice-196.png","volume-slice-197.png","volume-slice-198.png","volume-slice-199.png","volume-slice-200.png","volume-slice-201.png","volume-slice-202.png","volume-slice-203.png","volume-slice-204.png","volume-slice-205.png","volume-slice-206.png","volume-slice-207.png","volume-slice-208.png","volume-slice-209.png","volume-slice-210.png","volume-slice-211.png","volume-slice-212.png","volume-slice-213.png","volume-slice-214.png","volume-slice-215.png","volume-slice-216.png","volume-slice-217.png","volume-slice-218.png","volume-slice-219.png","volume-slice-220.png","volume-slice-221.png","volume-slice-222.png","volume-slice-223.png","volume-slice-224.png","volume-slice-225.png","volume-slice-226.png","volume-slice-227.png","volume-slice-228.png","volume-slice-229.png","volume-slice-230.png","volume-slice-231.png","volume-slice-232.png","volume-slice-233.png","volume-slice-234.png","volume-slice-235.png","volume-slice-236.png","volume-slice-237.png","volume-slice-238.png","volume-slice-239.png","volume-slice-240.png","volume-slice-241.png","volume-slice-242.png","volume-slice-243.png","volume-slice-244.png","volume-slice-245.png","volume-slice-246.png","volume-slice-247.png","volume-slice-248.png","volume-slice-249.png","volume-slice-250.png","volume-slice-251.png","volume-slice-252.png","volume-slice-253.png","volume-slice-254.png","volume-slice-255.png","volume-slice-256.png" -o (Join-Path $SaveFolder "volume has-mips $FileFormat.pvr") -m -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -array -q $Speed -i "volume-slice-1.png","volume-slice-2.png","volume-slice-3.png","volume-slice-4.png","volume-slice-5.png","volume-slice-6.png","volume-slice-7.png","volume-slice-8.png","volume-slice-9.png","volume-slice-10.png","volume-slice-11.png","volume-slice-12.png","volume-slice-13.png","volume-slice-14.png","volume-slice-15.png","volume-slice-16.png","volume-slice-17.png","volume-slice-18.png","volume-slice-19.png","volume-slice-20.png","volume-slice-21.png","volume-slice-22.png","volume-slice-23.png","volume-slice-24.png","volume-slice-25.png","volume-slice-26.png","volume-slice-27.png","volume-slice-28.png","volume-slice-29.png","volume-slice-30.png","volume-slice-31.png","volume-slice-32.png","volume-slice-33.png","volume-slice-34.png","volume-slice-35.png","volume-slice-36.png","volume-slice-37.png","volume-slice-38.png","volume-slice-39.png","volume-slice-40.png","volume-slice-41.png","volume-slice-42.png","volume-slice-43.png","volume-slice-44.png","volume-slice-45.png","volume-slice-46.png","volume-slice-47.png","volume-slice-48.png","volume-slice-49.png","volume-slice-50.png","volume-slice-51.png","volume-slice-52.png","volume-slice-53.png","volume-slice-54.png","volume-slice-55.png","volume-slice-56.png","volume-slice-57.png","volume-slice-58.png","volume-slice-59.png","volume-slice-60.png","volume-slice-61.png","volume-slice-62.png","volume-slice-63.png","volume-slice-64.png","volume-slice-65.png","volume-slice-66.png","volume-slice-67.png","volume-slice-68.png","volume-slice-69.png","volume-slice-70.png","volume-slice-71.png","volume-slice-72.png","volume-slice-73.png","volume-slice-74.png","volume-slice-75.png","volume-slice-76.png","volume-slice-77.png","volume-slice-78.png","volume-slice-79.png","volume-slice-80.png","volume-slice-81.png","volume-slice-82.png","volume-slice-83.png","volume-slice-84.png","volume-slice-85.png","volume-slice-86.png","volume-slice-87.png","volume-slice-88.png","volume-slice-89.png","volume-slice-90.png","volume-slice-91.png","volume-slice-92.png","volume-slice-93.png","volume-slice-94.png","volume-slice-95.png","volume-slice-96.png","volume-slice-97.png","volume-slice-98.png","volume-slice-99.png","volume-slice-100.png","volume-slice-101.png","volume-slice-102.png","volume-slice-103.png","volume-slice-104.png","volume-slice-105.png","volume-slice-106.png","volume-slice-107.png","volume-slice-108.png","volume-slice-109.png","volume-slice-110.png","volume-slice-111.png","volume-slice-112.png","volume-slice-113.png","volume-slice-114.png","volume-slice-115.png","volume-slice-116.png","volume-slice-117.png","volume-slice-118.png","volume-slice-119.png","volume-slice-120.png","volume-slice-121.png","volume-slice-122.png","volume-slice-123.png","volume-slice-124.png","volume-slice-125.png","volume-slice-126.png","volume-slice-127.png","volume-slice-128.png","volume-slice-129.png","volume-slice-130.png","volume-slice-131.png","volume-slice-132.png","volume-slice-133.png","volume-slice-134.png","volume-slice-135.png","volume-slice-136.png","volume-slice-137.png","volume-slice-138.png","volume-slice-139.png","volume-slice-140.png","volume-slice-141.png","volume-slice-142.png","volume-slice-143.png","volume-slice-144.png","volume-slice-145.png","volume-slice-146.png","volume-slice-147.png","volume-slice-148.png","volume-slice-149.png","volume-slice-150.png","volume-slice-151.png","volume-slice-152.png","volume-slice-153.png","volume-slice-154.png","volume-slice-155.png","volume-slice-156.png","volume-slice-157.png","volume-slice-158.png","volume-slice-159.png","volume-slice-160.png","volume-slice-161.png","volume-slice-162.png","volume-slice-163.png","volume-slice-164.png","volume-slice-165.png","volume-slice-166.png","volume-slice-167.png","volume-slice-168.png","volume-slice-169.png","volume-slice-170.png","volume-slice-171.png","volume-slice-172.png","volume-slice-173.png","volume-slice-174.png","volume-slice-175.png","volume-slice-176.png","volume-slice-177.png","volume-slice-178.png","volume-slice-179.png","volume-slice-180.png","volume-slice-181.png","volume-slice-182.png","volume-slice-183.png","volume-slice-184.png","volume-slice-185.png","volume-slice-186.png","volume-slice-187.png","volume-slice-188.png","volume-slice-189.png","volume-slice-190.png","volume-slice-191.png","volume-slice-192.png","volume-slice-193.png","volume-slice-194.png","volume-slice-195.png","volume-slice-196.png","volume-slice-197.png","volume-slice-198.png","volume-slice-199.png","volume-slice-200.png","volume-slice-201.png","volume-slice-202.png","volume-slice-203.png","volume-slice-204.png","volume-slice-205.png","volume-slice-206.png","volume-slice-207.png","volume-slice-208.png","volume-slice-209.png","volume-slice-210.png","volume-slice-211.png","volume-slice-212.png","volume-slice-213.png","volume-slice-214.png","volume-slice-215.png","volume-slice-216.png","volume-slice-217.png","volume-slice-218.png","volume-slice-219.png","volume-slice-220.png","volume-slice-221.png","volume-slice-222.png","volume-slice-223.png","volume-slice-224.png","volume-slice-225.png","volume-slice-226.png","volume-slice-227.png","volume-slice-228.png","volume-slice-229.png","volume-slice-230.png","volume-slice-231.png","volume-slice-232.png","volume-slice-233.png","volume-slice-234.png","volume-slice-235.png","volume-slice-236.png","volume-slice-237.png","volume-slice-238.png","volume-slice-239.png","volume-slice-240.png","volume-slice-241.png","volume-slice-242.png","volume-slice-243.png","volume-slice-244.png","volume-slice-245.png","volume-slice-246.png","volume-slice-247.png","volume-slice-248.png","volume-slice-249.png","volume-slice-250.png","volume-slice-251.png","volume-slice-252.png","volume-slice-253.png","volume-slice-254.png","volume-slice-255.png","volume-slice-256.png" -o (Join-Path $SaveFolder "volume no-mips $FileFormat.pvr") -f $ProcessFormat
                }
                else
                {
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -array -i "volume-slice-1.png","volume-slice-2.png","volume-slice-3.png","volume-slice-4.png","volume-slice-5.png","volume-slice-6.png","volume-slice-7.png","volume-slice-8.png","volume-slice-9.png","volume-slice-10.png","volume-slice-11.png","volume-slice-12.png","volume-slice-13.png","volume-slice-14.png","volume-slice-15.png","volume-slice-16.png","volume-slice-17.png","volume-slice-18.png","volume-slice-19.png","volume-slice-20.png","volume-slice-21.png","volume-slice-22.png","volume-slice-23.png","volume-slice-24.png","volume-slice-25.png","volume-slice-26.png","volume-slice-27.png","volume-slice-28.png","volume-slice-29.png","volume-slice-30.png","volume-slice-31.png","volume-slice-32.png","volume-slice-33.png","volume-slice-34.png","volume-slice-35.png","volume-slice-36.png","volume-slice-37.png","volume-slice-38.png","volume-slice-39.png","volume-slice-40.png","volume-slice-41.png","volume-slice-42.png","volume-slice-43.png","volume-slice-44.png","volume-slice-45.png","volume-slice-46.png","volume-slice-47.png","volume-slice-48.png","volume-slice-49.png","volume-slice-50.png","volume-slice-51.png","volume-slice-52.png","volume-slice-53.png","volume-slice-54.png","volume-slice-55.png","volume-slice-56.png","volume-slice-57.png","volume-slice-58.png","volume-slice-59.png","volume-slice-60.png","volume-slice-61.png","volume-slice-62.png","volume-slice-63.png","volume-slice-64.png","volume-slice-65.png","volume-slice-66.png","volume-slice-67.png","volume-slice-68.png","volume-slice-69.png","volume-slice-70.png","volume-slice-71.png","volume-slice-72.png","volume-slice-73.png","volume-slice-74.png","volume-slice-75.png","volume-slice-76.png","volume-slice-77.png","volume-slice-78.png","volume-slice-79.png","volume-slice-80.png","volume-slice-81.png","volume-slice-82.png","volume-slice-83.png","volume-slice-84.png","volume-slice-85.png","volume-slice-86.png","volume-slice-87.png","volume-slice-88.png","volume-slice-89.png","volume-slice-90.png","volume-slice-91.png","volume-slice-92.png","volume-slice-93.png","volume-slice-94.png","volume-slice-95.png","volume-slice-96.png","volume-slice-97.png","volume-slice-98.png","volume-slice-99.png","volume-slice-100.png","volume-slice-101.png","volume-slice-102.png","volume-slice-103.png","volume-slice-104.png","volume-slice-105.png","volume-slice-106.png","volume-slice-107.png","volume-slice-108.png","volume-slice-109.png","volume-slice-110.png","volume-slice-111.png","volume-slice-112.png","volume-slice-113.png","volume-slice-114.png","volume-slice-115.png","volume-slice-116.png","volume-slice-117.png","volume-slice-118.png","volume-slice-119.png","volume-slice-120.png","volume-slice-121.png","volume-slice-122.png","volume-slice-123.png","volume-slice-124.png","volume-slice-125.png","volume-slice-126.png","volume-slice-127.png","volume-slice-128.png","volume-slice-129.png","volume-slice-130.png","volume-slice-131.png","volume-slice-132.png","volume-slice-133.png","volume-slice-134.png","volume-slice-135.png","volume-slice-136.png","volume-slice-137.png","volume-slice-138.png","volume-slice-139.png","volume-slice-140.png","volume-slice-141.png","volume-slice-142.png","volume-slice-143.png","volume-slice-144.png","volume-slice-145.png","volume-slice-146.png","volume-slice-147.png","volume-slice-148.png","volume-slice-149.png","volume-slice-150.png","volume-slice-151.png","volume-slice-152.png","volume-slice-153.png","volume-slice-154.png","volume-slice-155.png","volume-slice-156.png","volume-slice-157.png","volume-slice-158.png","volume-slice-159.png","volume-slice-160.png","volume-slice-161.png","volume-slice-162.png","volume-slice-163.png","volume-slice-164.png","volume-slice-165.png","volume-slice-166.png","volume-slice-167.png","volume-slice-168.png","volume-slice-169.png","volume-slice-170.png","volume-slice-171.png","volume-slice-172.png","volume-slice-173.png","volume-slice-174.png","volume-slice-175.png","volume-slice-176.png","volume-slice-177.png","volume-slice-178.png","volume-slice-179.png","volume-slice-180.png","volume-slice-181.png","volume-slice-182.png","volume-slice-183.png","volume-slice-184.png","volume-slice-185.png","volume-slice-186.png","volume-slice-187.png","volume-slice-188.png","volume-slice-189.png","volume-slice-190.png","volume-slice-191.png","volume-slice-192.png","volume-slice-193.png","volume-slice-194.png","volume-slice-195.png","volume-slice-196.png","volume-slice-197.png","volume-slice-198.png","volume-slice-199.png","volume-slice-200.png","volume-slice-201.png","volume-slice-202.png","volume-slice-203.png","volume-slice-204.png","volume-slice-205.png","volume-slice-206.png","volume-slice-207.png","volume-slice-208.png","volume-slice-209.png","volume-slice-210.png","volume-slice-211.png","volume-slice-212.png","volume-slice-213.png","volume-slice-214.png","volume-slice-215.png","volume-slice-216.png","volume-slice-217.png","volume-slice-218.png","volume-slice-219.png","volume-slice-220.png","volume-slice-221.png","volume-slice-222.png","volume-slice-223.png","volume-slice-224.png","volume-slice-225.png","volume-slice-226.png","volume-slice-227.png","volume-slice-228.png","volume-slice-229.png","volume-slice-230.png","volume-slice-231.png","volume-slice-232.png","volume-slice-233.png","volume-slice-234.png","volume-slice-235.png","volume-slice-236.png","volume-slice-237.png","volume-slice-238.png","volume-slice-239.png","volume-slice-240.png","volume-slice-241.png","volume-slice-242.png","volume-slice-243.png","volume-slice-244.png","volume-slice-245.png","volume-slice-246.png","volume-slice-247.png","volume-slice-248.png","volume-slice-249.png","volume-slice-250.png","volume-slice-251.png","volume-slice-252.png","volume-slice-253.png","volume-slice-254.png","volume-slice-255.png","volume-slice-256.png" -o (Join-Path $SaveFolder "volume has-mips $FileFormat.pvr") -m -f $ProcessFormat
                    .(Join-Path $PSScriptRoot PVRTexToolCLI.exe) -array -i "volume-slice-1.png","volume-slice-2.png","volume-slice-3.png","volume-slice-4.png","volume-slice-5.png","volume-slice-6.png","volume-slice-7.png","volume-slice-8.png","volume-slice-9.png","volume-slice-10.png","volume-slice-11.png","volume-slice-12.png","volume-slice-13.png","volume-slice-14.png","volume-slice-15.png","volume-slice-16.png","volume-slice-17.png","volume-slice-18.png","volume-slice-19.png","volume-slice-20.png","volume-slice-21.png","volume-slice-22.png","volume-slice-23.png","volume-slice-24.png","volume-slice-25.png","volume-slice-26.png","volume-slice-27.png","volume-slice-28.png","volume-slice-29.png","volume-slice-30.png","volume-slice-31.png","volume-slice-32.png","volume-slice-33.png","volume-slice-34.png","volume-slice-35.png","volume-slice-36.png","volume-slice-37.png","volume-slice-38.png","volume-slice-39.png","volume-slice-40.png","volume-slice-41.png","volume-slice-42.png","volume-slice-43.png","volume-slice-44.png","volume-slice-45.png","volume-slice-46.png","volume-slice-47.png","volume-slice-48.png","volume-slice-49.png","volume-slice-50.png","volume-slice-51.png","volume-slice-52.png","volume-slice-53.png","volume-slice-54.png","volume-slice-55.png","volume-slice-56.png","volume-slice-57.png","volume-slice-58.png","volume-slice-59.png","volume-slice-60.png","volume-slice-61.png","volume-slice-62.png","volume-slice-63.png","volume-slice-64.png","volume-slice-65.png","volume-slice-66.png","volume-slice-67.png","volume-slice-68.png","volume-slice-69.png","volume-slice-70.png","volume-slice-71.png","volume-slice-72.png","volume-slice-73.png","volume-slice-74.png","volume-slice-75.png","volume-slice-76.png","volume-slice-77.png","volume-slice-78.png","volume-slice-79.png","volume-slice-80.png","volume-slice-81.png","volume-slice-82.png","volume-slice-83.png","volume-slice-84.png","volume-slice-85.png","volume-slice-86.png","volume-slice-87.png","volume-slice-88.png","volume-slice-89.png","volume-slice-90.png","volume-slice-91.png","volume-slice-92.png","volume-slice-93.png","volume-slice-94.png","volume-slice-95.png","volume-slice-96.png","volume-slice-97.png","volume-slice-98.png","volume-slice-99.png","volume-slice-100.png","volume-slice-101.png","volume-slice-102.png","volume-slice-103.png","volume-slice-104.png","volume-slice-105.png","volume-slice-106.png","volume-slice-107.png","volume-slice-108.png","volume-slice-109.png","volume-slice-110.png","volume-slice-111.png","volume-slice-112.png","volume-slice-113.png","volume-slice-114.png","volume-slice-115.png","volume-slice-116.png","volume-slice-117.png","volume-slice-118.png","volume-slice-119.png","volume-slice-120.png","volume-slice-121.png","volume-slice-122.png","volume-slice-123.png","volume-slice-124.png","volume-slice-125.png","volume-slice-126.png","volume-slice-127.png","volume-slice-128.png","volume-slice-129.png","volume-slice-130.png","volume-slice-131.png","volume-slice-132.png","volume-slice-133.png","volume-slice-134.png","volume-slice-135.png","volume-slice-136.png","volume-slice-137.png","volume-slice-138.png","volume-slice-139.png","volume-slice-140.png","volume-slice-141.png","volume-slice-142.png","volume-slice-143.png","volume-slice-144.png","volume-slice-145.png","volume-slice-146.png","volume-slice-147.png","volume-slice-148.png","volume-slice-149.png","volume-slice-150.png","volume-slice-151.png","volume-slice-152.png","volume-slice-153.png","volume-slice-154.png","volume-slice-155.png","volume-slice-156.png","volume-slice-157.png","volume-slice-158.png","volume-slice-159.png","volume-slice-160.png","volume-slice-161.png","volume-slice-162.png","volume-slice-163.png","volume-slice-164.png","volume-slice-165.png","volume-slice-166.png","volume-slice-167.png","volume-slice-168.png","volume-slice-169.png","volume-slice-170.png","volume-slice-171.png","volume-slice-172.png","volume-slice-173.png","volume-slice-174.png","volume-slice-175.png","volume-slice-176.png","volume-slice-177.png","volume-slice-178.png","volume-slice-179.png","volume-slice-180.png","volume-slice-181.png","volume-slice-182.png","volume-slice-183.png","volume-slice-184.png","volume-slice-185.png","volume-slice-186.png","volume-slice-187.png","volume-slice-188.png","volume-slice-189.png","volume-slice-190.png","volume-slice-191.png","volume-slice-192.png","volume-slice-193.png","volume-slice-194.png","volume-slice-195.png","volume-slice-196.png","volume-slice-197.png","volume-slice-198.png","volume-slice-199.png","volume-slice-200.png","volume-slice-201.png","volume-slice-202.png","volume-slice-203.png","volume-slice-204.png","volume-slice-205.png","volume-slice-206.png","volume-slice-207.png","volume-slice-208.png","volume-slice-209.png","volume-slice-210.png","volume-slice-211.png","volume-slice-212.png","volume-slice-213.png","volume-slice-214.png","volume-slice-215.png","volume-slice-216.png","volume-slice-217.png","volume-slice-218.png","volume-slice-219.png","volume-slice-220.png","volume-slice-221.png","volume-slice-222.png","volume-slice-223.png","volume-slice-224.png","volume-slice-225.png","volume-slice-226.png","volume-slice-227.png","volume-slice-228.png","volume-slice-229.png","volume-slice-230.png","volume-slice-231.png","volume-slice-232.png","volume-slice-233.png","volume-slice-234.png","volume-slice-235.png","volume-slice-236.png","volume-slice-237.png","volume-slice-238.png","volume-slice-239.png","volume-slice-240.png","volume-slice-241.png","volume-slice-242.png","volume-slice-243.png","volume-slice-244.png","volume-slice-245.png","volume-slice-246.png","volume-slice-247.png","volume-slice-248.png","volume-slice-249.png","volume-slice-250.png","volume-slice-251.png","volume-slice-252.png","volume-slice-253.png","volume-slice-254.png","volume-slice-255.png","volume-slice-256.png" -o (Join-Path $SaveFolder "volume no-mips $FileFormat.pvr") -f $ProcessFormat
                }

            }
        }
    }

}

Push-Location $PSScriptRoot

try {

    $CubemapLoadFolder = Resolve-Path -Path ..\Images\Baseline\Cubemap
    $CubemapDdsSaveFolder = Resolve-Path -Path ..\Images\Input\Dds\Cubemap
    $CubemapKtxSaveFolder = Resolve-Path -Path ..\Images\Input\Ktx\Cubemap
    # $CubemapPvrSaveFolder = Resolve-Path -Path ..\Images\Input\Pvr\Cubemap

    $FlatLoadFolder = Resolve-Path -Path ..\Images\Baseline\Flat
    $FlatDdsSaveFolder = Resolve-Path -Path ..\Images\Input\Dds\Flat
    $FlatKtxSaveFolder = Resolve-Path -Path ..\Images\Input\Ktx\Flat
    # $FlatPvrSaveFolder = Resolve-Path -Path ..\Images\Input\Pvr\Flat
    $FlatBasisSaveFolder = Resolve-Path -Path ..\Images\Input\Basis\Flat

    $VolumeLoadFolder = Resolve-Path -Path ..\Images\Baseline\Volume
    $VolumeDdsSaveFolder = Resolve-Path -Path ..\Images\Input\Dds\Volume
    # $VolumePvrSaveFolder = Resolve-Path -Path ..\Images\Input\Pvr\Volume

    Cubemap-NvDxt -LoadFolder $CubemapLoadFolder -SaveFolder $CubemapDdsSaveFolder
    Flat-NvDxt -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder
    Volume-NvDxt -LoadFolder $VolumeLoadFolder -SaveFolder $VolumeDdsSaveFolder
    Flat-TexConv -LoadFolder $FlatLoadFolder -SaveFolder $FlatDdsSaveFolder

    Flat-Basis -LoadFolder $FlatLoadFolder -SaveFolder $FlatBasisSaveFolder

    Cubemap-ToKtx -LoadFolder $CubemapLoadFolder -SaveFolder $CubemapKtxSaveFolder
    Flat-ToKtx -LoadFolder $FlatLoadFolder -SaveFolder $FlatKtxSaveFolder

    # Cubemap-Pvr -LoadFolder $CubemapLoadFolder -SaveFolder $CubemapPvrSaveFolder
    # Flat-Pvr -LoadFolder $FlatLoadFolder -SaveFolder $FlatPvrSaveFolder
    # Volume-Pvr -LoadFolder $VolumeLoadFolder -SaveFolder $VolumePvrSaveFolder

} finally {
    Push-Location $PSScriptRoot
}


