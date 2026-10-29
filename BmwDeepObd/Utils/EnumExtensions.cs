using System.ComponentModel;

namespace BmwDeepObd
{
    public static class EnumExtensions
    {
        public static string ToDescriptionString(this ActivityCommon.InterfaceType val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
                .GetType()
                .GetField(val.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
