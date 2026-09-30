using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.Measurement.Common;
using BMW.Rheingold.Measurement.Common.Data;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    internal class MeasuringConfigurationAdapter : BaseAdapter
    {
        private ConfigurationContainer config;

        private List<string> nodeName = new List<string>();

        private MeasuringConfigurationType result;

        public MeasuringConfigurationAdapter(ConfigurationContainer config)
            : base(StandardErrorHandling: false, config)
        {
            this.config = config;
            result = new MeasuringConfigurationType();
        }

        public IDiagnosticDeviceResult Execute()
        {
            Log.Warning("MeasuringConfigurationAdapter.Execute()", "Not implemented yet !!!!");
            return null;
        }

        public MeasuringConfigurationType ParseParametrization()
        {
            if (config == null || config.Body == null || config.Body.Configuration == null || config.Body.Configuration.Parametrization == null || config.Body.Configuration.Parametrization.Children == null)
            {
                throw new ArgumentNullException("No usefull configuration.");
            }
            if (!"BMW IMIB".Equals(config.Body.Configuration.Name) && !"BMW IMIB_SpExtract".Equals(config.Body.Configuration.Name))
            {
                throw new ArgumentNullException("No configuration for BMW IMIB, but \"" + config.Body.Configuration.Name + "\".");
            }
            if (!"Device".Equals(config.Body.Configuration.Parametrization.Name))
            {
                throw new ArgumentNullException("No parametrization for BMW IMIB Device, but for \"" + config.Body.Configuration.Parametrization.Name + "\".");
            }
            foreach (ANode child in config.Body.Configuration.Parametrization.Children)
            {
                result.Dmm = new DmmType();
                DmmChannelType dmmChannelType = new DmmChannelType();
                dmmChannelType.SourceName = "None";
                result.Dmm.Channel.Add(dmmChannelType);
                dmmChannelType = new DmmChannelType();
                dmmChannelType.SourceName = "None";
                result.Dmm.Channel.Add(dmmChannelType);
                if ("DMM1".Equals(child.Name))
                {
                    KonfigureDmm(child, result.Dmm.Channel[0]);
                    continue;
                }
                if ("DMM2".Equals(child.Name))
                {
                    KonfigureDmm(child, result.Dmm.Channel[1]);
                    continue;
                }
                if ("CNT".Equals(child.Name))
                {
                    result.CntField = ConfigureCnt(child);
                    continue;
                }
                Log.Error("MeasuringConfigurationAdapter.ParseParametrization()", "Unsupported device {0} will be skipped", child.Name);
            }
            return result;
        }

        protected virtual void Parse(ANode node)
        {
            if (node is SingleChoice)
            {
                ParseSingleChoice(node as SingleChoice);
                return;
            }
            if (node is MultipleChoice)
            {
                ParseMultipleChoice(node as MultipleChoice);
                return;
            }
            if (node is QuantityChoice)
            {
                ParseQuantityChoice(node as QuantityChoice);
                return;
            }
            if (node is AChoice)
            {
                ParseChoice(node as AChoice);
                return;
            }
            if (node is Sequence)
            {
                ParseSequence(node as Sequence);
                return;
            }
            if (node is Executable)
            {
                ParseExecutable(node as Executable);
                return;
            }
            if (node is All)
            {
                ParseAll(node as All);
                return;
            }
            if (node is ABranch)
            {
                ParseBranch(node as ABranch);
                return;
            }
            if (node is Value)
            {
                ParseValue(node as Value);
                return;
            }
            Log.Error("MeasuringConfigurationAdapter.Parse()", "Unknown node type {0}.", node.GetType());
        }

        protected virtual void ParseAll(All node)
        {
        }

        protected virtual void ParseBranch(ABranch node)
        {
            nodeName.Add(node.Name);
            if (node.Children == null)
            {
                return;
            }
            foreach (ANode child in node.Children)
            {
                Parse(child);
            }
        }

        protected virtual void ParseChoice(AChoice node)
        {
        }

        protected virtual void ParseExecutable(Executable node)
        {
        }

        protected virtual void ParseMultipleChoice(MultipleChoice node)
        {
        }

        protected virtual void ParseQuantityChoice(QuantityChoice node)
        {
        }

        protected virtual void ParseSequence(Sequence node)
        {
        }

        protected virtual void ParseSingleChoice(SingleChoice node)
        {
            if (node.Children == null)
            {
                return;
            }
            foreach (ANode child in node.Children)
            {
                Parse(child);
            }
        }

        protected virtual void ParseValue(Value node)
        {
        }

        private ImibCounterConfigData ConfigureCnt(ANode root)
        {
            bool frameEnabled = FindKonfiguredValue(root, "FrameEnabled", defaultValue: true);
            bool averageEnabled = FindKonfiguredValue(root, "AverageEnabled", defaultValue: true);
            int frameLength = FindKonfiguredValue(root, "FrameLength", (ushort)0);
            float timeOut = FindKonfiguredValue(root, "TimeOut", 0f);
            string filter = FindKonfiguredValue(root, "Filter", "Off");
            string source = FindKonfiguredValue(root, "Source", "Probes1");
            string function = FindKonfiguredValue(root, "Function", "Voltage");
            string coupling = FindKonfiguredValue(root, "Coupling", "DC");
            string range = FindKonfiguredValue(root, "Range", "Auto");
            float level = FindKonfiguredValue(root, "Level", 0f);
            string edge = FindKonfiguredValue(root, "Edge", "Pos");
            return new ImibCounterConfigData(frameEnabled, averageEnabled, frameLength, timeOut, filter, source, function, coupling, range, level, edge);
        }

        private T FindKonfiguredValue<T>(ANode root, string property, T defaultValue)
        {
            ANode aNode = FindNodeWithName(property, root);
            if (aNode == null)
            {
                Log.Warning("MeasuringConfigurationAdapter.GetFirstChildName()", "No node found with name {0} found, use given default value {1}.", property, defaultValue);
                return defaultValue;
            }
            if (aNode is ABranch)
            {
                return (T)GetFirstChildName((ABranch)aNode, defaultValue as string);
            }
            return GetContent(aNode, property, defaultValue);
        }

        private ANode FindNodeWithName(string name, ANode root)
        {
            if (name.Equals(root.Name))
            {
                return root;
            }
            if (!(root is ABranch aBranch) || aBranch.Children == null)
            {
                return null;
            }
            foreach (ANode child in aBranch.Children)
            {
                ANode aNode = FindNodeWithName(name, child);
                if (aNode != null)
                {
                    return aNode;
                }
            }
            return null;
        }

        private T GetContent<T>(ANode parent, string parentName, T defaultValue)
        {
            if (!(parent is Value value))
            {
                Log.Warning("MeasuringConfigurationAdapter.GetLiteralText()", "Child of node with name {0} is not of type Value. Use given default value {1}.", parentName, defaultValue);
                return defaultValue;
            }
            ValueLiteral literal = value.Literal;
            if (literal == null)
            {
                Log.Warning("MeasuringConfigurationAdapter.GetLiteralText()", "Child of node with name {0} is of type Value, but has no ValueLiteral. Use given default value {1}.", parentName, defaultValue);
                return defaultValue;
            }
            if (literal.ItemType == "Float" || literal.ItemType == "Double" || literal.ItemType == "UShort")
            {
                return (T)literal.Item;
            }
            if (literal.ItemType == "Text")
            {
                return (T)GetLiteralText(literal, parentName, defaultValue as string);
            }
            return default(T);
        }

        private object GetFirstChildName(ABranch parent, string defaultValue)
        {
            if (parent != null && parent.Children != null && parent.Children.Count > 0)
            {
                return parent.Children[0].Name;
            }
            Log.Warning("MeasuringConfigurationAdapter.GetFirstChildName()", "No child of node with name {0} found, use given default value {1}.", parent.Name, defaultValue);
            return defaultValue;
        }

        private object GetLiteralText(ValueLiteral parent, string parentName, string defaultValue)
        {
            if (!(parent.Item is Text text))
            {
                Log.Warning("MeasuringConfigurationAdapter.GetLiteralText()", "Child of node with name {0} is of type Value, and has a ValueLiteral, but this has no Item of type Text. Use given default value {1}.", parentName, defaultValue);
                return defaultValue;
            }
            if (text.Value == null)
            {
                Log.Warning("MeasuringConfigurationAdapter.GetLiteralText()", "Child of node with name {0} is of type Value, and has a ValueLiteral, and has an Item of type Text, but this has no Value. Use given default value {1}.", parentName, defaultValue);
                return defaultValue;
            }
            return text.Value;
        }

        private void KonfigureDmm(ANode root, DmmChannelType channel)
        {
            channel.SourceName = FindKonfiguredValue(root, "Source", "Probes1");
            channel.Function = FindKonfiguredValue(root, "Function", "Voltage");
            channel.Coupling = FindKonfiguredValue(root, "Coupling", "DC");
            channel.Range = FindKonfiguredValue(root, "Range", "Auto");
        }
    }
}
