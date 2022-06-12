using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface ISgbmIdChange : INotifyPropertyChanged
    {
        string Actual { get; }

        string Target { get; }
    }
}
