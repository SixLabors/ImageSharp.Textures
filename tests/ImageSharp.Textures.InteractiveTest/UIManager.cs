// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using ImGuiNET;
using SixLabors.ImageSharp.Textures.InteractiveTest.UI;
using SixLabors.ImageSharp.Textures.InteractiveTest.UI.WizardPages;

namespace SixLabors.ImageSharp.Textures.InteractiveTest
{
    public class UiManager
    {
        private readonly MenuBar menuBar;
        private readonly TitleBar titleBar;
        private readonly Wizard wizard;
        private readonly WizardPage[] wizardPages;

        public UiManager()
        {
            this.menuBar = new MenuBar();
            this.titleBar = new TitleBar();

            this.wizard = new Wizard();
            this.wizardPages = new WizardPage[]
            {
                new Welcome(this.wizard),
                new Preview(this.wizard)
            };
            this.wizard.Pages = this.wizardPages.Length;

            this.wizard.OnCancel += this.CancelButton_Action;
            this.wizard.OnValidate += this.ValidateButton_Action;
            this.wizard.OnPrevious += this.PreviousButton_Action;
            this.wizard.OnNext += this.NextButton_Action;
        }

        public void Render(float width, float height)
        {
            uint backgroundColor = ImGui.GetColorU32(ImGuiCol.WindowBg);
            Vector4 newBackgroundColor = ImGui.ColorConvertU32ToFloat4(backgroundColor);
            newBackgroundColor.W = 1.0f;
            ImGui.PushStyleColor(ImGuiCol.WindowBg, newBackgroundColor);

            this.menuBar.Render(out float menuHeight);
            if (this.menuBar.DemoMode)
            {
                ImGui.ShowDemoWindow();
                return;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            if (ImGui.Begin(string.Empty, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.SetWindowPos(new Vector2(0.0f, menuHeight));
                ImGui.SetWindowSize(new Vector2(width, height - menuHeight));

                this.titleBar.Render(this.wizard.CurrentPageIndex);

                this.wizard.CancelButton.Visible = false;
                this.wizard.CancelButton.Enabled = true;
                this.wizard.CancelButton.Title = "Cancel";

                this.wizard.ValidateButton.Visible = false;
                this.wizard.ValidateButton.Enabled = true;
                this.wizard.ValidateButton.Title = "Validate";

                this.wizard.PreviousButton.Visible = true;
                this.wizard.PreviousButton.Enabled = this.wizard.CurrentPageIndex > 0;
                this.wizard.PreviousButton.Title = "Previous";

                this.wizard.NextButton.Visible = true;
                this.wizard.NextButton.Enabled = this.wizard.CurrentPageIndex < (this.wizard.Pages - 1);
                this.wizard.NextButton.Title = "Next";
                this.wizard.Render(this.RenderPage_Action);

                ImGui.End();
            }

            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
        }

        private void RenderPage_Action() => this.wizardPages[this.wizard.CurrentPageIndex].Render();

        private void CancelButton_Action() => this.wizardPages[this.wizard.CurrentPageIndex].Cancel();

        private void ValidateButton_Action() => this.wizardPages[this.wizard.CurrentPageIndex].Validate();

        private bool PreviousButton_Action(int newPageIndex) => this.wizardPages[this.wizard.CurrentPageIndex].Previous(this.wizardPages[newPageIndex]);

        private bool NextButton_Action(int newPageIndex)
        {
            bool value = this.wizardPages[this.wizard.CurrentPageIndex].Next(this.wizardPages[newPageIndex]);
            if (value)
            {
                this.wizardPages[newPageIndex].Initialize();
            }

            return value;
        }
    }
}
