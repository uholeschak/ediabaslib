namespace PsdzRpcServer.Shared
{
    public class PsdzRpcOptionType
    {
        public PsdzRpcOptionType(PsdzRpcSwiRegisterEnum swiRegisterEnum, PsdzSwiRegisterGroupEnum swiRegisterGroupEnum, string caption)
        {
            SwiRegisterEnum = swiRegisterEnum;
            SwiRegisterGroupEnum = swiRegisterGroupEnum;
            Caption = caption;
        }

        public PsdzRpcSwiRegisterEnum SwiRegisterEnum { get; private set; }

        public PsdzSwiRegisterGroupEnum SwiRegisterGroupEnum { get; private set; }

        public string Caption { get; private set; }
    }
}