using BMW.Authoring.API.MetaData;
using PsdzClient.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BMW.Authoring.API.MetaData.Enum;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public static class DataFieldFactory
    {
        private static Dictionary<string, string> SimpleTypeMappingDictionary = new Dictionary<string, string>
    {
        {
            typeof(string).FullName,
            "string"
        },
        {
            typeof(bool).FullName,
            "bool"
        },
        {
            typeof(DateTime).FullName,
            "datetime"
        },
        {
            typeof(int).FullName,
            "int"
        },
        {
            typeof(uint).FullName,
            "uint"
        },
        {
            typeof(long).FullName,
            "long"
        },
        {
            typeof(ulong).FullName,
            "ulong"
        },
        {
            typeof(short).FullName,
            "short"
        },
        {
            typeof(ushort).FullName,
            "ushort"
        },
        {
            typeof(float).FullName,
            "float"
        },
        {
            typeof(double).FullName,
            "double"
        },
        {
            typeof(decimal).FullName,
            "decimal"
        },
        {
            typeof(bool?).FullName,
            "bool"
        },
        {
            typeof(DateTime?).FullName,
            "datetime"
        },
        {
            typeof(int?).FullName,
            "int"
        },
        {
            typeof(uint?).FullName,
            "uint"
        },
        {
            typeof(long?).FullName,
            "long"
        },
        {
            typeof(ulong?).FullName,
            "ulong"
        },
        {
            typeof(short?).FullName,
            "short"
        },
        {
            typeof(ushort?).FullName,
            "ushort"
        },
        {
            typeof(float?).FullName,
            "float"
        },
        {
            typeof(double?).FullName,
            "double"
        },
        {
            typeof(decimal?).FullName,
            "decimal"
        }
    };

        [Obsolete("Please use the new function 'CreateStringDataFieldWithParamId'")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateStringDataField(bool mandatory, bool isEditable, string paramName, ITextLocator paramDescription, string paramValue, int? stringInputDataType, int? minLength = null, int? maxLength = null)
        {
            return CreateStringDataFieldWithParamId(mandatory, isEditable, paramName, new TextLocator(paramName), paramDescription, paramValue, stringInputDataType, minLength, maxLength);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateStringDataFieldWithParamId(bool mandatory, bool isEditable, string paramId, ITextLocator paramName, ITextLocator paramDescription, string paramValue, int? stringInputDataType, int? minLength = null, int? maxLength = null)
        {
            DefaultChecks(paramId, paramName, paramDescription);
            StringInputDataType? stringInputDataType2 = null;
            if (!stringInputDataType.HasValue || (stringInputDataType < 0 && stringInputDataType > 3))
            {
                Log.Warning("CreateStringDataField", "'stringInputDataType' must can only be 0 = Numerical, 1 = Alphanumerical, 2 = Decimal or 3 = HEX. So the Alphanumeric value is taken!");
                stringInputDataType = 1;
            }
            stringInputDataType2 = (StringInputDataType)stringInputDataType.Value;
            if (!maxLength.HasValue)
            {
                switch (stringInputDataType2)
                {
                    case StringInputDataType.Numerical:
                    case StringInputDataType.Decimal:
                        maxLength = ((!maxLength.HasValue || maxLength > 38) ? new int?(38) : maxLength);
                        break;
                    case StringInputDataType.Hex:
                        maxLength = ((!maxLength.HasValue || maxLength > 6) ? new int?(6) : maxLength);
                        break;
                }
            }
            return new DataField
            {
                Type = GetSimpleType(typeof(string)),
                DataFieldType = DataFieldType.StringDataField,
                Id = paramId,
                Name = paramName.TextContent.PlainText,
                Description = paramDescription.TextContent.PlainText,
                Value = (paramValue ?? ""),
                MaxLength = maxLength,
                MinLength = minLength,
                StringDataType = stringInputDataType2,
                Mandatory = mandatory,
                IsEditable = isEditable
            };
        }

        [Obsolete("Please use the new function 'CreateBoolDataFieldWithParamId'")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateBoolDataField(bool mandatory, bool isEditable, string paramName, ITextLocator paramDescription, bool? paramValue = false)
        {
            return CreateBoolDataFieldWithParamId(mandatory, isEditable, paramName, new TextLocator(paramName), paramDescription, paramValue);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateBoolDataFieldWithParamId(bool mandatory, bool isEditable, string paramId, ITextLocator paramName, ITextLocator paramDescription, bool? paramValue = false)
        {
            DefaultChecks(paramId, paramName, paramDescription);
            return new DataField
            {
                Mandatory = mandatory,
                IsEditable = isEditable,
                Type = GetSimpleType(typeof(bool?)),
                DataFieldType = DataFieldType.BoolDataField,
                Id = paramId,
                Name = paramName.TextContent.PlainText,
                Description = paramDescription.TextContent.PlainText,
                Value = paramValue
            };
        }

        [Obsolete("Please use the new function 'CreateDateTimeDataFieldWithParamId'")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateDateTimeDataField(bool mandatory, bool isEditable, string paramName, ITextLocator paramDescription, DateTime? paramValue, DateTime? minDate = null, DateTime? maxDate = null)
        {
            return CreateDateTimeDataFieldWithParamId(mandatory, isEditable, paramName, new TextLocator(paramName), paramDescription, paramValue, minDate, maxDate);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateDateTimeDataFieldWithParamId(bool mandatory, bool isEditable, string paramId, ITextLocator paramName, ITextLocator paramDescription, DateTime? paramValue, DateTime? minDate = null, DateTime? maxDate = null)
        {
            DefaultChecks(paramId, paramName, paramDescription);
            return new DataField
            {
                Mandatory = mandatory,
                IsEditable = isEditable,
                Type = GetSimpleType(typeof(DateTime?)),
                DataFieldType = DataFieldType.DateTimeDataField,
                Id = paramId,
                Name = paramName.TextContent.PlainText,
                Description = paramDescription.TextContent.PlainText,
                Value = paramValue,
                MinDate = minDate,
                MaxDate = maxDate
            };
        }

        [Obsolete("Please use the new functon 'CreateDateDataFieldWithParamId'")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateDateDataField(bool mandatory, bool isEditable, string paramName, ITextLocator paramDescription, DateTime? paramValue, DateTime? minDate = null, DateTime? maxDate = null)
        {
            return CreateDateDataFieldWithParamId(mandatory, isEditable, paramName, new TextLocator(paramName), paramDescription, paramValue, minDate, maxDate);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateDateDataFieldWithParamId(bool mandatory, bool isEditable, string paramId, ITextLocator paramName, ITextLocator paramDescription, DateTime? paramValue, DateTime? minDate = null, DateTime? maxDate = null)
        {
            DefaultChecks(paramId, paramName, paramDescription);
            return new DataField
            {
                Mandatory = mandatory,
                IsEditable = isEditable,
                Type = GetSimpleType(typeof(DateTime?)),
                DataFieldType = DataFieldType.DateDataField,
                Id = paramId,
                Name = paramName.TextContent.PlainText,
                Description = paramDescription.TextContent.PlainText,
                Value = paramValue,
                MinDate = minDate,
                MaxDate = maxDate
            };
        }

        [Obsolete("Please the new function 'CreateNumericDataFieldWithParamId'")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateNumericDataField(NumericDataFieldType numericType, bool mandatory, bool isEditable, string paramName, ITextLocator paramDescription, object paramValue, object minValue, object maxValue)
        {
            return CreateNumericDataFieldWithParamId(numericType, mandatory, isEditable, paramName, new TextLocator(paramName), paramDescription, paramValue, minValue, maxValue);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateNumericDataFieldWithParamId(NumericDataFieldType numericType, bool mandatory, bool isEditable, string paramId, ITextLocator paramName, ITextLocator paramDescription, object paramValue, object minValue, object maxValue)
        {
            DefaultChecks(paramId, paramName, paramDescription);
            return new DataField
            {
                Mandatory = mandatory,
                IsEditable = isEditable,
                Type = numericType.ToString().ToLower(),
                DataFieldType = DataFieldType.NumericDataField,
                Id = paramId,
                Name = paramName.TextContent.PlainText,
                Description = paramDescription.TextContent.PlainText,
                Value = paramValue,
                MaxValue = maxValue,
                MinValue = minValue
            };
        }

        [Obsolete("Please use the new functon 'CreatePickListDataFieldWithParamId'")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreatePickListDataField<T>(bool mandatory, bool isEditable, string paramName, ITextLocator paramDescription, List<T> valueList, List<ITextLocator> buttons, int? selectionIndex = null)
        {
            return CreatePickListDataFieldWithParamId(mandatory, isEditable, paramName, new TextLocator(paramName), paramDescription, valueList, buttons, selectionIndex);
        }

        [Obsolete("Please use the new function 'CreatePickListDataFieldWithParamId'")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreatePickListDataField(bool mandatory, bool isEditable, string paramName, ITextLocator paramDescription, object valueListAsObject, List<ITextLocator> buttons, int? selectionIndex = null)
        {
            return CreatePickListDataFieldWithParamId(mandatory, isEditable, paramName, new TextLocator(paramName), paramDescription, valueListAsObject, buttons, selectionIndex);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreatePickListDataFieldWithParamId(bool mandatory, bool isEditable, string paramId, ITextLocator paramName, ITextLocator paramDescription, object valueListAsObject, List<ITextLocator> buttons, int? selectionIndex = null)
        {
            List<object> list = ConvertToObjectList(valueListAsObject);
            DefaultChecks(paramId, paramName, paramDescription);
            buttons = picklistChecks(paramId, list, buttons);
            List<PicklistDataFieldItem> possibleValues = list.Select((object c, int idx) => new PicklistDataFieldItem
            {
                Description = (buttons?[idx]?.TextContent?.PlainText ?? ""),
                IsSelected = (idx == selectionIndex),
                IsEnabled = true,
                IsVisible = true,
                Value = c
            }).ToList();
            return new DataField
            {
                Mandatory = mandatory,
                IsEditable = isEditable,
                Type = GetSimpleType(list.FirstOrDefault().GetType(), isPicklistDataType: true),
                DataFieldType = DataFieldType.PicklistDataField,
                Id = paramId,
                Name = paramName.TextContent.PlainText,
                Description = paramDescription.TextContent.PlainText,
                PossibleValues = possibleValues
            };
        }

        [Obsolete("Please use the new function 'CreateMultiSelectPicklistDataFieldWithParamId'")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateMultiSelectPicklistDataField<T>(bool mandatory, bool isEditable, string paramName, ITextLocator paramDescription, List<T> valueList, List<ITextLocator> buttons, List<int> selectionDefinition)
        {
            return CreateMultiSelectPicklistDataFieldWithParamId(mandatory, isEditable, paramName, new TextLocator(paramName), paramDescription, valueList, buttons, selectionDefinition);
        }

        [Obsolete("Please use the new function 'CreateMultiSelectPicklistDataFieldWithParamId'")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateMultiSelectPicklistDataField(bool mandatory, bool isEditable, string paramName, ITextLocator paramDescription, object valueListAsObject, List<ITextLocator> buttons, List<int> selectionDefinition)
        {
            return CreateMultiSelectPicklistDataFieldWithParamId(mandatory, isEditable, paramName, new TextLocator(paramName), paramDescription, valueListAsObject, buttons, selectionDefinition);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static DataField CreateMultiSelectPicklistDataFieldWithParamId(bool mandatory, bool isEditable, string paramId, ITextLocator paramName, ITextLocator paramDescription, object valueListAsObject, List<ITextLocator> buttons, List<int> selectionDefinition)
        {
            DefaultChecks(paramId, paramName, paramDescription);
            List<object> list = ConvertToObjectList(valueListAsObject);
            buttons = picklistChecks(paramId, list, buttons);
            int num = list?.Count ?? 0;
            if (selectionDefinition != null && selectionDefinition.Count < num)
            {
                throw new ArgumentException(string.Format("The count of '{0}' for the picklist {1} should be the same as values or null!", "selectionDefinition", paramName), "selectionDefinition");
            }
            List<PicklistDataFieldItem> possibleValues = list.Select((object c, int idx) => new PicklistDataFieldItem
            {
                Description = (buttons?[idx]?.TextContent?.PlainText ?? ""),
                IsSelected = (selectionDefinition[idx] == 2 || selectionDefinition[idx] == -2),
                IsEnabled = (selectionDefinition[idx] > 0),
                IsVisible = (selectionDefinition[idx] != 0),
                Value = c
            }).ToList();
            return new DataField
            {
                Mandatory = mandatory,
                IsEditable = isEditable,
                PossibleValues = possibleValues,
                Type = GetSimpleType(list.FirstOrDefault().GetType(), isPicklistDataType: true),
                DataFieldType = DataFieldType.MultiPicklistDataField,
                Id = paramId,
                Name = paramName.TextContent.PlainText,
                Description = paramDescription?.TextContent?.PlainText
            };
        }

        private static void DefaultChecks(string paramId, ITextLocator paramName, ITextLocator paramDescription)
        {
            if (string.IsNullOrEmpty(paramId))
            {
                throw new ArgumentException("'paramId' cannot be null or empty.", "paramId");
            }
            if (paramName == null)
            {
                throw new ArgumentException("'paramName' cannot be null or empty.", "paramName");
            }
            if (paramDescription == null)
            {
                throw new ArgumentNullException("paramDescription");
            }
        }

        private static List<object> ConvertToObjectList(object listAsObject)
        {
            if (listAsObject is IEnumerable enumerable)
            {
                List<object> list = new List<object>();
                {
                    foreach (object item in enumerable)
                    {
                        list.Add(item);
                    }
                    return list;
                }
            }
            throw new ArgumentException("listAsObject must be an enumerable collection.");
        }

        private static List<ITextLocator> picklistChecks(string paramId, List<object> valueList, List<ITextLocator> buttons)
        {
            int num = valueList?.Count ?? 0;
            if (num == 0)
            {
                throw new ArgumentException("valueList must have some entries for the " + paramId + "-picklist!", "valueList");
            }
            if (buttons != null && !buttons.Any())
            {
                buttons = null;
            }
            if (buttons != null && buttons.Count < num)
            {
                throw new ArgumentException("The count of buttons for the picklist " + paramId + " should be the same as values or 0/null!", "buttons");
            }
            return buttons;
        }

        private static string GetSimpleType(Type valueType, bool isPicklistDataType = false)
        {
            if ((object)valueType == null)
            {
                return null;
            }
            if (SimpleTypeMappingDictionary.ContainsKey(valueType.FullName))
            {
                return SimpleTypeMappingDictionary[valueType.FullName];
            }
            if (!isPicklistDataType)
            {
                throw new ArgumentException("The Type '" + valueType.FullName + "' is not allowed for non Multi-/Picklist-DataField. Only the following types are allowed: " + string.Join(", ", SimpleTypeMappingDictionary.Values) + ".");
            }
            return valueType.FullName;
        }
    }
}
