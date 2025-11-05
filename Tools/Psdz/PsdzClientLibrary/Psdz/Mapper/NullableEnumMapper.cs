using System;

namespace BMW.Rheingold.Psdz
{
    internal abstract class NullableEnumMapper<TKey, TValue> : MapperBase<TKey?, TValue?> where TKey : struct, Enum where TValue : struct, Enum
    {
        internal override TKey? GetValue(TValue? key)
        {
            if (key.HasValue)
            {
                return base.GetValue(key);
            }

            return null;
        }

        internal override TValue? GetValue(TKey? key)
        {
            if (key.HasValue)
            {
                return base.GetValue(key);
            }

            return null;
        }
    }
}