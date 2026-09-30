using BMW.Rheingold.CoreFramework;
using System;

#pragma warning disable CS0169
namespace BMW.Rheingold.Module.ISTA
{
    internal class SidisPanel
    {
        private IModuleExecutionParent parent;

        private SidisButton forwardButton;

        public SidisButton Forward => forwardButton;

        public SidisPanel(IModuleExecutionParent parent)
        {
            if (parent == null)
            {
                throw new ArgumentException("Parameter parent must not be null.");
            }
            forwardButton = new SidisButton(parent);
        }
    }
}
