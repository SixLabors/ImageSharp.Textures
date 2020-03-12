using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ImGuiNET;

namespace Phoenix.Import.Application.UI.WizardPages
{
    public class Select : WizardPage
    {
        public Select(Wizard wizard) : base(wizard)
        {
        }

        public override void Initialize()
        {
        }

        public override void Render()
        {
            Wizard.CancelButton.Visble = false;

            ImGui.TextWrapped("Select...");
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.TextWrapped("TODO");
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Indent();
            ImGui.AlignTextToFramePadding();
        }

        public override bool Next(WizardPage newWizardPage)
        {
            return true;
        }
    }
}
