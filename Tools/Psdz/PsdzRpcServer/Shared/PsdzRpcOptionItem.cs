namespace PsdzRpcServer.Shared
{
    public class PsdzRpcOptionItem
    {
        public PsdzRpcOptionItem(PsdzRpcSwiRegisterEnum swiRegisterEnum, string id, string caption, bool enabled, bool selected = false)
        {
            SwiRegisterEnum = swiRegisterEnum;
            Id = id;
            Caption = caption;
            Enabled = enabled;
            Selected = selected;
        }

        public PsdzRpcSwiRegisterEnum SwiRegisterEnum { get; private set; }

        public string Id { get; private set; }

        public string Caption { get; private set; }

        public bool Enabled { get; set; }

        public bool Selected { get; set; }
    }
}