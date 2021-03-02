// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using ImGuiNET;

namespace SixLabors.ImageSharp.Textures.InteractiveTest.UI
{
    public class Wizard
    {
        public int Pages { get; set; }

        public int CurrentPageIndex { get; set; }

        public Action OnCancel { get; set; }

        public Action OnValidate { get; set; }

        public Func<int, bool> OnPrevious { get; set; }

        public Func<int, bool> OnNext { get; set; }

        public Button CancelButton { get; private set; }

        public Button ValidateButton { get; private set; }

        public Button PreviousButton { get; private set; }

        public Button NextButton { get; private set; }

        public Wizard()
        {
            this.CancelButton = new Button()
            {
                Title = "Cancel",
                Enabled = true,
                Visible = false,
                Size = new Vector2(100, 30)
            };

            this.ValidateButton = new Button()
            {
                Title = "Validate",
                Enabled = true,
                Visible = false,
                Size = new Vector2(100, 30)
            };

            this.PreviousButton = new Button()
            {
                Title = "Previous",
                Enabled = true,
                Visible = true,
                Size = new Vector2(100, 30)
            };

            this.NextButton = new Button()
            {
                Title = "Next",
                Enabled = true,
                Visible = true,
                Size = new Vector2(100, 30)
            };

            this.Pages = 1;
            this.CurrentPageIndex = 0;
        }

        public void Render(Action renderPage)
        {
            ImGui.BeginChild("Wizard", new Vector2(0, ImGui.GetWindowSize().Y - 88), true, ImGuiWindowFlags.None);
            renderPage?.Invoke();
            ImGui.EndChild();

            ImGui.SetCursorPos(new Vector2(8, ImGui.GetWindowSize().Y - 38));
            this.CancelButton.Render(this.CancelAction);

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X - 324, ImGui.GetWindowSize().Y - 38));
            this.ValidateButton.Render(this.ValidateAction);

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X - 216, ImGui.GetWindowSize().Y - 38));
            this.PreviousButton.Render(this.PreviousAction);

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X - 108, ImGui.GetWindowSize().Y - 38));
            this.NextButton.Render(this.NextAction);
        }

        private void CancelAction()
        {
            this.OnCancel?.Invoke();
        }

        private void ValidateAction()
        {
            this.OnValidate?.Invoke();
        }

        private void PreviousAction()
        {
            int pageIndex = this.CurrentPageIndex;
            if (pageIndex > 0)
            {
                pageIndex -= 1;
            }

            if (this.OnPrevious?.Invoke(pageIndex) ?? false)
            {
                this.CurrentPageIndex = pageIndex;
            }
        }

        private void NextAction()
        {
            int pageIndex = this.CurrentPageIndex;
            if (pageIndex < (this.Pages - 1))
            {
                pageIndex += 1;
            }

            if (this.OnNext?.Invoke(pageIndex) ?? false)
            {
                this.CurrentPageIndex = pageIndex;
            }
        }

        public void GoHome()
        {
            this.CurrentPageIndex = 0;
        }

        public void GoNext()
        {
            this.NextAction();
        }
    }
}
