using System;
namespace Phoenix.Import.Application.UI
{
    public abstract class WizardPage
    {
        public Wizard Wizard { get; }

        protected WizardPage(Wizard wizard)
        {
            Wizard = wizard;
        }

        public virtual void Cancel()
        {
            Wizard.GoHome();
        }

        public virtual void Validate()
        {
        }

        public virtual bool Previous(WizardPage newWizardPage)
        {
            return true;
        }

        public virtual bool Next(WizardPage newWizardPage)
        {
            return true;
        }

        public abstract void Initialize();
        public abstract void Render();
    }
}
