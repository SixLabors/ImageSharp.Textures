using System;
using System.Numerics;
using ImGuiNET;

namespace Phoenix.Import.Application.UI
{
    public class TitleBar
    {
        public void Render(int page)
        {
            var position = ImGui.GetCursorScreenPos();

            if (ImGui.BeginChild("TitleBar", new Vector2(0, 30), true, ImGuiWindowFlags.None))
            {
                var size = ImGui.GetWindowSize();

                var inactiveColor = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Text));
                var activeColor = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.ButtonActive));

                ImGui.TextColored(page == 0 ? activeColor : inactiveColor, "Welcome");
                ImGui.SameLine(0, 16);
                ImGui.TextColored(page == 1 ? activeColor : inactiveColor, "Select");
                ImGui.SameLine(0, 16);
                ImGui.TextColored(page == 2 ? activeColor : inactiveColor, "Review");

                ImGui.EndChild();
                ImGui.GetWindowDrawList().AddRectFilled(position, position + size, ImGui.GetColorU32(ImGuiCol.TitleBgActive));
            }
        }
    }
}
