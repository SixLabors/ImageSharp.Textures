// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using ImGuiNET;

namespace SixLabors.ImageSharp.Textures.InteractiveTest.UI
{
    public class Button
    {
        public Vector2 Size { get; set; } = new Vector2(100, 30);

        public string Title { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public bool Visible { get; set; } = true;

        public void Render(Action clicked)
        {
            if (!this.Visible)
            {
                return;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * (this.Enabled ? 1.0f : 0.5f));
            if (ImGui.Button(this.Title, this.Size))
            {
                if (this.Enabled)
                {
                    clicked?.Invoke();
                }
            }

            ImGui.PopStyleVar();
        }
    }
}
