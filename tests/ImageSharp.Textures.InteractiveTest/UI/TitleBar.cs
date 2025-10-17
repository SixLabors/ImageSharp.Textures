// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using ImGuiNET;

namespace SixLabors.ImageSharp.Textures.InteractiveTest.UI
{
    public class TitleBar
    {
        public void Render(int page)
        {
            Vector2 position = ImGui.GetCursorScreenPos();

            if (ImGui.BeginChild("TitleBar", new Vector2(0, 30), true, ImGuiWindowFlags.None))
            {
                Vector2 size = ImGui.GetWindowSize();

                Vector4 inactiveColor = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Text));
                Vector4 activeColor = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ButtonActive));

                ImGui.TextColored(page == 0 ? activeColor : inactiveColor, "Welcome");
                ImGui.SameLine(0, 16);
                ImGui.TextColored(page == 1 ? activeColor : inactiveColor, "Preview");

                ImGui.EndChild();
                ImGui.GetWindowDrawList().AddRectFilled(position, position + size, ImGui.GetColorU32(ImGuiCol.TitleBgActive));
            }
        }
    }
}
