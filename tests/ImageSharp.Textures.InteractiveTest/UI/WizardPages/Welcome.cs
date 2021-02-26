// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using ImGuiNET;

namespace Phoenix.Import.Application.UI.WizardPages
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
