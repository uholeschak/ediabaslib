using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    public class SidisButton
    {
        private IModuleExecutionParent parent;

        public bool Enabled
        {
            get
            {
                return parent.IsNextEnabled;
            }
            set
            {
                parent.IsNextEnabled = value;
            }
        }

        public SidisButton(IModuleExecutionParent parent)
        {
            this.parent = parent;
        }
    }
}
