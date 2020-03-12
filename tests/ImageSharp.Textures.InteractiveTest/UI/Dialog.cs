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
    public class Dialog
    {
        private bool _showModal;
        private bool _open;
        private string _title;
        private string _description;
        private string _cancelButton;
        private string _okButton;

        public bool Cancelled { get; private set; }

        public void ShowModal(string title, string description, string cancelButton, string okButton)
        {
            _showModal = true;
            _title = title;
            _description = description;
            _cancelButton = cancelButton;
            _okButton = okButton;
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
                ImGui.SetWindowSize(new Vector2(224, 100));
            }

            var size = ImGui.GetWindowSize();

            ImGui.TextWrapped(_description);
            ImGui.Spacing();
            ImGui.SetCursorPosX(size.X - 216);
            if (ImGui.Button(_cancelButton, new Vector2(100, 30)))
            {
                Cancelled = true;
                result = true;
                CloseModal();
            }
            ImGui.SameLine();
            if (ImGui.Button(_okButton, new Vector2(100, 30)))
            {
                Cancelled = false;
                result = true;
                CloseModal();               
            }

            ImGui.EndPopup();

            return result;
        }
    }
}
