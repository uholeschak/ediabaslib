using PsdzClient;

namespace PsdzRpcServer.Shared
{
    public class PsdzRpcOptionItem
    {
        public PsdzRpcOptionItem(PsdzDatabase.SwiRegisterEnum swiRegisterEnum, string caption, bool enabled, bool selected = false)
        {
            SwiRegisterEnum = swiRegisterEnum;
            Caption = caption;
            Enabled = enabled;
            Selected = selected;
        }

        public PsdzDatabase.SwiRegisterEnum SwiRegisterEnum { get; private set; }

        public string Caption { get; private set; }

        public bool Enabled { get; set; }

        public bool Selected { get; set; }
    }
}