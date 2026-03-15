using PsdzClient;

namespace PsdzRpcServer.Shared
{
    public class PsdzRpcOptionType
    {
        public PsdzRpcOptionType(PsdzDatabase.SwiRegisterEnum swiRegisterEnum, string caption)
        {
            SwiRegisterEnum = swiRegisterEnum;
            Caption = caption;
        }

        public PsdzDatabase.SwiRegisterEnum SwiRegisterEnum { get; private set; }

        public string Caption { get; private set; }
    }
}