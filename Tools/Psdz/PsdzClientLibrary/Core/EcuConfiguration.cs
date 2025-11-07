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
            EcuConfiguration ecuConfiguration = new EcuConfiguration();
            ecuConfiguration.EcuGroups = new List<long>(EcuGroups);
            ecuConfiguration.EcuVariants = new List<long>(EcuVariants);
            ecuConfiguration.EcuCliques = new List<long>(EcuCliques);
            ecuConfiguration.EcuRepresentatives = new List<long>(EcuRepresentatives);
            ecuConfiguration.Equipments = new List<long>(Equipments);
            ecuConfiguration.SaLaPas = new List<long>(SaLaPas);
            ecuConfiguration.IStufe = IStufe;
            ecuConfiguration.UnknownEcuGroups = new List<long>(UnknownEcuGroups);
            ecuConfiguration.UnknownEcuVariants = new List<long>(UnknownEcuVariants);
            ecuConfiguration.UnknownEcuCliques = new List<long>(UnknownEcuCliques);
            ecuConfiguration.UnknownEcuRepresentatives = new List<long>(UnknownEcuRepresentatives);
            ecuConfiguration.UnknownEquipments = new List<long>(UnknownEquipments);
            return ecuConfiguration;
        }
    }
}