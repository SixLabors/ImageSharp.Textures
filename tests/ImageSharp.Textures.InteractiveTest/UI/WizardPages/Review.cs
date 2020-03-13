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
using SixLabors.ImageSharp.Processing;
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

        private string rootFolder;
        private string currentFolder;

        private ImageInfo expectedImageInfo;
        private ImageInfo actualImageInfo;

        public Review(Wizard wizard) : base(wizard)
        {
            this._inputDialog = new InputDialog();
            this._dialog = new Dialog();
            this._alertDialog = new AlertDialog();

            this.rootFolder = @"D:\GitPersonal\ImageSharp.Textures\tests\Images\Input\Dds\Flat\";
            this.currentFolder = this.rootFolder;
        }

        public void OpenCompare(string filePath1, string filePath2)
        {
            string command = @"C:\Program Files\Beyond Compare 4\bcomp.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(command, $"\"{filePath1}\" \"{filePath2}\" /fv=\"Picture Compare\""));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                //Process.Start("open", url);
            }
        }

        public string DecompressDDS(string filePath)
        {
            string command = @"D:\GitPersonal\ImageSharp.Textures\tests\Tools\TexConv.exe";
            var process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = $"-ft PNG \"{filePath}\" -o {Path.GetTempPath()}";
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            string saveFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.png");
            string sourceFile = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(filePath)}.png");
            if (File.Exists(sourceFile))
            {
                File.Move(sourceFile, saveFilePath);
            }
            return saveFilePath;
        }

        public ImageInfo LoadExpected(string filePath)
        {
            try
            {
                string ddsSaveFilePath = this.DecompressDDS(filePath);
                if (File.Exists(ddsSaveFilePath))
                {
                    using var clone = Image.Load<Rgba32>(ddsSaveFilePath);
                    return new ImageInfo { TexturePtr = ApplicationManager.Create(clone), Size = new Vector2(clone.Width, clone.Height), FilePath = filePath, TempFilePath = ddsSaveFilePath };
                }
            }
            catch (Exception ex)
            {
                //exc
            }
            return new ImageInfo { TexturePtr = IntPtr.Zero, Size = Vector2.Zero, FilePath = filePath, TempFilePath = string.Empty };
        }

        public ImageInfo LoadActualImage(string filePath)
        {
            try
            {
                string saveFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.png");
                var decoder = new DdsDecoder();
                using FileStream fileStream = File.OpenRead(filePath);
                using var result = (FlatTexture)decoder.DecodeTexture(SixLabors.ImageSharp.Textures.Configuration.Default, fileStream);
                using Image ddsImage = result.MipMaps[0].GetImage();
                using Image<Rgba32> clone = ddsImage.CloneAs<Rgba32>();
                return new ImageInfo { TexturePtr = ApplicationManager.Create(clone), Size = new Vector2(clone.Width, clone.Height), FilePath = filePath, TempFilePath = saveFilePath };
            }
            catch (Exception ex)
            {
            }
            return new ImageInfo { TexturePtr = IntPtr.Zero, Size = Vector2.Zero, FilePath = filePath, TempFilePath = string.Empty };
        }

        public override void Initialize()
        {
            ApplicationManager.ClearImageCache();

            string imagePath = @"D:\GitPersonal\ImageSharp.Textures\tests\Images\Input\Dds\Flat\TexConv\9.1\flat DXT1.DDS";
            this.expectedImageInfo = this.LoadExpected(imagePath);
            this.actualImageInfo = this.LoadActualImage(imagePath);
        }

        private static void DrawLines(IReadOnlyList<Vector2> points, Vector2 location, float size)
        {
            uint iconColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1));
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            for (int i = 0; i < points.Count; i += 2)
            {
                Vector2 vector1 = (points[i] / 100) * size;
                Vector2 vector2 = (points[i + 1] / 100) * size;
                drawList.AddLine(location + vector1, location + vector2, iconColor);
            }
        }

        private static void GenerateFolderIcon(Vector2 location, float size)
        {
            Vector2[] points = new[] {
                new Vector2(0.0f,0.0f), new Vector2(45.0f, 0.0f),
                new Vector2(45.0f,0.0f), new Vector2(55.0f, 22.5f),
                new Vector2(55.0f,22.5f), new Vector2(100.0f, 22.5f),
                new Vector2(100.0f,22.5f), new Vector2(100.0f, 87.5f),
                new Vector2(100.0f,87.5f), new Vector2(0.0f, 87.5f),
                new Vector2(0.0f,87.5f), new Vector2(0.0f, 0.0f)
            };
            DrawLines(points, location, size);
        }

        private static void GenerateFileIcon(Vector2 location, float size)
        {
            Vector2[] points = new[] {
                new Vector2(12.5f,0.0f), new Vector2(62.5f, 0.0f),
                new Vector2(62.5f,0.0f), new Vector2(87.5f, 50.0f),
                new Vector2(87.5f,50.0f), new Vector2(87.5f, 100.0f),
                new Vector2(87.5f,100.0f), new Vector2(12.5f, 100.0f),
                new Vector2(12.5f,100.0f), new Vector2(12.5f, 0.0f),
                new Vector2(62.5f,0.0f), new Vector2(62.5f, 50.0f),
                new Vector2(62.5f,50.0f), new Vector2(87.5f, 50.0f)
            };
            DrawLines(points, location, size);
        }

        public override void Render()
        {
            this.Wizard.CancelButton.Visble = true;
            this.Wizard.PreviousButton.Visble = false;
            this.Wizard.NextButton.Title = "Approve";

            Vector2 size = ImGui.GetWindowSize();

            ImGui.PushItemWidth(size.X - 16);
            ImGui.PopItemWidth();
            ImGui.Spacing();

            if (ImGui.BeginChildFrame(1, new Vector2(200, size.Y - 100), ImGuiWindowFlags.None))
            {
                var directories = new List<string>();
                if (!this.currentFolder.Equals(this.rootFolder, StringComparison.CurrentCultureIgnoreCase))
                {
                    directories.Add("..");
                }
                directories.AddRange(Directory.GetDirectories(this.currentFolder));
                foreach (string directory in directories)
                {
                    Vector2 iconPosition = ImGui.GetWindowPos() + ImGui.GetCursorPos();
                    iconPosition.Y -= ImGui.GetScrollY();
                    float lineHeight = ImGui.GetTextLineHeight();
                    ImGui.SetCursorPosX(lineHeight * 2);

                    if (ImGui.Selectable(Path.GetFileName(directory), false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        this.currentFolder = directory.Equals("..") ? Path.GetFullPath(Path.Combine(this.currentFolder, "..")) : directory;
                    }

                    GenerateFolderIcon(iconPosition, lineHeight);
                }

                string[] files = Directory.GetFiles(this.currentFolder);
                foreach (string file in files)
                {
                    Vector2 iconPosition = ImGui.GetWindowPos() + ImGui.GetCursorPos();
                    iconPosition.Y -= ImGui.GetScrollY();
                    float lineHeight = ImGui.GetTextLineHeight();
                    ImGui.SetCursorPosX(lineHeight * 2);

                    if (ImGui.Selectable(Path.GetFileName(file), false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        ApplicationManager.ClearImageCache();
                        this.expectedImageInfo = this.LoadExpected(file);
                        this.actualImageInfo = this.LoadActualImage(file);
                    }

                    GenerateFileIcon(iconPosition, lineHeight);
                }


                ImGui.EndChildFrame();
            }

            ImGui.SameLine();
            if (ImGui.BeginChildFrame(2, new Vector2(size.X - 224, size.Y - 100), ImGuiWindowFlags.None))
            {
                if (this.expectedImageInfo.TexturePtr != IntPtr.Zero)
                {
                    ImGui.Text($"Expected Image ({this.expectedImageInfo.FilePath})");
                    ImGui.Image(this.expectedImageInfo.TexturePtr, this.expectedImageInfo.Size);
                }

                if (this.actualImageInfo.TexturePtr != IntPtr.Zero)
                {
                    ImGui.Text($"Actual Image ({this.actualImageInfo.FilePath})");
                    ImGui.Image(this.actualImageInfo.TexturePtr, this.actualImageInfo.Size);
                }

                if (ImGui.Button("Compare"))
                {
                    this.OpenCompare(this.expectedImageInfo.TempFilePath, this.actualImageInfo.TempFilePath);
                }

                ImGui.EndChildFrame();
            }



           
        }

        public override bool Next(WizardPage newWizardPage)
        {
            this.Wizard.GoHome();
            return false;
        }
    }
}