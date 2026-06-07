using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    [Serializable]
    public class EcuConfiguration : ICloneable
    {
        private IList<long> ecuCliques = new List<long>();
        private IList<long> ecuGroups = new List<long>();
        private IList<long> ecuRepresentatives = new List<long>();
        private IList<long> ecuVariants = new List<long>();
        private IList<long> equipments = new List<long>();
        private long iStufe;
        private IList<long> saLaPas = new List<long>();
        private IList<long> unknownEcuCliques = new List<long>();
        private IList<long> unknownEcuGroups = new List<long>();
        private IList<long> unknownEcuRepresentatives = new List<long>();
        private IList<long> unknownEcuVariants = new List<long>();
        private IList<long> unknownEquipments = new List<long>();
        public IList<long> EcuCliques
        {
            get
            {
                return ecuCliques;
            }

            set
            {
                ecuCliques = value;
            }
        }

        public IList<long> EcuGroups
        {
            get
            {
                return ecuGroups;
            }

            set
            {
                ecuGroups = value;
            }
        }

        public IList<long> EcuRepresentatives
        {
            get
            {
                return ecuRepresentatives;
            }

            set
            {
                ecuRepresentatives = value;
            }
        }

        public IList<long> EcuVariants
        {
            get
            {
                return ecuVariants;
            }

            set
            {
                ecuVariants = value;
            }
        }

        public IList<long> Equipments
        {
            get
            {
                return equipments;
            }

            set
            {
                equipments = value;
            }
        }

        public long IStufe
        {
            get
            {
                return iStufe;
            }

            set
            {
                iStufe = value;
            }
        }

        public IList<long> SaLaPas
        {
            get
            {
                return saLaPas;
            }

            set
            {
                saLaPas = value;
            }
        }

        public IList<long> UnknownEcuCliques
        {
            get
            {
                return unknownEcuCliques;
            }

            set
            {
                unknownEcuCliques = value;
            }
        }

        public IList<long> UnknownEcuGroups
        {
            get
            {
                return unknownEcuGroups;
            }

            set
            {
                unknownEcuGroups = value;
            }
        }

        public IList<long> UnknownEcuRepresentatives
        {
            get
            {
                return unknownEcuRepresentatives;
            }

            set
            {
                unknownEcuRepresentatives = value;
            }
        }

        public IList<long> UnknownEcuVariants
        {
            get
            {
                return unknownEcuVariants;
            }

            set
            {
                unknownEcuVariants = value;
            }
        }

        public IList<long> UnknownEquipments
        {
            get
            {
                return unknownEquipments;
            }

            set
            {
                unknownEquipments = value;
            }
        }

        public object Clone()
        {
            return new EcuConfiguration
            {
                EcuGroups = new List<long>(EcuGroups),
                EcuVariants = new List<long>(EcuVariants),
                EcuCliques = new List<long>(EcuCliques),
                EcuRepresentatives = new List<long>(EcuRepresentatives),
                Equipments = new List<long>(Equipments),
                SaLaPas = new List<long>(SaLaPas),
                IStufe = IStufe,
                UnknownEcuGroups = new List<long>(UnknownEcuGroups),
                UnknownEcuVariants = new List<long>(UnknownEcuVariants),
                UnknownEcuCliques = new List<long>(UnknownEcuCliques),
                UnknownEcuRepresentatives = new List<long>(UnknownEcuRepresentatives),
                UnknownEquipments = new List<long>(UnknownEquipments)
            };
        }
    }
}