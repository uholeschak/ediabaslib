using System.Collections;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class LcSwitchList : ILcSwitchList, IList<ILcSwitch>, ICollection<ILcSwitch>, IEnumerable<ILcSwitch>, IEnumerable
    {
        private IList<ILcSwitch> lcSwitchList { get; set; } = new List<ILcSwitch>();

        public ILcSwitch this[int index]
        {
            get
            {
                return lcSwitchList[index];
            }
            set
            {
                lcSwitchList[index] = value;
            }
        }

        public int Count => lcSwitchList.Count;

        public bool IsReadOnly => lcSwitchList.IsReadOnly;

        public void Add(ILcSwitch item)
        {
            lcSwitchList.Add(item);
        }

        public void Clear()
        {
            lcSwitchList.Clear();
        }

        public bool Contains(ILcSwitch item)
        {
            return lcSwitchList.Contains(item);
        }

        public void CopyTo(ILcSwitch[] array, int arrayIndex)
        {
            lcSwitchList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ILcSwitch> GetEnumerator()
        {
            return lcSwitchList.GetEnumerator();
        }

        public int IndexOf(ILcSwitch item)
        {
            return lcSwitchList.IndexOf(item);
        }

        public void Insert(int index, ILcSwitch item)
        {
            lcSwitchList.Insert(index, item);
        }

        public bool Remove(ILcSwitch item)
        {
            return lcSwitchList.Remove(item);
        }

        public void RemoveAt(int index)
        {
            lcSwitchList.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return lcSwitchList.GetEnumerator();
        }
    }
}
