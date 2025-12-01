using System;

namespace PsdzClient.Core.Container
{
    internal class BaseAdapter
    {
        protected bool StandardErrorHandling;

        protected ConfigurationContainer configContainer;

        public BaseAdapter(bool StandardErrorHandling, ConfigurationContainer configContainer)
        {
            this.StandardErrorHandling = StandardErrorHandling;
            this.configContainer = configContainer;
        }

        protected void diveConfigNodes(ParameterContainer NodePathList, ANode node, string path, bool binaryRequest)
        {
            path += $"/{node.Name}";
            if (node is All)
            {
                All all = (All)node;
                if (all.Children != null)
                {
                    if (all.Children.Count > 0)
                    {
                        foreach (ANode child in all.Children)
                        {
                            diveConfigNodes(NodePathList, child, path, binaryRequest);
                        }
                        return;
                    }
                    Log.Info("BaseAdapter.diveConfigNodes()", "found subnode of type All with 0 children");
                    if (!path.Contains("ECUGroupOrVariant"))
                    {
                        NodePathList.setParameter(path + "/ECUGroupOrVariant", null);
                    }
                    else
                    {
                        NodePathList.setParameter(path, null);
                    }
                }
                else
                {
                    Log.Warning("BaseAdapter.diveConfigNodes()", "found unhandled configuration container path: {0}", path);
                }
            }
            else if (node is Value)
            {
                Value value = (Value)node;
                if (value.Literal != null)
                {
                    ValueLiteral literal = value.Literal;
                    if (literal.Item is byte[])
                    {
                        binaryRequest = true;
                        byte[] parameter = (byte[])literal.Item;
                        NodePathList.setParameter(path, parameter);
                    }
                    else if (literal.Item is bool)
                    {
                        bool flag = (bool)literal.Item;
                        NodePathList.setParameter(path, flag);
                    }
                    else if (literal.Item is DateTime)
                    {
                        DateTime dateTime = (DateTime)literal.Item;
                        NodePathList.setParameter(path, dateTime);
                    }
                    else if (literal.Item is decimal)
                    {
                        decimal num = (decimal)literal.Item;
                        NodePathList.setParameter(path, num);
                    }
                    else if (literal.Item is double)
                    {
                        double num2 = (double)literal.Item;
                        NodePathList.setParameter(path, num2);
                    }
                    else if (literal.Item is float)
                    {
                        float num3 = (float)literal.Item;
                        NodePathList.setParameter(path, num3);
                    }
                    else if (literal.Item is int)
                    {
                        int num4 = (int)literal.Item;
                        NodePathList.setParameter(path, num4);
                    }
                    else if (literal.Item is long)
                    {
                        long num5 = (long)literal.Item;
                        NodePathList.setParameter(path, num5);
                    }
                    else if (literal.Item is sbyte)
                    {
                        sbyte b = (sbyte)literal.Item;
                        NodePathList.setParameter(path, b);
                    }
                    else if (literal.Item is short)
                    {
                        short num6 = (short)literal.Item;
                        NodePathList.setParameter(path, num6);
                    }
                    else if (literal.Item is Text)
                    {
                        Text text = literal.Item as Text;
                        NodePathList.setParameter(path, text.Value);
                    }
                    else if (literal.Item is byte)
                    {
                        byte b2 = (byte)literal.Item;
                        NodePathList.setParameter(path, b2);
                    }
                    else if (literal.Item is uint)
                    {
                        uint num7 = (uint)literal.Item;
                        NodePathList.setParameter(path, num7);
                    }
                    else if (literal.Item is ulong)
                    {
                        ulong num8 = (ulong)literal.Item;
                        NodePathList.setParameter(path, num8);
                    }
                    else if (literal.Item is ushort)
                    {
                        ushort num9 = (ushort)literal.Item;
                        NodePathList.setParameter(path, num9);
                    }
                    else if (literal.Item == null)
                    {
                        NodePathList.setParameter(path, null);
                    }
                    else
                    {
                        Log.Warning("BaseAdapter.diveConfigNodes()", "Path: {0} Value: unknown Type: {1}", path, literal.Item.GetType().Name);
                    }
                }
            }
            else
            {
                if (node is SingleChoice)
                {
                    SingleChoice singleChoice = (SingleChoice)node;
                    if (singleChoice.Children == null)
                    {
                        return;
                    }
                    {
                        foreach (ANode child2 in singleChoice.Children)
                        {
                            diveConfigNodes(NodePathList, child2, path, binaryRequest);
                        }
                        return;
                    }
                }
                if (node is Executable)
                {
                    Executable executable = (Executable)node;
                    if (executable.Children == null)
                    {
                        return;
                    }
                    {
                        foreach (ANode child3 in executable.Children)
                        {
                            diveConfigNodes(NodePathList, child3, path, binaryRequest);
                        }
                        return;
                    }
                }
                if (node is MultipleChoice)
                {
                    MultipleChoice multipleChoice = (MultipleChoice)node;
                    if (multipleChoice.Children == null)
                    {
                        return;
                    }
                    {
                        foreach (ANode child4 in multipleChoice.Children)
                        {
                            diveConfigNodes(NodePathList, child4, path, binaryRequest);
                        }
                        return;
                    }
                }
                if (node is Sequence)
                {
                    Sequence sequence = (Sequence)node;
                    if (sequence.Children == null)
                    {
                        return;
                    }
                    {
                        foreach (ANode child5 in sequence.Children)
                        {
                            diveConfigNodes(NodePathList, child5, path, binaryRequest);
                        }
                        return;
                    }
                }
                if (node is QuantityChoice)
                {
                    QuantityChoice quantityChoice = (QuantityChoice)node;
                    if (quantityChoice.Children == null)
                    {
                        return;
                    }
                    {
                        foreach (ANode child6 in quantityChoice.Children)
                        {
                            diveConfigNodes(NodePathList, child6, path, binaryRequest);
                        }
                        return;
                    }
                }
                if (!(node is AChoice))
                {
                    return;
                }
                AChoice aChoice = (AChoice)node;
                if (aChoice.Children == null)
                {
                    return;
                }
                foreach (ANode child7 in aChoice.Children)
                {
                    diveConfigNodes(NodePathList, child7, path, binaryRequest);
                }
            }
        }
    }
}
