using BMW.Rheingold.ISTA.CoreFramework.SOCAccessor;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core.Container;
using System;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    internal class SYS_VAR_ISTA : ISTAModule
    {
        public string strDlgInfo;
        public bool p_WeiterButtonEnabledStack;
        public SYS_VAR_ISTA(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }

            __handleInParameter();
            strDlgInfo = "03.06.2008-SYS_VAR_ISTA";
            p_WeiterButtonEnabledStack = false;
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void InitializeDialog(ref string[] ISTA_Systemvariable)
        {
            int num = 0;
            Logger.WriteInformation("InitializeDialogcalled");
            Logger.WriteInformation(strDlgInfo);
            ISTA_Systemvariable = new string[20];
            _DoLoopHandling = true;
            for (int i = 0; i < ISTA_Systemvariable.Length; i++)
            {
                ISTA_Systemvariable[i] = "";
            }

            _DoLoopHandling = false;
            if (SOCAccessor.SessionContext.GetProperty("/UserContext/UserData/Country")is string text)
            {
                ISTA_Systemvariable[0] = text;
            }

            ISTA_Systemvariable[1] = DateTime.Now.ToString("dd.MM.yyyy");
            ISTA_Systemvariable[2] = DateTime.Now.ToString("HH:mm:ss");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void LandTester(ref string PLandTester)
        {
            int num = 0;
            Logger.WriteInformation("LandTestercalled");
            if (SOCAccessor.SessionContext.GetProperty("/UserContext/UserData/Country")is string text)
            {
                PLandTester = text;
            }

            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void DatumTester(ref string PDatumTester)
        {
            int num = 0;
            Logger.WriteInformation("DatumTestercalled");
            PDatumTester = DateTime.Now.ToString("dd.MM.yyyy");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }

        public virtual void UhrzeitTester(ref string PUhrzeitTester)
        {
            int num = 0;
            Logger.WriteInformation("UhrzeitTestercalled");
            PUhrzeitTester = DateTime.Now.ToString("HH:mm:ss");
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}