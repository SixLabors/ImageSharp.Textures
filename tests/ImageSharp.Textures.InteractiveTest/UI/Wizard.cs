using System;
using System.Numerics;
using ImGuiNET;

namespace Phoenix.Import.Application.UI
{
    public class Wizard
    {
        public int Pages { get; set; }

        private int _currentPageIndex;
        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set
            {
                _currentPageIndex = value;
            }
        }

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
            CancelButton = new Button()
            {
                Title = "Cancel",
                Enabled = true,
                Visble = false,
                Size = new Vector2(100, 30)
            };

            ValidateButton = new Button()
            {
                Title = "Validate",
                Enabled = true,
                Visble = false,
                Size = new Vector2(100, 30)
            };

            PreviousButton = new Button()
            {
                Title = "Previous",
                Enabled = true,
                Visble = true,
                Size = new Vector2(100, 30)
            };

            NextButton = new Button()
            {
                Title = "Next",
                Enabled = true,
                Visble = true,
                Size = new Vector2(100, 30)
            };

            Pages = 1;
            CurrentPageIndex = 0;
        }

        public void Render(Action renderPage)
        {
            ImGui.BeginChild("Wizard", new Vector2(0, ImGui.GetWindowSize().Y - 88), true, ImGuiWindowFlags.None);
            renderPage?.Invoke();
            ImGui.EndChild();

            ImGui.SetCursorPos(new Vector2(8, ImGui.GetWindowSize().Y - 38));
            CancelButton.Render(CancelAction);

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X - 324, ImGui.GetWindowSize().Y - 38));
            ValidateButton.Render(ValidateAction);

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X - 216, ImGui.GetWindowSize().Y - 38));
            PreviousButton.Render(PreviousAction);

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X - 108, ImGui.GetWindowSize().Y - 38));
            NextButton.Render(NextAction);
        }

        private void CancelAction()
        {
            OnCancel?.Invoke();
        }

        private void ValidateAction()
        {
            OnValidate?.Invoke();
        }

        private void PreviousAction()
        {
            var pageIndex = CurrentPageIndex;
            if (pageIndex > 0)
            {
                pageIndex -= 1;
            }
            if (OnPrevious?.Invoke(pageIndex) ?? false)
            {
                CurrentPageIndex = pageIndex;
            }
        }

        private void NextAction()
        {
            var pageIndex = CurrentPageIndex;
            if (pageIndex < (Pages - 1))
            {
                pageIndex += 1;
            }
            if (OnNext?.Invoke(pageIndex) ?? false)
            {
                CurrentPageIndex = pageIndex;
            }
        }

        public void GoHome()
        {
            CurrentPageIndex = 0;
        }

        public void GoNext()
        {
            NextAction();
        }

    }
}
