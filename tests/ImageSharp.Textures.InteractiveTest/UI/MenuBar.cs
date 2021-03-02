// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using ImGuiNET;

namespace SixLabors.ImageSharp.Textures.InteractiveTest.UI
{
    public class MenuBar
    {
        public bool DarkMode { get; private set; }

        public bool DemoMode { get; private set; }

        public MenuBar()
        {
            this.DarkMode = true;
            this.DemoMode = false;
        }

        public void Render(out float menuHeight)
        {
            if (this.DarkMode)
            {
                ImGui.StyleColorsDark();
            }
            else
            {
                ImGui.StyleColorsLight();
            }

            menuHeight = 0.0f;
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Theme"))
                {
                    if (ImGui.MenuItem("Light", string.Empty, !this.DarkMode, this.DarkMode))
                    {
                        this.DarkMode = false;
                    }

                    if (ImGui.MenuItem("Dark", string.Empty, this.DarkMode, !this.DarkMode))
                    {
                        this.DarkMode = true;
                    }

                    // if (ImGui.MenuItem("Demo", "", DemoMode))
                    // {
                    //     DemoMode = !DemoMode;
                    // }
                    ImGui.EndMenu();
                }

                menuHeight = ImGui.GetWindowHeight();
                ImGui.EndMainMenuBar();
            }
        }
    }
}
