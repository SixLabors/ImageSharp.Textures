using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace Phoenix.Import.Application.UI
{
    public class InputDialog
    {
        private bool _showModal;
        private bool _open;
        private string _title;
        private string _description;

        private string _value;

        public string Value
        {
            get { return _value; }
        }

        public bool Cancelled { get; private set; }

        public void ShowModal(string title, string description)
        {
            _showModal = true;
            _title = title;
            _description = description;
            _value = string.Empty;
        }

        private void CloseModal()
        {
            _open = false;
            ImGui.CloseCurrentPopup();
        }
        
        public bool Render()
        {
            if (_showModal)
            {
                _showModal = false;
                _open = true;
                ImGui.OpenPopup(_title);
            }

            if (!_open)
            {
                return false;
            }

            if (!ImGui.BeginPopupModal(_title))
            {
                return false;
            }

            var result = false;

            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetWindowSize(new Vector2(224, 200));
            }

            var size = ImGui.GetWindowSize();

            ImGui.TextWrapped(_description);
            ImGui.Spacing();

            var yPos = ImGui.GetCursorPosY();

            ImGui.InputTextMultiline("##input-text", ref _value, 1000, new Vector2(size.X - 16, size.Y - (46 + yPos)), ImGuiInputTextFlags.None);
         
            ImGui.Spacing();
            ImGui.SetCursorPosX(size.X - 216);
            if (ImGui.Button("Cancel", new Vector2(100, 30)))
            {
                Cancelled = true;
                result = true;
                CloseModal();
            }
            ImGui.SameLine();
            if (ImGui.Button("Ok", new Vector2(100, 30)))
            {
                if (Value.Length > 0)
                {
                    Cancelled = false;
                    result = true;
                    CloseModal();
                }
            }

            ImGui.EndPopup();

            return result;
        }
    }
}
