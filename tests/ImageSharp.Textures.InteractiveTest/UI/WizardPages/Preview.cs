// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace Phoenix.Import.Application.UI.WizardPages
{
    public class Preview : WizardPage
    {
        public struct ImageInfo
        {
            public IntPtr TexturePtr;
            public Vector2 Size;
            public string FilePath;
            public string TempFilePath;
            public string ErrorMessage;
        }

        private string rootFolder;
        private string currentFolder;
        private string currentFile;

        private ImageInfo expectedImageInfo;
        private ImageInfo actualImageInfo;

        public Preview(Wizard wizard)
            : base(wizard)
        {
            this.rootFolder = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Dds", "Flat");
            this.currentFolder = this.rootFolder;
        }

        public void OpenCompare(string filePath1, string filePath2)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string command = @"C:\Program Files\Beyond Compare 4\bcomp.exe";
                Process.Start(new ProcessStartInfo(command, $"\"{filePath1}\" \"{filePath2}\" /fv=\"Picture Compare\""));
            }
        }

        public string DecompressDds(string filePath)
        {
            string command = Path.Combine(TestEnvironment.ToolsDirectoryFullPath, "TexConv.exe");
            var process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = $"-ft PNG \"{filePath}\" -f rgba -o {Path.GetTempPath()}";
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            string sourceFile = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(filePath)}.png");
            if (!File.Exists(sourceFile))
            {
                throw new Exception(process.StandardOutput.ReadToEnd());
            }

            string saveFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            File.Move(sourceFile, saveFilePath);
            return saveFilePath;
        }

        public ImageInfo LoadExpected(string filePath)
        {
            try
            {
                string ddsSaveFilePath = this.DecompressDds(filePath);
                using var clone = Image.Load<Rgba32>(ddsSaveFilePath);
                return new ImageInfo { TexturePtr = ApplicationManager.Create(clone), Size = new Vector2(clone.Width, clone.Height), FilePath = filePath, TempFilePath = ddsSaveFilePath };
            }
            catch (Exception ex)
            {
                return new ImageInfo { TexturePtr = IntPtr.Zero, Size = Vector2.Zero, FilePath = filePath, TempFilePath = string.Empty, ErrorMessage = ex.ToString() };
            }
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
                clone.Save(saveFilePath);
                return new ImageInfo { TexturePtr = ApplicationManager.Create(clone), Size = new Vector2(clone.Width, clone.Height), FilePath = filePath, TempFilePath = saveFilePath };
            }
            catch (Exception ex)
            {
                return new ImageInfo { TexturePtr = IntPtr.Zero, Size = Vector2.Zero, FilePath = filePath, TempFilePath = string.Empty, ErrorMessage = ex.ToString() };
            }
        }

        public override void Initialize()
        {
            ApplicationManager.ClearImageCache();
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
            Vector2[] points = new[]
            {
                new Vector2(0.0f, 0.0f), new Vector2(45.0f, 0.0f),
                new Vector2(45.0f, 0.0f), new Vector2(55.0f, 22.5f),
                new Vector2(55.0f, 22.5f), new Vector2(100.0f, 22.5f),
                new Vector2(100.0f, 22.5f), new Vector2(100.0f, 87.5f),
                new Vector2(100.0f, 87.5f), new Vector2(0.0f, 87.5f),
                new Vector2(0.0f, 87.5f), new Vector2(0.0f, 0.0f)
            };
            DrawLines(points, location, size);
        }

        private static void GenerateFileIcon(Vector2 location, float size)
        {
            Vector2[] points = new[]
            {
                new Vector2(12.5f, 0.0f), new Vector2(62.5f, 0.0f),
                new Vector2(62.5f, 0.0f), new Vector2(87.5f, 50.0f),
                new Vector2(87.5f, 50.0f), new Vector2(87.5f, 100.0f),
                new Vector2(87.5f, 100.0f), new Vector2(12.5f, 100.0f),
                new Vector2(12.5f, 100.0f), new Vector2(12.5f, 0.0f),
                new Vector2(62.5f, 0.0f), new Vector2(62.5f, 50.0f),
                new Vector2(62.5f, 50.0f), new Vector2(87.5f, 50.0f)
            };
            DrawLines(points, location, size);
        }

        public override void Render()
        {
            this.Wizard.NextButton.Enabled = true;
            this.Wizard.PreviousButton.Visible = false;
            this.Wizard.NextButton.Title = "Home";

            Vector2 size = ImGui.GetWindowSize();

            ImGui.PushItemWidth(size.X - 16);
            ImGui.PopItemWidth();
            ImGui.Spacing();

            if (ImGui.BeginChildFrame(1, new Vector2(200, size.Y - 24), ImGuiWindowFlags.None))
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
                        this.currentFile = null;
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
                    if (ImGui.Selectable(Path.GetFileName(file), string.Equals(file, this.currentFile, StringComparison.CurrentCultureIgnoreCase), ImGuiSelectableFlags.DontClosePopups))
                    {
                        ApplicationManager.ClearImageCache();
                        this.currentFile = file;
                        this.expectedImageInfo = this.LoadExpected(file);
                        this.actualImageInfo = this.LoadActualImage(file);
                    }

                    GenerateFileIcon(iconPosition, lineHeight);
                }

                ImGui.EndChildFrame();
            }

            ImGui.SameLine();
            if (ImGui.BeginChildFrame(2, new Vector2(size.X - 224, size.Y - 24), ImGuiWindowFlags.None))
            {
                ImGui.Text("Expected Image");
                if (this.expectedImageInfo.TexturePtr != IntPtr.Zero)
                {
                    ImGui.Image(this.expectedImageInfo.TexturePtr, this.expectedImageInfo.Size);
                }
                else if (this.expectedImageInfo.ErrorMessage != null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                    ImGui.TextWrapped(this.expectedImageInfo.ErrorMessage);
                    ImGui.PopStyleColor();
                }

                ImGui.Spacing();

                ImGui.Text("Actual Image");
                if (this.actualImageInfo.TexturePtr != IntPtr.Zero)
                {
                    ImGui.Image(this.actualImageInfo.TexturePtr, this.actualImageInfo.Size);
                }
                else if (this.actualImageInfo.ErrorMessage != null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                    ImGui.TextWrapped(this.actualImageInfo.ErrorMessage);
                    ImGui.PopStyleColor();
                }

                ImGui.Spacing();

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
