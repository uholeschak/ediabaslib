using System;
using System.Collections.Generic;
using System.Reflection;

namespace PsdzClient.Utility
{
    public static class DeepCloneUtility
    {
        private class ArrayTraverse
        {
            public int[] Position;
            private int[] maxLengths;
            public ArrayTraverse(Array array)
            {
                maxLengths = new int[array.Rank];
                for (int i = 0; i < array.Rank; i++)
                {
                    maxLengths[i] = array.GetLength(i) - 1;
                }

                Position = new int[array.Rank];
            }

            public bool Step()
            {
                for (int i = 0; i < Position.Length; i++)
                {
                    if (Position[i] < maxLengths[i])
                    {
                        Position[i]++;
                        for (int j = 0; j < i; j++)
                        {
                            Position[j] = 0;
                        }

                        return true;
                    }
                }

                return false;
            }
        }

        private static readonly MethodInfo MemberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        public static T DeepClone<T>(T original)
        {
            return (T)DeepCloneInternal(original, new Dictionary<object, object>());
        }

        private static object DeepCloneInternal(object source, IDictionary<object, object> alreadyCloned)
        {
            if (source == null)
            {
                return null;
            }

            Type type = source.GetType();
            if (type.IsTypePrimitive())
            {
                return source;
            }

            if (alreadyCloned.ContainsKey(source))
            {
                return alreadyCloned[source];
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                return null;
            }

            object obj = MemberwiseCloneMethod.Invoke(source, null);
            if (type.IsArray && !type.GetElementType().IsTypePrimitive())
            {
                HandleArray((Array)obj, alreadyCloned);
            }

            alreadyCloned.Add(source, obj);
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            CopyFields(source, alreadyCloned, obj, type, bindingFlags, null);
            CopyBaseTypePrivateFields(source, alreadyCloned, obj, type);
            return obj;
        }

        private static void CopyBaseTypePrivateFields(object source, IDictionary<object, object> visited, object clone, Type sourceType)
        {
            if (sourceType.BaseType != null)
            {
                CopyBaseTypePrivateFields(source, visited, clone, sourceType.BaseType);
                Func<FieldInfo, bool> filter = (FieldInfo fieldInfo) => fieldInfo.IsPrivate;
                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                CopyFields(source, visited, clone, sourceType.BaseType, bindingFlags, filter);
            }
        }

        private static void CopyFields(object source, IDictionary<object, object> visited, object clone, Type sourceType, BindingFlags bindingFlags, Func<FieldInfo, bool> filter)
        {
            FieldInfo[] fields = sourceType.GetFields(bindingFlags);
            foreach (FieldInfo fieldInfo in fields)
            {
                if ((filter == null || filter(fieldInfo)) && !fieldInfo.FieldType.IsTypePrimitive())
                {
                    object value = DeepCloneInternal(fieldInfo.GetValue(source), visited);
                    fieldInfo.SetValue(clone, value);
                }
            }
        }

        private static bool IsTypePrimitive(this Type type)
        {
            if (!(type == typeof(string)))
            {
                if (type.IsValueType)
                {
                    return type.IsPrimitive;
                }

                return false;
            }

            return true;
        }

        private static void HandleArray(Array clonedArray, IDictionary<object, object> alreadyCloned)
        {
            if (clonedArray.LongLength != 0L)
            {
                ArrayTraverse arrayTraverse = new ArrayTraverse(clonedArray);
                do
                {
                    object value = DeepCloneInternal(clonedArray.GetValue(arrayTraverse.Position), alreadyCloned);
                    clonedArray.SetValue(value, arrayTraverse.Position);
                }
                while (arrayTraverse.Step());
            }
        }
    }
}