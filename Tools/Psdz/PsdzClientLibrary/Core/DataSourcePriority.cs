namespace PsdzClient.Core
{
    // BMW.ISPI.TRIC.ISTA.MultisourceLogic, Version=4.35.0.0, Culture=neutral, PublicKeyToken=6505efbdc3e5f324
    // BMW.ISPI.TRIC.ISTA.MultisourceLogic.DataSourcePriority
    using System.Collections.Generic;

    public class DataSourcePriority
    {
        private static List<DataSource> defaultPriorities = new List<DataSource>
        {
            DataSource.Database,
            DataSource.FBM,
            DataSource.SVMD,
            DataSource.Legacy,
            DataSource.Fallback
        };
        private static Dictionary<string, List<DataSource>> priorities;
        private DataSourcePriority()
        {
        }

        public static void Init(IMultisourceProperties multisourceProperties)
        {
            priorities = new Dictionary<string, List<DataSource>>
            {
                {
                    multisourceProperties.ProductionDatePropName,
                    new List<DataSource>
                    {
                        DataSource.FBM,
                        DataSource.SVMD,
                        DataSource.Database
                    }
                },
                {
                    multisourceProperties.VIN17PropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.UserInput,
                        DataSource.SVMD,
                        DataSource.FBM
                    }
                },
                {
                    multisourceProperties.VINRangeTypePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.UserInput
                    }
                },
                {
                    multisourceProperties.ILevelWerkPropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.SVMD,
                        DataSource.Hardcoded
                    }
                },
                {
                    multisourceProperties.ILevelPropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.Hardcoded
                    }
                },
                {
                    multisourceProperties.BaustandsJahrPropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.BaustandsMonatPropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.BaustandPropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.ModelljahrPropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.ModellmonatPropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.ModelltagPropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.AntriebPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.BasicTypePropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.SVMD,
                        DataSource.Database
                    }
                },
                {
                    multisourceProperties.BaureihenverbundPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.EreihePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.ElektrischeReichweitePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.HybridkennzeichenPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.KarosseriePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.LandPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.LenkungPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.ProdartPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.ProduktliniePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.SicherheitsrelevantPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.TuerenPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.TypPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.VerkaufsBezeichnungPropName,
                    new List<DataSource>
                    {
                        DataSource.FBM,
                        DataSource.Database
                    }
                },
                {
                    multisourceProperties.AEBezeichnungPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.AEKurzbezeichnungPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.AELeistungsklassePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.AEUeberarbeitungPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.MotorPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.MOTEinbaulagePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.HubraumPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.KraftstoffartPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.LeistungsklassePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.UeberarbeitungPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.EMOTDrehmomentPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.EMOTKraftstoffartPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.EMOTLeistungsklassePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.EMOTUeberarbeitungPropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.EMOTEinbaulagePropName,
                    new List<DataSource>
                    {
                        DataSource.Database,
                        DataSource.FBM,
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.SerialBodyShell,
                    new List<DataSource>
                    {
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.SerialEngine,
                    new List<DataSource>
                    {
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.SerialGearBox,
                    new List<DataSource>
                    {
                        DataSource.SVMD
                    }
                },
                {
                    multisourceProperties.FirstRegistration,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM
                    }
                },
                {
                    multisourceProperties.FAPropName,
                    new List<DataSource>
                    {
                        DataSource.Vehicle,
                        DataSource.FBM,
                        DataSource.Hardcoded
                    }
                }
            };
            foreach (KeyValuePair<string, List<DataSource>> priority in priorities)
            {
                priority.Value.Add(DataSource.Legacy);
                priority.Value.Add(DataSource.Fallback);
            }
        }

        public static List<DataSource> GetPriorities(string propertyName, ILogger log)
        {
            List<DataSource> result = defaultPriorities;
            if (priorities.ContainsKey(propertyName))
            {
                result = priorities[propertyName];
            }
            else
            {
                log.Info(log.CurrentMethod(), "Default set of priorities was used for property: '" + propertyName + "'.");
            }

            return result;
        }
    }
}