// ReSharper disable once CheckNamespace
namespace EdiabasLib
{
    public class EdBluetoothInterfaceBase
    {
        public static int CurrentBaudRate { get; protected set; }

        public static int CurrentWordLength { get; protected set; }

        public static EdInterfaceObd.SerialParity CurrentParity { get; protected set; }

        static EdBluetoothInterfaceBase()
        {
            CurrentBaudRate = 0;
            CurrentWordLength = 0;
            CurrentParity = EdInterfaceObd.SerialParity.None;
        }
    }
}
