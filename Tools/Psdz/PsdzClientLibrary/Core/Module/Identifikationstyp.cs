using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.Module.ISTA;

namespace BMW.Rheingold.Module.ISTA
{
    internal class Identifikationstyp : ISTAModule
    {
        public Identifikationstyp(ParameterContainer InParameter)
        {
            if (InParameter != null)
            {
                _globalModuleInParameter = InParameter;
            }
            __handleInParameter();
        }

        public virtual void Prepare()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Get_Ident_Typ(ref int Typ)
        {
            int num = 0;
            Logger.WriteInformation("Get_Ident_Typcalled");
            string text = Contexts.OrderContext.System.GetProperty("/ExternalData/VinFromVehicle") as string;
            string text2 = Contexts.OrderContext.System.GetProperty("/ExternalData/VehicleIdentification/VehicleIdentificationNumber") as string;
            if (text == null)
            {
                Typ = 0;
                num = 0;
            }
            else if (text2 == null)
            {
                Typ = 1;
                num = 1;
            }
            else if (text2 == text)
            {
                Typ = 2;
                num = 2;
            }
            else
            {
                Typ = 3;
                num = 3;
            }
            Logger.WriteInformation("_ExitIndex is: {0}", num);
        }
    }
}
