// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using ImGuiNET;

namespace SixLabors.ImageSharp.Textures.InteractiveTest.UI.WizardPages
{
    public class Welcome : WizardPage
    {
        public Welcome(Wizard wizard)
            : base(wizard)
        {
        }

        public override void Initialize()
        {
        }

        public override void Render()
        {
            this.Wizard.PreviousButton.Visible = false;
            this.Wizard.CancelButton.Visible = false;

            ImGui.TextWrapped("Welcome");
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.TextWrapped("Welcome to the ImageSharp Textures Test tool.");
        }
    }
}
