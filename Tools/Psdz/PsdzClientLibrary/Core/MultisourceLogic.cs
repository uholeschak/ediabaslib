using System.Collections.Generic;
using System.Linq;

namespace PsdzClientLibrary.Core
{
    public class MultisourceLogic
    {
        private Dictionary<string, object> locks = new Dictionary<string, object>();

        private readonly DataHolder dataHolder;

        private readonly IMultisourceLogger log;

        private readonly IValueValidator valueValidator;

        public bool Enabled { get; set; }

        public MultisourceLogic(DataHolder dataHolder, IMultisourceLogger log, IMultisourceProperties multisourceProperties, IValueValidator valueValidator)
        {
            this.dataHolder = dataHolder;
            this.log = log;
            this.valueValidator = valueValidator;
            DataSourcePriority.Init(multisourceProperties);
        }

        public T SetLegacyProperty<T>(T value, string propertyName)
        {
            return SetProperty(value, DataSource.Legacy, propertyName);
        }

        public T SetProperty<T>(T value, DataSource source, string propertyName)
        {
            lock (GetLock(propertyName))
            {
                if (!valueValidator.IsValid<T>(propertyName, value))
                {
                    IList<PropertyData<T>> propertyCollection = dataHolder.GetPropertyCollection<T>(propertyName);
                    propertyCollection.Add(new PropertyData<T>(value, source)
                    {
                        IsValidValue = false
                    });
                    return propertyCollection.First().Value;
                }
                List<DataSource> priorities = DataSourcePriority.GetPriorities(propertyName, log);
                IList<PropertyData<T>> propertyCollection2 = dataHolder.GetPropertyCollection<T>(propertyName);
                if (!Enabled)
                {
                    propertyCollection2.Insert(0, new PropertyData<T>(value, source));
                    log.Info(log.CurrentMethod(), $"Fusion reactor disabled. Property: '{propertyName}' update with value = '{value}' from source = {source}");
                    return propertyCollection2.First().Value;
                }
                if (propertyCollection2.Count == 0)
                {
                    propertyCollection2.Add(new PropertyData<T>(value, source));
                    log.Info(log.CurrentMethod(), $"Property '{propertyName}': Added first item from '{source}' with value = '{value}'");
                    return propertyCollection2.First().Value;
                }
                int num2 = priorities.IndexOf(source);
                if (num2 == -1)
                {
                    PropertyData<T> propertyData = propertyCollection2.FirstOrDefault((PropertyData<T> e) => priorities.All((DataSource p) => p != e.Source) || !e.IsValidValue);
                    int num3 = -1;
                    if (propertyData != null)
                    {
                        num3 = propertyCollection2.IndexOf(propertyData);
                    }
                    if (num3 > -1)
                    {
                        propertyCollection2.Insert(num3, new PropertyData<T>(value, source));
                    }
                    else
                    {
                        propertyCollection2.Add(new PropertyData<T>(value, source));
                    }
                    log.Warning(log.CurrentMethod(), $"Property '{propertyName}': Datasource with undifined priority used! Added as element with lowest priority. Source: '{source}', Value = '{value}'");
                    return propertyCollection2.First().Value;
                }
                int count = propertyCollection2.Count;
                int num4 = 0;
                while (true)
                {
                    if (num4 < count)
                    {
                        PropertyData<T> propertyData2 = propertyCollection2[num4];
                        int num5 = priorities.IndexOf(propertyData2.Source);
                        if (num5 != -1 && propertyData2.IsValidValue)
                        {
                            if (num2 <= num5)
                            {
                                break;
                            }
                            if (num4 == propertyCollection2.Count - 1)
                            {
                                propertyCollection2.Add(new PropertyData<T>(value, source));
                                log.Info(log.CurrentMethod(), $"Property '{propertyName}': Added new item from '{source}' with value = '{value}'");
                            }
                            num4++;
                            continue;
                        }
                        propertyCollection2.Insert(0, new PropertyData<T>(value, source));
                        log.Info(log.CurrentMethod(), $"Property '{propertyName}': Inserted new item from '{source}' with value = '{value}'");
                        return propertyCollection2.First().Value;
                    }
                    return propertyCollection2.First().Value;
                }
                propertyCollection2.Insert(num4, new PropertyData<T>(value, source));
                log.Info(log.CurrentMethod(), $"Property '{propertyName}': Inserted new item from '{source}' with value = '{value}'");
                return propertyCollection2.First().Value;
            }
        }

        private object GetLock(string propertyName)
        {
            if (locks.ContainsKey(propertyName))
            {
                return locks[propertyName];
            }
            object obj = new object();
            locks[propertyName] = obj;
            return obj;
        }
    }
}
