using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace BMW.Rheingold.Module.ISTA
{
    public static class GraphUtility
    {
        private static readonly int[] TEXT_WHITE_FOREGROUNDS = new int[7] { 1, 2, 3, 4, 6, 11, 12 };

        public static void AddOrReplace<T>(this List<T> collection, int index, T value)
        {
            if (collection.Count > index)
            {
                collection[index] = value;
            }
            else
            {
                collection.Add(value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ClearEventInvocations(this object obj, string eventName)
        {
            FieldInfo eventField = obj.GetType().GetEventField(eventName);
            if (!(eventField == null))
            {
                eventField.SetValue(obj, null);
            }
        }

        public static Brush GetColorBrushFromNumber(int colorNumber)
        {
            switch (colorNumber)
            {
                case 1:
                    return Brushes.Black;
                case 2:
                    return Brushes.Blue;
                case 3:
                    return Brushes.Green;
                case 4:
                    return Brushes.Red;
                case 5:
                    return Brushes.Yellow;
                case 6:
                    return Brushes.Gray;
                case 7:
                    return Brushes.LightBlue;
                case 8:
                    return Brushes.LightGreen;
                case 9:
                    return Brushes.Salmon;
                case 10:
                    return Brushes.Orange;
                case 11:
                    return Brushes.Purple;
                case 12:
                    return Brushes.Brown;
                case 13:
                    return Brushes.Magenta;
                case 14:
                    return new SolidColorBrush(Color.FromRgb(179, 143, 238));
                case 15:
                    return new SolidColorBrush(Color.FromRgb(217, 217, 217));
                case 16:
                    return Brushes.Yellow;
                case 17:
                    return Brushes.Gray;
                case 18:
                    return Brushes.LightBlue;
                case 19:
                    return Brushes.LightGreen;
                case 20:
                    return Brushes.Salmon;
                case 21:
                    return Brushes.Orange;
                case 22:
                    return Brushes.Purple;
                case 23:
                    return Brushes.Brown;
                case 24:
                    return Brushes.Blue;
                case 25:
                    return Brushes.Green;
                case 26:
                    return Brushes.Red;
                case 27:
                    return Brushes.Yellow;
                case 28:
                    return Brushes.Gray;
                case 29:
                    return Brushes.LightBlue;
                case 30:
                    return Brushes.LightGreen;
                default:
                    return Brushes.Black;
            }
        }

        public static DoubleCollection GetDoubleCollectionFromStyleNumber(int styleNumber)
        {
            DoubleCollection result = null;
            switch (styleNumber)
            {
                case 2:
                    result = new DoubleCollection(new double[2] { 4.0, 2.0 });
                    break;
                case 3:
                    result = new DoubleCollection(new double[2] { 1.0, 2.0 });
                    break;
                case 4:
                    result = new DoubleCollection(new double[4] { 8.0, 8.0, 2.0, 8.0 });
                    break;
            }
            return result;
        }

        public static Rect GetFrameAdjustedBounds(Rect bounds, bool ignoreFrameHorizontalSides = false, bool ignoreFrameVerticalSides = false, bool useFullThicknessHorizontal = false, bool useFullThicknessVertical = false)
        {
            double num = (useFullThicknessHorizontal ? GraphConstants.FRAME_THICKNESS : (GraphConstants.FRAME_THICKNESS / 2.0));
            double num2 = (useFullThicknessVertical ? GraphConstants.FRAME_THICKNESS : (GraphConstants.FRAME_THICKNESS / 2.0));
            double num3 = (ignoreFrameHorizontalSides ? 0.0 : num);
            double num4 = (ignoreFrameVerticalSides ? 0.0 : num2);
            Rect result = new Rect(bounds.Location, bounds.Size);
            result.Inflate(0.0 - num3, 0.0 - num4);
            return result;
        }

        public static double GetPointsSquareDistance(double x1, double y1, double x2, double y2)
        {
            return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
        }

        public static Brush GetTextForegroundBrush(int backgroundColorNumber)
        {
            if (TEXT_WHITE_FOREGROUNDS.Contains(backgroundColorNumber))
            {
                return Brushes.White;
            }
            return Brushes.Black;
        }

        public static double GetZoomedMaxValue(double maxValue, double minValue, double zoom)
        {
            return (maxValue - minValue) / Math.Pow(GraphConstants.ZOOM_FACTOR, zoom - 1.0) + minValue;
        }

        public static double GetZoomedValue(double value, double zoom)
        {
            return value * Math.Pow(GraphConstants.ZOOM_FACTOR, zoom - 1.0);
        }

        public static double MapValueToRange(double value, double from1, double from2, double to1, double to2, bool reverseDestinationRange = false)
        {
            if (from1 == from2)
            {
                return value;
            }
            if (reverseDestinationRange)
            {
                double num = to1;
                to1 = to2;
                to2 = num;
            }
            return (value - from1) / (from2 - from1) * (to2 - to1) + to1;
        }

        private static FieldInfo GetEventField(this Type type, string eventName)
        {
            FieldInfo fieldInfo = null;
            while (type != null)
            {
                fieldInfo = type.GetField(eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                if (fieldInfo != null && (fieldInfo.FieldType == typeof(MulticastDelegate) || fieldInfo.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                {
                    break;
                }
                fieldInfo = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
                if (fieldInfo != null)
                {
                    break;
                }
                type = type.BaseType;
            }
            return fieldInfo;
        }
    }
}
