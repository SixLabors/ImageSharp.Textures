namespace Phoenix.Import.Application.UI
{
    public abstract class WizardPage
    {
        public Wizard Wizard { get; }

        protected WizardPage(Wizard wizard)
        {
            this.Wizard = wizard;
        }

        public virtual void Cancel()
        {
            this.Wizard.GoHome();
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
