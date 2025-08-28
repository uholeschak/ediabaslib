using System;

namespace BMW.Rheingold.Psdz.Model
{
    public interface IPsdzConnection
    {
        Guid Id { get; }

        IPsdzTargetSelector TargetSelector { get; }

        int Port { get; }
    }
}
