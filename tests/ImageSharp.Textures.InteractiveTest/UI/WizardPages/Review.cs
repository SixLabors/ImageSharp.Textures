using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace Phoenix.Import.Application.UI.WizardPages
{
    public class Review : WizardPage
    {
        public struct ImageInfo
        {
            public IntPtr TexturePtr;
            public Vector2 Size;
            public string FilePath;
            public string TempFilePath;
        }

        private readonly InputDialog _inputDialog;
        private readonly Dialog _dialog;
        private readonly AlertDialog _alertDialog;

        private ImageInfo referenceImageInfo;
        private ImageInfo actualImageInfo;

        public Review(Wizard wizard) : base(wizard)
        {
            this._inputDialog = new InputDialog();
            this._dialog = new Dialog();
            this._alertDialog = new AlertDialog();
        }

        public void OpenCompare(string filePath1, string filePath2)
        {
            var command = @"C:\Program Files\Beyond Compare 4\bcomp.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(command, $"\"{filePath1}\" \"{filePath2}\" /fv=\"Picture Compare\""));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                //Process.Start("open", url);
            }
        }

        public ImageInfo LoadImage(string filePath)
        {
            string saveFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.png");
            if (Path.GetExtension(filePath).Equals(".dds", StringComparison.CurrentCultureIgnoreCase))
            {
                var decoder = new DdsDecoder();
                using FileStream fileStream = File.OpenRead(filePath);
                using var result = (FlatTexture)decoder.DecodeTexture(SixLabors.ImageSharp.Textures.Configuration.Default, fileStream);
                using Image ddsImage = result.MipMaps[0].GetImage();
                using Image<Rgba32> clone = ddsImage.CloneAs<Rgba32>();
                clone.Save(saveFilePath);
                return new ImageInfo { TexturePtr = ApplicationManager.Create(clone), Size = new Vector2(clone.Width, clone.Height), FilePath = filePath, TempFilePath = saveFilePath };
            }
            using var image = Image.Load<Rgba32>(filePath);
            image.Save(saveFilePath);
            return new ImageInfo { TexturePtr = ApplicationManager.Create(image), Size = new Vector2(image.Width, image.Height), FilePath = filePath, TempFilePath = saveFilePath };
        }

        public override void Initialize()
        {
            ApplicationManager.ClearImageCache();

            this.referenceImageInfo = this.LoadImage(@"D:\GitPersonal\ImageSharp.Textures\tests\Images\Baseline\Flat\Flat.png");
            this.actualImageInfo = this.LoadImage(@"D:\GitPersonal\ImageSharp.Textures\tests\Images\Input\Dds\Flat\TexConv\9.1\flat DXT1.DDS");
            //this.ddsImageInfo = this.LoadImage(@"D:\GitPersonal\ImageSharp.Textures\tests\Images\Input\Dds\Flat\TexConv\9.1\flat DXT1.DDS");
        }

        public override void Render()
        {
            this.Wizard.CancelButton.Visble = true;
            this.Wizard.PreviousButton.Visble = false;
            this.Wizard.NextButton.Title = "Approve";

            if (this.referenceImageInfo.TexturePtr != IntPtr.Zero)
            { 
                ImGui.Text($"Reference Image ({this.referenceImageInfo.FilePath})");
                ImGui.Image(this.referenceImageInfo.TexturePtr, this.referenceImageInfo.Size);
            }

            if (this.actualImageInfo.TexturePtr != IntPtr.Zero)
            {
                ImGui.Text($"Actual Image ({this.actualImageInfo.FilePath})");
                ImGui.Image(this.actualImageInfo.TexturePtr, this.actualImageInfo.Size);
            }

            if (ImGui.Button("Compare"))
            {
                this.OpenCompare(this.referenceImageInfo.TempFilePath, this.actualImageInfo.TempFilePath);
            }
        }

        public override bool Next(WizardPage newWizardPage)
        {
            this.Wizard.GoHome();
            return false;
        }
    }
}