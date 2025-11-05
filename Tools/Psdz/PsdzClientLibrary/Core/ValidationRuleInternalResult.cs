using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class ValidationRuleInternalResult
    {
        public CharacteristicType Type { get; set; }
        public bool IsValid { get; set; }
        public string Id { get; set; }
        public decimal CharacteristicId { get; set; }
    }
}