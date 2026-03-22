using PsdzClient;

namespace PsdzRpcServer.Shared
{
    public class PsdzRpcOptionType
    {
        public PsdzRpcOptionType(PsdzRpcSwiRegisterEnum swiRegisterEnum, string caption)
        {
            SwiRegisterEnum = swiRegisterEnum;
            Caption = caption;
        }

        public PsdzRpcSwiRegisterEnum SwiRegisterEnum { get; private set; }

        public string Caption { get; private set; }
    }
}