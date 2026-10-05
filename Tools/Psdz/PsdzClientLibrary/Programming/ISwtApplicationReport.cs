using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;
using PsdzClient.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface ISwtApplicationReport
    {
        int DiagAddrAsInt { get; set; }

        FscState FscState { get; }

        ISwtApplicationId Id { get; }

        string Title { get; set; }

        IDictionary<string, string> TitleDictionary { get; set; }
    }
}
