using System;
using System.Numerics;
using System.Threading;
using ImGuiNET;
using Phoenix.Import.Application.UI;
using Phoenix.Import.Application.UI.WizardPages;

namespace Phoenix.Import.Application
{
    public class UiManager
    {
        private readonly MenuBar _menuBar;
        private readonly TitleBar _titleBar;
        private readonly Wizard _wizard;
        private readonly WizardPage[] _wizardPages;
        private bool _busy;

        public UiManager()
        {
            _menuBar = new MenuBar();
            _titleBar = new TitleBar();

            _wizard = new Wizard();
            _wizardPages = new WizardPage[]
            {
                new Welcome(_wizard),
                new Select(_wizard),
                new Review(_wizard)
            };
            _wizard.Pages = _wizardPages.Length;

            _wizard.OnCancel += CancelButton_Action;
            _wizard.OnValidate += ValidateButton_Action;
            _wizard.OnPrevious += PreviousButton_Action;
            _wizard.OnNext += NextButton_Action;
        }



        public void Render(float width, float height)
        {
            var backgroundColor = ImGui.GetColorU32(ImGuiCol.WindowBg);
            var newBackgroundColor = ImGui.ColorConvertU32ToFloat4(backgroundColor);
            newBackgroundColor.W = 1.0f;
            ImGui.PushStyleColor(ImGuiCol.WindowBg, newBackgroundColor);

            _menuBar.Render(out var menuHeight);
            if (_menuBar.DemoMode)
            {
                ImGui.ShowDemoWindow();
                return;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            if (ImGui.Begin("", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.SetWindowPos(new Vector2(0.0f, menuHeight));
                ImGui.SetWindowSize(new Vector2(width, height - menuHeight));
        
                _titleBar.Render(_wizard.CurrentPageIndex);

                _wizard.CancelButton.Visble = false;
                _wizard.CancelButton.Enabled = true;
                _wizard.CancelButton.Title = "Cancel";

                _wizard.ValidateButton.Visble = false;
                _wizard.ValidateButton.Enabled = true;
                _wizard.ValidateButton.Title = "Validate";

                _wizard.PreviousButton.Visble = true;
                _wizard.PreviousButton.Enabled = _wizard.CurrentPageIndex > 0;
                _wizard.PreviousButton.Title = "Previous";

                _wizard.NextButton.Visble = true;
                _wizard.NextButton.Enabled = _wizard.CurrentPageIndex < (_wizard.Pages - 1);
                _wizard.NextButton.Title = "Next";
                _wizard.Render(RenderPage_Action);

                if (_busy)
                {
                    var position = ImGui.GetWindowPos() + (ImGui.GetWindowSize() / 2);
                    Widgets.RenderSpinner(position, 20, 3);
                }

                ImGui.End();
            }
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();
        }

        private void RenderPage_Action()
        {
            _wizardPages[_wizard.CurrentPageIndex].Render();
        }

        private void CancelButton_Action()
        {
            _wizardPages[_wizard.CurrentPageIndex].Cancel();
        }

        private void ValidateButton_Action()
        {
            _wizardPages[_wizard.CurrentPageIndex].Validate();
        }

        private bool PreviousButton_Action(int newPageIndex)
        {
            return _wizardPages[_wizard.CurrentPageIndex].Previous(_wizardPages[newPageIndex]);
        }

        private bool NextButton_Action(int newPageIndex)
        {
            var value = _wizardPages[_wizard.CurrentPageIndex].Next(_wizardPages[newPageIndex]);
            if (value)
            {
                _wizardPages[newPageIndex].Initialize();
            }
            return value;
        }

    }
}
