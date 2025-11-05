using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [DesignerCategory("code")]
    [ToolboxItem(true)]
    [XmlSchemaProvider("GetTypedDataSetSchema")]
    [XmlRoot("Localization")]
    [HelpKeyword("vs.data.DataSet")]
    public class Localization : DataSet
    {
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        public delegate void ModuleRowChangeEventHandler(object sender, ModuleRowChangeEvent e);
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        public delegate void LanguageRowChangeEventHandler(object sender, LanguageRowChangeEvent e);
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        public delegate void TextRowChangeEventHandler(object sender, TextRowChangeEvent e);
        [Serializable]
        [XmlSchemaProvider("GetTypedTableSchema")]
        public class ModuleDataTable : TypedTableBase<ModuleRow>
        {
            private DataColumn columnname;
            private DataColumn columnModule_Id;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataColumn nameColumn => columnname;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataColumn Module_IdColumn => columnModule_Id;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            [Browsable(false)]
            public int Count => base.Rows.Count;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public ModuleRow this[int index] => (ModuleRow)base.Rows[index];
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event ModuleRowChangeEventHandler ModuleRowChanging;
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event ModuleRowChangeEventHandler ModuleRowChanged;
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event ModuleRowChangeEventHandler ModuleRowDeleting;
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event ModuleRowChangeEventHandler ModuleRowDeleted;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public ModuleDataTable()
            {
                base.TableName = "Module";
                BeginInit();
                InitClass();
                EndInit();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            internal ModuleDataTable(DataTable table)
            {
                base.TableName = table.TableName;
                if (table.CaseSensitive != table.DataSet.CaseSensitive)
                {
                    base.CaseSensitive = table.CaseSensitive;
                }

                if (table.Locale.ToString() != table.DataSet.Locale.ToString())
                {
                    base.Locale = table.Locale;
                }

                if (table.Namespace != table.DataSet.Namespace)
                {
                    base.Namespace = table.Namespace;
                }

                base.Prefix = table.Prefix;
                base.MinimumCapacity = table.MinimumCapacity;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected ModuleDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
            {
                InitVars();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void AddModuleRow(ModuleRow row)
            {
                base.Rows.Add(row);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public ModuleRow AddModuleRow(string name)
            {
                ModuleRow moduleRow = (ModuleRow)NewRow();
                object[] itemArray = new object[2]
                {
                    name,
                    null
                };
                moduleRow.ItemArray = itemArray;
                base.Rows.Add(moduleRow);
                return moduleRow;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public override DataTable Clone()
            {
                ModuleDataTable obj = (ModuleDataTable)base.Clone();
                obj.InitVars();
                return obj;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override DataTable CreateInstance()
            {
                return new ModuleDataTable();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            internal void InitVars()
            {
                columnname = base.Columns["name"];
                columnModule_Id = base.Columns["Module_Id"];
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            private void InitClass()
            {
                columnname = new DataColumn("name", typeof(string), null, MappingType.Attribute);
                base.Columns.Add(columnname);
                columnModule_Id = new DataColumn("Module_Id", typeof(int), null, MappingType.Hidden);
                base.Columns.Add(columnModule_Id);
                base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[1] { columnModule_Id }, isPrimaryKey: true));
                columnname.Namespace = "";
                columnModule_Id.AutoIncrement = true;
                columnModule_Id.AllowDBNull = false;
                columnModule_Id.Unique = true;
                columnModule_Id.Namespace = "";
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public ModuleRow NewModuleRow()
            {
                return (ModuleRow)NewRow();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                return new ModuleRow(builder);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override Type GetRowType()
            {
                return typeof(ModuleRow);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowChanged(DataRowChangeEventArgs e)
            {
                base.OnRowChanged(e);
                if (this.ModuleRowChanged != null)
                {
                    this.ModuleRowChanged(this, new ModuleRowChangeEvent((ModuleRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowChanging(DataRowChangeEventArgs e)
            {
                base.OnRowChanging(e);
                if (this.ModuleRowChanging != null)
                {
                    this.ModuleRowChanging(this, new ModuleRowChangeEvent((ModuleRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowDeleted(DataRowChangeEventArgs e)
            {
                base.OnRowDeleted(e);
                if (this.ModuleRowDeleted != null)
                {
                    this.ModuleRowDeleted(this, new ModuleRowChangeEvent((ModuleRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowDeleting(DataRowChangeEventArgs e)
            {
                base.OnRowDeleting(e);
                if (this.ModuleRowDeleting != null)
                {
                    this.ModuleRowDeleting(this, new ModuleRowChangeEvent((ModuleRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void RemoveModuleRow(ModuleRow row)
            {
                base.Rows.Remove(row);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
            {
                XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
                XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
                Localization localization = new Localization();
                XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
                xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
                xmlSchemaAny.MinOccurs = 0m;
                xmlSchemaAny.MaxOccurs = decimal.MaxValue;
                xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
                xmlSchemaSequence.Items.Add(xmlSchemaAny);
                XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
                xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
                xmlSchemaAny2.MinOccurs = 1m;
                xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
                xmlSchemaSequence.Items.Add(xmlSchemaAny2);
                XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
                xmlSchemaAttribute.Name = "namespace";
                xmlSchemaAttribute.FixedValue = localization.Namespace;
                xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
                XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
                xmlSchemaAttribute2.Name = "tableTypeName";
                xmlSchemaAttribute2.FixedValue = "ModuleDataTable";
                xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
                xmlSchemaComplexType.Particle = xmlSchemaSequence;
                XmlSchema schemaSerializable = localization.GetSchemaSerializable();
                if (xs.Contains(schemaSerializable.TargetNamespace))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    MemoryStream memoryStream2 = new MemoryStream();
                    try
                    {
                        schemaSerializable.Write(memoryStream);
                        IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            XmlSchema obj = (XmlSchema)enumerator.Current;
                            memoryStream2.SetLength(0L);
                            obj.Write(memoryStream2);
                            if (memoryStream.Length == memoryStream2.Length)
                            {
                                memoryStream.Position = 0L;
                                memoryStream2.Position = 0L;
                                while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
                                {
                                }

                                if (memoryStream.Position == memoryStream.Length)
                                {
                                    return xmlSchemaComplexType;
                                }
                            }
                        }
                    }
                    finally
                    {
                        memoryStream?.Close();
                        memoryStream2?.Close();
                    }
                }

                xs.Add(schemaSerializable);
                return xmlSchemaComplexType;
            }
        }

        [Serializable]
        [XmlSchemaProvider("GetTypedTableSchema")]
        public class LanguageDataTable : TypedTableBase<LanguageRow>
        {
            private DataColumn columnculture;
            private DataColumn columndefCulture;
            private DataColumn columnLanguage_Id;
            private DataColumn columnModule_Id;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataColumn cultureColumn => columnculture;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataColumn defCultureColumn => columndefCulture;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataColumn Language_IdColumn => columnLanguage_Id;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataColumn Module_IdColumn => columnModule_Id;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            [Browsable(false)]
            public int Count => base.Rows.Count;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public LanguageRow this[int index] => (LanguageRow)base.Rows[index];
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event LanguageRowChangeEventHandler LanguageRowChanging;
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event LanguageRowChangeEventHandler LanguageRowChanged;
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event LanguageRowChangeEventHandler LanguageRowDeleting;
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event LanguageRowChangeEventHandler LanguageRowDeleted;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public LanguageDataTable()
            {
                base.TableName = "Language";
                BeginInit();
                InitClass();
                EndInit();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            internal LanguageDataTable(DataTable table)
            {
                base.TableName = table.TableName;
                if (table.CaseSensitive != table.DataSet.CaseSensitive)
                {
                    base.CaseSensitive = table.CaseSensitive;
                }

                if (table.Locale.ToString() != table.DataSet.Locale.ToString())
                {
                    base.Locale = table.Locale;
                }

                if (table.Namespace != table.DataSet.Namespace)
                {
                    base.Namespace = table.Namespace;
                }

                base.Prefix = table.Prefix;
                base.MinimumCapacity = table.MinimumCapacity;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected LanguageDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
            {
                InitVars();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void AddLanguageRow(LanguageRow row)
            {
                base.Rows.Add(row);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public LanguageRow AddLanguageRow(string culture, string defCulture, ModuleRow parentModuleRowByModule_Language)
            {
                LanguageRow languageRow = (LanguageRow)NewRow();
                object[] array = new object[4]
                {
                    culture,
                    defCulture,
                    null,
                    null
                };
                if (parentModuleRowByModule_Language != null)
                {
                    array[3] = parentModuleRowByModule_Language[1];
                }

                languageRow.ItemArray = array;
                base.Rows.Add(languageRow);
                return languageRow;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public override DataTable Clone()
            {
                LanguageDataTable obj = (LanguageDataTable)base.Clone();
                obj.InitVars();
                return obj;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override DataTable CreateInstance()
            {
                return new LanguageDataTable();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            internal void InitVars()
            {
                columnculture = base.Columns["culture"];
                columndefCulture = base.Columns["defCulture"];
                columnLanguage_Id = base.Columns["Language_Id"];
                columnModule_Id = base.Columns["Module_Id"];
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            private void InitClass()
            {
                columnculture = new DataColumn("culture", typeof(string), null, MappingType.Attribute);
                base.Columns.Add(columnculture);
                columndefCulture = new DataColumn("defCulture", typeof(string), null, MappingType.Attribute);
                base.Columns.Add(columndefCulture);
                columnLanguage_Id = new DataColumn("Language_Id", typeof(int), null, MappingType.Hidden);
                base.Columns.Add(columnLanguage_Id);
                columnModule_Id = new DataColumn("Module_Id", typeof(int), null, MappingType.Hidden);
                base.Columns.Add(columnModule_Id);
                base.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[1] { columnLanguage_Id }, isPrimaryKey: true));
                columnculture.Namespace = "";
                columndefCulture.Namespace = "";
                columnLanguage_Id.AutoIncrement = true;
                columnLanguage_Id.AllowDBNull = false;
                columnLanguage_Id.Unique = true;
                columnLanguage_Id.Namespace = "";
                columnModule_Id.Namespace = "";
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public LanguageRow NewLanguageRow()
            {
                return (LanguageRow)NewRow();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                return new LanguageRow(builder);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override Type GetRowType()
            {
                return typeof(LanguageRow);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowChanged(DataRowChangeEventArgs e)
            {
                base.OnRowChanged(e);
                if (this.LanguageRowChanged != null)
                {
                    this.LanguageRowChanged(this, new LanguageRowChangeEvent((LanguageRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowChanging(DataRowChangeEventArgs e)
            {
                base.OnRowChanging(e);
                if (this.LanguageRowChanging != null)
                {
                    this.LanguageRowChanging(this, new LanguageRowChangeEvent((LanguageRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowDeleted(DataRowChangeEventArgs e)
            {
                base.OnRowDeleted(e);
                if (this.LanguageRowDeleted != null)
                {
                    this.LanguageRowDeleted(this, new LanguageRowChangeEvent((LanguageRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowDeleting(DataRowChangeEventArgs e)
            {
                base.OnRowDeleting(e);
                if (this.LanguageRowDeleting != null)
                {
                    this.LanguageRowDeleting(this, new LanguageRowChangeEvent((LanguageRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void RemoveLanguageRow(LanguageRow row)
            {
                base.Rows.Remove(row);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
            {
                XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
                XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
                Localization localization = new Localization();
                XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
                xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
                xmlSchemaAny.MinOccurs = 0m;
                xmlSchemaAny.MaxOccurs = decimal.MaxValue;
                xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
                xmlSchemaSequence.Items.Add(xmlSchemaAny);
                XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
                xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
                xmlSchemaAny2.MinOccurs = 1m;
                xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
                xmlSchemaSequence.Items.Add(xmlSchemaAny2);
                XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
                xmlSchemaAttribute.Name = "namespace";
                xmlSchemaAttribute.FixedValue = localization.Namespace;
                xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
                XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
                xmlSchemaAttribute2.Name = "tableTypeName";
                xmlSchemaAttribute2.FixedValue = "LanguageDataTable";
                xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
                xmlSchemaComplexType.Particle = xmlSchemaSequence;
                XmlSchema schemaSerializable = localization.GetSchemaSerializable();
                if (xs.Contains(schemaSerializable.TargetNamespace))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    MemoryStream memoryStream2 = new MemoryStream();
                    try
                    {
                        schemaSerializable.Write(memoryStream);
                        IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            XmlSchema obj = (XmlSchema)enumerator.Current;
                            memoryStream2.SetLength(0L);
                            obj.Write(memoryStream2);
                            if (memoryStream.Length == memoryStream2.Length)
                            {
                                memoryStream.Position = 0L;
                                memoryStream2.Position = 0L;
                                while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
                                {
                                }

                                if (memoryStream.Position == memoryStream.Length)
                                {
                                    return xmlSchemaComplexType;
                                }
                            }
                        }
                    }
                    finally
                    {
                        memoryStream?.Close();
                        memoryStream2?.Close();
                    }
                }

                xs.Add(schemaSerializable);
                return xmlSchemaComplexType;
            }
        }

        [Serializable]
        [XmlSchemaProvider("GetTypedTableSchema")]
        public class TextDataTable : TypedTableBase<TextRow>
        {
            private DataColumn columnid;
            private DataColumn columnname;
            private DataColumn columnLanguage_Id;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataColumn idColumn => columnid;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataColumn nameColumn => columnname;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataColumn Language_IdColumn => columnLanguage_Id;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            [Browsable(false)]
            public int Count => base.Rows.Count;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public TextRow this[int index] => (TextRow)base.Rows[index];
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event TextRowChangeEventHandler TextRowChanging;
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event TextRowChangeEventHandler TextRowChanged;
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event TextRowChangeEventHandler TextRowDeleting;
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public event TextRowChangeEventHandler TextRowDeleted;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public TextDataTable()
            {
                base.TableName = "Text";
                BeginInit();
                InitClass();
                EndInit();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            internal TextDataTable(DataTable table)
            {
                base.TableName = table.TableName;
                if (table.CaseSensitive != table.DataSet.CaseSensitive)
                {
                    base.CaseSensitive = table.CaseSensitive;
                }

                if (table.Locale.ToString() != table.DataSet.Locale.ToString())
                {
                    base.Locale = table.Locale;
                }

                if (table.Namespace != table.DataSet.Namespace)
                {
                    base.Namespace = table.Namespace;
                }

                base.Prefix = table.Prefix;
                base.MinimumCapacity = table.MinimumCapacity;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected TextDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
            {
                InitVars();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void AddTextRow(TextRow row)
            {
                base.Rows.Add(row);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public TextRow AddTextRow(string id, string name, LanguageRow parentLanguageRowByLanguage_Text)
            {
                TextRow textRow = (TextRow)NewRow();
                object[] array = new object[3]
                {
                    id,
                    name,
                    null
                };
                if (parentLanguageRowByLanguage_Text != null)
                {
                    array[2] = parentLanguageRowByLanguage_Text[2];
                }

                textRow.ItemArray = array;
                base.Rows.Add(textRow);
                return textRow;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public override DataTable Clone()
            {
                TextDataTable obj = (TextDataTable)base.Clone();
                obj.InitVars();
                return obj;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override DataTable CreateInstance()
            {
                return new TextDataTable();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            internal void InitVars()
            {
                columnid = base.Columns["id"];
                columnname = base.Columns["name"];
                columnLanguage_Id = base.Columns["Language_Id"];
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            private void InitClass()
            {
                columnid = new DataColumn("id", typeof(string), null, MappingType.Attribute);
                base.Columns.Add(columnid);
                columnname = new DataColumn("name", typeof(string), null, MappingType.Attribute);
                base.Columns.Add(columnname);
                columnLanguage_Id = new DataColumn("Language_Id", typeof(int), null, MappingType.Hidden);
                base.Columns.Add(columnLanguage_Id);
                columnid.Namespace = "";
                columnname.Namespace = "";
                columnLanguage_Id.Namespace = "";
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public TextRow NewTextRow()
            {
                return (TextRow)NewRow();
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
            {
                return new TextRow(builder);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override Type GetRowType()
            {
                return typeof(TextRow);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowChanged(DataRowChangeEventArgs e)
            {
                base.OnRowChanged(e);
                if (this.TextRowChanged != null)
                {
                    this.TextRowChanged(this, new TextRowChangeEvent((TextRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowChanging(DataRowChangeEventArgs e)
            {
                base.OnRowChanging(e);
                if (this.TextRowChanging != null)
                {
                    this.TextRowChanging(this, new TextRowChangeEvent((TextRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowDeleted(DataRowChangeEventArgs e)
            {
                base.OnRowDeleted(e);
                if (this.TextRowDeleted != null)
                {
                    this.TextRowDeleted(this, new TextRowChangeEvent((TextRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            protected override void OnRowDeleting(DataRowChangeEventArgs e)
            {
                base.OnRowDeleting(e);
                if (this.TextRowDeleting != null)
                {
                    this.TextRowDeleting(this, new TextRowChangeEvent((TextRow)e.Row, e.Action));
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void RemoveTextRow(TextRow row)
            {
                base.Rows.Remove(row);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
            {
                XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
                XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
                Localization localization = new Localization();
                XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
                xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
                xmlSchemaAny.MinOccurs = 0m;
                xmlSchemaAny.MaxOccurs = decimal.MaxValue;
                xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
                xmlSchemaSequence.Items.Add(xmlSchemaAny);
                XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
                xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
                xmlSchemaAny2.MinOccurs = 1m;
                xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
                xmlSchemaSequence.Items.Add(xmlSchemaAny2);
                XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
                xmlSchemaAttribute.Name = "namespace";
                xmlSchemaAttribute.FixedValue = localization.Namespace;
                xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute);
                XmlSchemaAttribute xmlSchemaAttribute2 = new XmlSchemaAttribute();
                xmlSchemaAttribute2.Name = "tableTypeName";
                xmlSchemaAttribute2.FixedValue = "TextDataTable";
                xmlSchemaComplexType.Attributes.Add(xmlSchemaAttribute2);
                xmlSchemaComplexType.Particle = xmlSchemaSequence;
                XmlSchema schemaSerializable = localization.GetSchemaSerializable();
                if (xs.Contains(schemaSerializable.TargetNamespace))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    MemoryStream memoryStream2 = new MemoryStream();
                    try
                    {
                        schemaSerializable.Write(memoryStream);
                        IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            XmlSchema obj = (XmlSchema)enumerator.Current;
                            memoryStream2.SetLength(0L);
                            obj.Write(memoryStream2);
                            if (memoryStream.Length == memoryStream2.Length)
                            {
                                memoryStream.Position = 0L;
                                memoryStream2.Position = 0L;
                                while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
                                {
                                }

                                if (memoryStream.Position == memoryStream.Length)
                                {
                                    return xmlSchemaComplexType;
                                }
                            }
                        }
                    }
                    finally
                    {
                        memoryStream?.Close();
                        memoryStream2?.Close();
                    }
                }

                xs.Add(schemaSerializable);
                return xmlSchemaComplexType;
            }
        }

        public class ModuleRow : DataRow
        {
            private ModuleDataTable tableModule;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public string name
            {
                get
                {
                    try
                    {
                        return (string)base[tableModule.nameColumn];
                    }
                    catch (InvalidCastException innerException)
                    {
                        throw new StrongTypingException("The value for column 'name' in table 'Module' is DBNull.", innerException);
                    }
                }

                set
                {
                    base[tableModule.nameColumn] = value;
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public int Module_Id
            {
                get
                {
                    return (int)base[tableModule.Module_IdColumn];
                }

                set
                {
                    base[tableModule.Module_IdColumn] = value;
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            internal ModuleRow(DataRowBuilder rb) : base(rb)
            {
                tableModule = (ModuleDataTable)base.Table;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public bool IsnameNull()
            {
                return IsNull(tableModule.nameColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void SetnameNull()
            {
                base[tableModule.nameColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public LanguageRow[] GetLanguageRows()
            {
                if (base.Table.ChildRelations["Module_Language"] == null)
                {
                    return new LanguageRow[0];
                }

                return (LanguageRow[])GetChildRows(base.Table.ChildRelations["Module_Language"]);
            }
        }

        public class LanguageRow : DataRow
        {
            private LanguageDataTable tableLanguage;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public string culture
            {
                get
                {
                    try
                    {
                        return (string)base[tableLanguage.cultureColumn];
                    }
                    catch (InvalidCastException innerException)
                    {
                        throw new StrongTypingException("The value for column 'culture' in table 'Language' is DBNull.", innerException);
                    }
                }

                set
                {
                    base[tableLanguage.cultureColumn] = value;
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public string defCulture
            {
                get
                {
                    try
                    {
                        return (string)base[tableLanguage.defCultureColumn];
                    }
                    catch (InvalidCastException innerException)
                    {
                        throw new StrongTypingException("The value for column 'defCulture' in table 'Language' is DBNull.", innerException);
                    }
                }

                set
                {
                    base[tableLanguage.defCultureColumn] = value;
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public int Language_Id
            {
                get
                {
                    return (int)base[tableLanguage.Language_IdColumn];
                }

                set
                {
                    base[tableLanguage.Language_IdColumn] = value;
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public int Module_Id
            {
                get
                {
                    try
                    {
                        return (int)base[tableLanguage.Module_IdColumn];
                    }
                    catch (InvalidCastException innerException)
                    {
                        throw new StrongTypingException("The value for column 'Module_Id' in table 'Language' is DBNull.", innerException);
                    }
                }

                set
                {
                    base[tableLanguage.Module_IdColumn] = value;
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public ModuleRow ModuleRow
            {
                get
                {
                    return (ModuleRow)GetParentRow(base.Table.ParentRelations["Module_Language"]);
                }

                set
                {
                    SetParentRow(value, base.Table.ParentRelations["Module_Language"]);
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            internal LanguageRow(DataRowBuilder rb) : base(rb)
            {
                tableLanguage = (LanguageDataTable)base.Table;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public bool IscultureNull()
            {
                return IsNull(tableLanguage.cultureColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void SetcultureNull()
            {
                base[tableLanguage.cultureColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public bool IsdefCultureNull()
            {
                return IsNull(tableLanguage.defCultureColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void SetdefCultureNull()
            {
                base[tableLanguage.defCultureColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public bool IsModule_IdNull()
            {
                return IsNull(tableLanguage.Module_IdColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void SetModule_IdNull()
            {
                base[tableLanguage.Module_IdColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public TextRow[] GetTextRows()
            {
                if (base.Table.ChildRelations["Language_Text"] == null)
                {
                    return new TextRow[0];
                }

                return (TextRow[])GetChildRows(base.Table.ChildRelations["Language_Text"]);
            }
        }

        public class TextRow : DataRow
        {
            private TextDataTable tableText;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public string id
            {
                get
                {
                    try
                    {
                        return (string)base[tableText.idColumn];
                    }
                    catch (InvalidCastException innerException)
                    {
                        throw new StrongTypingException("The value for column 'id' in table 'Text' is DBNull.", innerException);
                    }
                }

                set
                {
                    base[tableText.idColumn] = value;
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public string name
            {
                get
                {
                    try
                    {
                        return (string)base[tableText.nameColumn];
                    }
                    catch (InvalidCastException innerException)
                    {
                        throw new StrongTypingException("The value for column 'name' in table 'Text' is DBNull.", innerException);
                    }
                }

                set
                {
                    base[tableText.nameColumn] = value;
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public int Language_Id
            {
                get
                {
                    try
                    {
                        return (int)base[tableText.Language_IdColumn];
                    }
                    catch (InvalidCastException innerException)
                    {
                        throw new StrongTypingException("The value for column 'Language_Id' in table 'Text' is DBNull.", innerException);
                    }
                }

                set
                {
                    base[tableText.Language_IdColumn] = value;
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public LanguageRow LanguageRow
            {
                get
                {
                    return (LanguageRow)GetParentRow(base.Table.ParentRelations["Language_Text"]);
                }

                set
                {
                    SetParentRow(value, base.Table.ParentRelations["Language_Text"]);
                }
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            internal TextRow(DataRowBuilder rb) : base(rb)
            {
                tableText = (TextDataTable)base.Table;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public bool IsidNull()
            {
                return IsNull(tableText.idColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void SetidNull()
            {
                base[tableText.idColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public bool IsnameNull()
            {
                return IsNull(tableText.nameColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void SetnameNull()
            {
                base[tableText.nameColumn] = Convert.DBNull;
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public bool IsLanguage_IdNull()
            {
                return IsNull(tableText.Language_IdColumn);
            }

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public void SetLanguage_IdNull()
            {
                base[tableText.Language_IdColumn] = Convert.DBNull;
            }
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        public class ModuleRowChangeEvent : EventArgs
        {
            private ModuleRow eventRow;
            private DataRowAction eventAction;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public ModuleRow Row => eventRow;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataRowAction Action => eventAction;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public ModuleRowChangeEvent(ModuleRow row, DataRowAction action)
            {
                eventRow = row;
                eventAction = action;
            }
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        public class LanguageRowChangeEvent : EventArgs
        {
            private LanguageRow eventRow;
            private DataRowAction eventAction;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public LanguageRow Row => eventRow;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataRowAction Action => eventAction;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public LanguageRowChangeEvent(LanguageRow row, DataRowAction action)
            {
                eventRow = row;
                eventAction = action;
            }
        }

        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        public class TextRowChangeEvent : EventArgs
        {
            private TextRow eventRow;
            private DataRowAction eventAction;
            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public TextRow Row => eventRow;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public DataRowAction Action => eventAction;

            [DebuggerNonUserCode]
            [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
            public TextRowChangeEvent(TextRow row, DataRowAction action)
            {
                eventRow = row;
                eventAction = action;
            }
        }

        private ModuleDataTable tableModule;
        private LanguageDataTable tableLanguage;
        private TextDataTable tableText;
        private DataRelation relationModule_Language;
        private DataRelation relationLanguage_Text;
        private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;
        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ModuleDataTable Module => tableModule;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LanguageDataTable Language => tableLanguage;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TextDataTable Text => tableText;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override SchemaSerializationMode SchemaSerializationMode
        {
            get
            {
                return _schemaSerializationMode;
            }

            set
            {
                _schemaSerializationMode = value;
            }
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DataTableCollection Tables => base.Tables;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DataRelationCollection Relations => base.Relations;

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        public Localization()
        {
            BeginInit();
            InitClass();
            CollectionChangeEventHandler value = SchemaChanged;
            base.Tables.CollectionChanged += value;
            base.Relations.CollectionChanged += value;
            EndInit();
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        protected Localization(SerializationInfo info, StreamingContext context) : base(info, context, ConstructSchema: false)
        {
            if (IsBinarySerialized(info, context))
            {
                InitVars(initTable: false);
                CollectionChangeEventHandler value = SchemaChanged;
                Tables.CollectionChanged += value;
                Relations.CollectionChanged += value;
                return;
            }

            string s = (string)info.GetValue("XmlSchema", typeof(string));
            if (DetermineSchemaSerializationMode(info, context) == SchemaSerializationMode.IncludeSchema)
            {
                DataSet dataSet = new DataSet();
                dataSet.ReadXmlSchema(new XmlTextReader(new StringReader(s)));
                if (dataSet.Tables["Module"] != null)
                {
                    base.Tables.Add(new ModuleDataTable(dataSet.Tables["Module"]));
                }

                if (dataSet.Tables["Language"] != null)
                {
                    base.Tables.Add(new LanguageDataTable(dataSet.Tables["Language"]));
                }

                if (dataSet.Tables["Text"] != null)
                {
                    base.Tables.Add(new TextDataTable(dataSet.Tables["Text"]));
                }

                base.DataSetName = dataSet.DataSetName;
                base.Prefix = dataSet.Prefix;
                base.Namespace = dataSet.Namespace;
                base.Locale = dataSet.Locale;
                base.CaseSensitive = dataSet.CaseSensitive;
                base.EnforceConstraints = dataSet.EnforceConstraints;
                Merge(dataSet, preserveChanges: false, MissingSchemaAction.Add);
                InitVars();
            }
            else
            {
                ReadXmlSchema(new XmlTextReader(new StringReader(s)));
            }

            GetSerializationData(info, context);
            CollectionChangeEventHandler value2 = SchemaChanged;
            base.Tables.CollectionChanged += value2;
            Relations.CollectionChanged += value2;
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        protected override void InitializeDerivedDataSet()
        {
            BeginInit();
            InitClass();
            EndInit();
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        public override DataSet Clone()
        {
            Localization obj = (Localization)base.Clone();
            obj.InitVars();
            obj.SchemaSerializationMode = SchemaSerializationMode;
            return obj;
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        protected override bool ShouldSerializeTables()
        {
            return false;
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        protected override bool ShouldSerializeRelations()
        {
            return false;
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        protected override void ReadXmlSerializable(XmlReader reader)
        {
            if (DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
            {
                Reset();
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(reader);
                if (dataSet.Tables["Module"] != null)
                {
                    base.Tables.Add(new ModuleDataTable(dataSet.Tables["Module"]));
                }

                if (dataSet.Tables["Language"] != null)
                {
                    base.Tables.Add(new LanguageDataTable(dataSet.Tables["Language"]));
                }

                if (dataSet.Tables["Text"] != null)
                {
                    base.Tables.Add(new TextDataTable(dataSet.Tables["Text"]));
                }

                base.DataSetName = dataSet.DataSetName;
                base.Prefix = dataSet.Prefix;
                base.Namespace = dataSet.Namespace;
                base.Locale = dataSet.Locale;
                base.CaseSensitive = dataSet.CaseSensitive;
                base.EnforceConstraints = dataSet.EnforceConstraints;
                Merge(dataSet, preserveChanges: false, MissingSchemaAction.Add);
                InitVars();
            }
            else
            {
                ReadXml(reader);
                InitVars();
            }
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        protected override XmlSchema GetSchemaSerializable()
        {
            MemoryStream memoryStream = new MemoryStream();
            WriteXmlSchema(new XmlTextWriter(memoryStream, null));
            memoryStream.Position = 0L;
            return XmlSchema.Read(new XmlTextReader(memoryStream), null);
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        internal void InitVars()
        {
            InitVars(initTable: true);
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        internal void InitVars(bool initTable)
        {
            tableModule = (ModuleDataTable)base.Tables["Module"];
            if (initTable && tableModule != null)
            {
                tableModule.InitVars();
            }

            tableLanguage = (LanguageDataTable)base.Tables["Language"];
            if (initTable && tableLanguage != null)
            {
                tableLanguage.InitVars();
            }

            tableText = (TextDataTable)base.Tables["Text"];
            if (initTable && tableText != null)
            {
                tableText.InitVars();
            }

            relationModule_Language = Relations["Module_Language"];
            relationLanguage_Text = Relations["Language_Text"];
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        private void InitClass()
        {
            base.DataSetName = "Localization";
            base.Prefix = "";
            base.Locale = new CultureInfo("en-US");
            base.EnforceConstraints = true;
            SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            tableModule = new ModuleDataTable();
            base.Tables.Add(tableModule);
            tableLanguage = new LanguageDataTable();
            base.Tables.Add(tableLanguage);
            tableText = new TextDataTable();
            base.Tables.Add(tableText);
            ForeignKeyConstraint foreignKeyConstraint = new ForeignKeyConstraint("Module_Language", new DataColumn[1] { tableModule.Module_IdColumn }, new DataColumn[1] { tableLanguage.Module_IdColumn });
            tableLanguage.Constraints.Add(foreignKeyConstraint);
            foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
            foreignKeyConstraint.DeleteRule = Rule.Cascade;
            foreignKeyConstraint.UpdateRule = Rule.Cascade;
            foreignKeyConstraint = new ForeignKeyConstraint("Language_Text", new DataColumn[1] { tableLanguage.Language_IdColumn }, new DataColumn[1] { tableText.Language_IdColumn });
            tableText.Constraints.Add(foreignKeyConstraint);
            foreignKeyConstraint.AcceptRejectRule = AcceptRejectRule.None;
            foreignKeyConstraint.DeleteRule = Rule.Cascade;
            foreignKeyConstraint.UpdateRule = Rule.Cascade;
            relationModule_Language = new DataRelation("Module_Language", new DataColumn[1] { tableModule.Module_IdColumn }, new DataColumn[1] { tableLanguage.Module_IdColumn }, createConstraints: false);
            relationModule_Language.Nested = true;
            Relations.Add(relationModule_Language);
            relationLanguage_Text = new DataRelation("Language_Text", new DataColumn[1] { tableLanguage.Language_IdColumn }, new DataColumn[1] { tableText.Language_IdColumn }, createConstraints: false);
            relationLanguage_Text.Nested = true;
            Relations.Add(relationLanguage_Text);
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        private bool ShouldSerializeModule()
        {
            return false;
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        private bool ShouldSerializeLanguage()
        {
            return false;
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        private bool ShouldSerializeText()
        {
            return false;
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        private void SchemaChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Remove)
            {
                InitVars();
            }
        }

        [DebuggerNonUserCode]
        [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "16.0.0.0")]
        public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
        {
            Localization localization = new Localization();
            XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
            XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
            XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
            xmlSchemaAny.Namespace = localization.Namespace;
            xmlSchemaSequence.Items.Add(xmlSchemaAny);
            xmlSchemaComplexType.Particle = xmlSchemaSequence;
            XmlSchema schemaSerializable = localization.GetSchemaSerializable();
            if (xs.Contains(schemaSerializable.TargetNamespace))
            {
                MemoryStream memoryStream = new MemoryStream();
                MemoryStream memoryStream2 = new MemoryStream();
                try
                {
                    schemaSerializable.Write(memoryStream);
                    IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        XmlSchema obj = (XmlSchema)enumerator.Current;
                        memoryStream2.SetLength(0L);
                        obj.Write(memoryStream2);
                        if (memoryStream.Length == memoryStream2.Length)
                        {
                            memoryStream.Position = 0L;
                            memoryStream2.Position = 0L;
                            while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
                            {
                            }

                            if (memoryStream.Position == memoryStream.Length)
                            {
                                return xmlSchemaComplexType;
                            }
                        }
                    }
                }
                finally
                {
                    memoryStream?.Close();
                    memoryStream2?.Close();
                }
            }

            xs.Add(schemaSerializable);
            return xmlSchemaComplexType;
        }
    }
}