using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using PsdzClient.Core;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Implementations
{
    public class ValidationRuleInternalResult
    {
        public CharacteristicType Type { get; set; }
        public bool IsValid { get; set; }
        public string Id { get; set; }
        public decimal CharacteristicId { get; set; }
    }
}