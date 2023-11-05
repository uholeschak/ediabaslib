namespace PsdzClientLibrary.Core
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
        DataSource.DOM,
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
                    DataSource.DOM,
                    DataSource.Database
                }
            },
            {
                multisourceProperties.VIN17PropName,
                new List<DataSource>
                {
                    DataSource.Vehicle,
                    DataSource.UserInput,
                    DataSource.DOM,
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
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.ILevelPropName,
                new List<DataSource>
                {
                    DataSource.Vehicle,
                    DataSource.FBM
                }
            },
            {
                multisourceProperties.BaustandsJahrPropName,
                new List<DataSource>
                {
                    DataSource.Vehicle,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.BaustandsMonatPropName,
                new List<DataSource>
                {
                    DataSource.Vehicle,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.BaustandPropName,
                new List<DataSource>
                {
                    DataSource.Vehicle,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.AntriebPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.BasicTypePropName,
                new List<DataSource>
                {
                    DataSource.Vehicle,
                    DataSource.FBM,
                    DataSource.DOM,
                    DataSource.Database
                }
            },
            {
                multisourceProperties.BaureihenverbundPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.EreihePropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.ElektrischeReichweitePropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.HybridkennzeichenPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.KarosseriePropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.LandPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.LenkungPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.ProdartPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.ProduktliniePropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.SicherheitsrelevantPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.TuerenPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.TypPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
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
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.AEKurzbezeichnungPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.AELeistungsklassePropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.AEUeberarbeitungPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.MotorPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.MOTEinbaulagePropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.HubraumPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.KraftstoffartPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.LeistungsklassePropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.UeberarbeitungPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.EMOTDrehmomentPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.EMOTKraftstoffartPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.EMOTLeistungsklassePropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.EMOTUeberarbeitungPropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.EMOTEinbaulagePropName,
                new List<DataSource>
                {
                    DataSource.Database,
                    DataSource.FBM,
                    DataSource.DOM
                }
            },
            {
                multisourceProperties.SerialBodyShell,
                new List<DataSource> { DataSource.DOM }
            },
            {
                multisourceProperties.SerialEngine,
                new List<DataSource> { DataSource.DOM }
            },
            {
                multisourceProperties.SerialGearBox,
                new List<DataSource> { DataSource.DOM }
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
                    DataSource.FBM
                }
            }
        };
            foreach (KeyValuePair<string, List<DataSource>> priority in priorities)
            {
                priority.Value.Add(DataSource.Legacy);
                priority.Value.Add(DataSource.Fallback);
            }
        }

        public static List<DataSource> GetPriorities(string propertyName, IMultisourceLogger log)
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
