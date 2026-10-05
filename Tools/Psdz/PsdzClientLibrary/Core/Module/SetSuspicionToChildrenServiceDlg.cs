using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    public class SetSuspicionToChildrenServiceDlg : ISTAModule
    {
        public bool p_WeiterButtonEnabledStack;

        public SetSuspicionToChildrenServiceDlg(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
            p_WeiterButtonEnabledStack = false;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void InitializeDialog(bool bCollectiveResultSetNotOk)
        {
            int num = 0;
            Logger.WriteInformation("InitializeDialogcalled");
            if (bCollectiveResultSetNotOk)
            {
                IDiagnosticObjectLocator diagnosticObjectLocator = __DiagnosticObject(null);
                if (diagnosticObjectLocator != null)
                {
                    ISPELocator[] parents = diagnosticObjectLocator.Parents;
                    if (parents != null && parents.Length != 0)
                    {
                        base._DoLoopHandling = true;
                        ISPELocator[] children = parents[0].Children;
                        foreach (ISPELocator iSPELocator in children)
                        {
                            if (iSPELocator.DataClassName.EndsWith("Control"))
                            {
                                __SetSuspiciousItem(iSPELocator as IDiagnosticObjectLocator);
                            }
                        }
                        base._DoLoopHandling = false;
                    }
                }
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
