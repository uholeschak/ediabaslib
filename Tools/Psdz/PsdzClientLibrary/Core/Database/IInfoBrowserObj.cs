using PsdzClient.Core;
using System;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public interface IInfoBrowserObj : IEquatable<IInfoBrowserObj>
    {
        IInfoBrowserObj ParentNode { get; }

        decimal Id { get; set; }

        ObservableCollectionEx<IXepInfoObject> SetInfoObjs { get; set; }

        string Title { get; }

        string Name { get; set; }

        string Identifier { get; }

        string SortingTag { get; }

        bool IsHiddenObject { get; }
    }
}
