using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMW.Rheingold.CoreFramework;

namespace PsdzClient.Core
{
    public class ParameterContainer : IParameters
    {
        private readonly HardenedStringObjectDictionary parameters;

        public static ParameterContainer Empty => new ParameterContainer();

        public int Count
        {
            get
            {
                if (parameters != null)
                {
                    return parameters.Count;
                }
                return 0;
            }
        }

        public HardenedStringObjectDictionary Parameter => parameters;

        public ParameterContainer(Dictionary<string, object> parameters)
        {
            this.parameters = new HardenedStringObjectDictionary();
            if (parameters == null)
            {
                return;
            }
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                this.parameters.Add(parameter.Key, parameter.Value);
            }
        }

        public ParameterContainer()
        {
            parameters = new HardenedStringObjectDictionary();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                stringBuilder.Append("[");
                foreach (string key in parameters.Keys)
                {
                    stringBuilder.Append("[").Append(key).Append(",");
                    object obj = parameters[key];
                    if (obj == null)
                    {
                        stringBuilder.Append("null");
                    }
                    else
                    {
                        stringBuilder.Append(obj.ToString());
                    }
                    stringBuilder.Append("],");
                }
                if (stringBuilder.Length > 1)
                {
                    stringBuilder.Length--;
                }
                stringBuilder.Append("]");
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.ToString()", exception);
            }
            return stringBuilder.ToString();
        }

        public void clearParameter(string name)
        {
            try
            {
                if (parameters.ContainsKey(name))
                {
                    parameters.Remove(name);
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.clearParameter()", exception);
            }
        }

        public void clearParameters()
        {
            try
            {
                parameters.Clear();
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.clearParameters()", exception);
            }
        }

        public void cloneParameters(ParameterContainer cloneParams)
        {
            try
            {
                if (cloneParams == null)
                {
                    return;
                }
                foreach (KeyValuePair<string, object> item in cloneParams.Parameter)
                {
                    setParameter(item.Key, item.Value);
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.cloneParameters()", exception);
            }
        }

        public KeyValuePair<string, object>? getKeyValuePair(string name)
        {
            try
            {
                KeyValuePair<string, object>? result = parameters.FirstOrDefault((KeyValuePair<string, object> item) => item.Key.Equals(name));
                if (result.HasValue && result.HasValue && !string.IsNullOrEmpty(result.Value.Key))
                {
                    return result;
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.getKeyValuePair()", exception);
            }
            return null;
        }

        public KeyValuePair<string, object>? getKeyValuePairContains(string keyContains)
        {
            try
            {
                KeyValuePair<string, object>? result = parameters.FirstOrDefault((KeyValuePair<string, object> item) => item.Key.Contains(keyContains));
                if (result.HasValue && result.HasValue && !string.IsNullOrEmpty(result.Value.Key))
                {
                    return result;
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.getKeyValuePairContains()", exception);
            }
            return null;
        }

        public KeyValuePair<string, object>? getKeyValuePairEndsWith(string keyEndsWith)
        {
            try
            {
                KeyValuePair<string, object>? result = parameters.FirstOrDefault((KeyValuePair<string, object> item) => item.Key.EndsWith(keyEndsWith, StringComparison.Ordinal));
                if (result.HasValue && result.HasValue && !string.IsNullOrEmpty(result.Value.Key))
                {
                    return result;
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.getKeyValuePairEndsWith()", exception);
            }
            return null;
        }

        public KeyValuePair<string, object>? getKeyValuePairStartsWith(string keyStartsWith)
        {
            try
            {
                KeyValuePair<string, object>? result = parameters.FirstOrDefault((KeyValuePair<string, object> item) => item.Key.StartsWith(keyStartsWith, StringComparison.Ordinal));
                if (result.HasValue && result.HasValue && !string.IsNullOrEmpty(result.Value.Key))
                {
                    return result;
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.getKeyValuePairStartsWith()", exception);
            }
            return null;
        }

        public object getParameter(string name)
        {
            try
            {
                if (parameters.ContainsKey(name))
                {
                    return parameters[name];
                }
                //Log.Warning("ParameterContainer.getParameter()", "Parameter \"{0}\" unknown.", name);
            }
            catch (Exception)
            {
                //Log.Warning("ParameterContainer.getParameter()", "failed for name \"{0}\" with exception: {1}", name, ex.ToString());
            }
            return null;
        }

        public object getParameter(string name, object defaultValue)
        {
            try
            {
                if (parameters.ContainsKey(name))
                {
                    return parameters[name];
                }
                return defaultValue;
            }
            catch (Exception)
            {
                //Log.Warning("ParameterContainer.getParameter()", "Failed for name \"{0}\" with exception: {1}", name, ex.ToString());
                return defaultValue;
            }
        }

        public object getParameterEndsWith(string nameEndsWith)
        {
            try
            {
                KeyValuePair<string, object> keyValuePair = parameters.FirstOrDefault((KeyValuePair<string, object> item) => item.Key.EndsWith(nameEndsWith, StringComparison.Ordinal));
                if (!string.IsNullOrEmpty(keyValuePair.Key))
                {
                    return keyValuePair.Value;
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.getParameterEndsWith()", exception);
            }
            return null;
        }

        public object getParameterStartsWith(string nameStartsWith)
        {
            try
            {
                KeyValuePair<string, object> keyValuePair = parameters.FirstOrDefault((KeyValuePair<string, object> item) => item.Key.StartsWith(nameStartsWith, StringComparison.Ordinal));
                if (!string.IsNullOrEmpty(keyValuePair.Key))
                {
                    return keyValuePair.Value;
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.getParameterStartsWith()", exception);
            }
            return null;
        }

        public void setParameter(string name, object parameter)
        {
            try
            {
                if (parameters.ContainsKey(name))
                {
                    parameters[name] = parameter;
                }
                else
                {
                    parameters.Add(name, parameter);
                }
            }
            catch (Exception)
            {
                //Log.WarningException("ParameterContainer.setParameter()", exception);
            }
        }
    }
}
