function Add-Flat-TexConv {

    Param ([string]$InputFolder, [string]$ExpectedFolder)

    Push-Location $InputFolder

    $files = Get-ChildItem $InputFolder -Filter *.dds
    foreach ($file in $files)
    {
        $outputPath = Join-Path $ExpectedFolder $file.Basename
        New-Item -ItemType Directory -Force -Path $outputPath
        .(Join-Path $PSScriptRoot texconv.exe) -o $outputPath -y -ft png $file.FullName
    }

}

Push-Location $PSScriptRoot

try {

    $CubemapDdsInputFolder = Resolve-Path -Path ..\Images\Input\Dds\Cubemap
    $CubemapDdsExpectedFolder = Resolve-Path -Path ..\Images\Expected\Dds\Cubemap

    $FlatDdsInputFolder = Resolve-Path -Path ..\Images\Input\Dds\Flat
    $FlatDdsExpectedFolder = Resolve-Path -Path ..\Images\Expected\Dds\Flat

    $VolumeDdsInputFolder = Resolve-Path -Path ..\Images\Input\Dds\Volume
    $VolumeDdsExpectedFolder = Resolve-Path -Path ..\Images\Expected\Dds\Volume

    Add-Flat-TexConv -InputFolder $CubemapDdsInputFolder -ExpectedFolder $CubemapDdsExpectedFolder
    Add-Flat-TexConv -InputFolder $FlatDdsInputFolder -ExpectedFolder $FlatDdsExpectedFolder
    Add-Flat-TexConv -InputFolder $VolumeDdsInputFolder -ExpectedFolder $VolumeDdsExpectedFolder

} finally {
    Push-Location $PSScriptRoot
}


