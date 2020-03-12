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
    public class AlertDialog
    {
        private bool _showModal;
        private bool _open;
        private string _title;
        private string _description;
        
        public void ShowModal(string title, string description)
        {
            _showModal = true;
            _title = title;
            _description = description;
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

            if (!ImGui.BeginPopupModal(_title, ref _open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                return false;
            }

            var result = false;

            var size = ImGui.GetWindowSize();

            ImGui.Text(_description);
            ImGui.Spacing();

            ImGui.Spacing();
            ImGui.SetCursorPosX(size.X - 116);
            if (ImGui.Button("Ok", new Vector2(100, 30)))
            {
                result = true;
                CloseModal();
            }

            ImGui.EndPopup();

            return result;
        }
    }
}
