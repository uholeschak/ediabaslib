using BMW.Authoring.API;
using System.Collections.Generic;

namespace BMW.Authoring.API
{
    public class RandomForestObjectCreator<T> where T : IRandomForest, new()
    {
        private readonly Dictionary<string, T> randomForestDictionary;

        public T this[string name]
        {
            get
            {
                T value = default(T);
                if (!randomForestDictionary.TryGetValue(name, out value))
                {
                    value = new T();
                    randomForestDictionary[name] = value;
                }
                return value;
            }
        }

        public RandomForestObjectCreator()
        {
            randomForestDictionary = new Dictionary<string, T>();
        }
    }
}
