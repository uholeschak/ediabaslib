using System;
using System.Collections.Generic;
using System.IO;

namespace BmwDeepObd
{
    public static class RecentConfigUtils
    {
        private static readonly object RecentConfigLockObject = new object();
        private static readonly List<string> _recentConfigList = new List<string>();

        public static List<string> GetRecentConfigList()
        {
            lock (RecentConfigLockObject)
            {
                return new List<string>(_recentConfigList);
            }
        }

        public static void SetRecentConfigList(List<string> configList)
        {
            lock (RecentConfigLockObject)
            {
                _recentConfigList.Clear();
                if (configList != null)
                {
                    foreach (string fileName in configList)
                    {
                        if (!_recentConfigList.Contains(fileName))
                        {
                            _recentConfigList.Add(fileName);
                        }
                    }
                }
            }
        }

        public static void RecentConfigListClear()
        {
            lock (RecentConfigLockObject)
            {
                _recentConfigList.Clear();
            }
        }

        public static bool RecentConfigListAdd(string fileName)
        {
            try
            {
                lock (RecentConfigLockObject)
                {
                    if (_recentConfigList.Contains(fileName))
                    {
                        _recentConfigList.Remove(fileName);
                    }

                    _recentConfigList.Insert(0, fileName);
                    while (_recentConfigList.Count > 10)
                    {
                        _recentConfigList.RemoveAt(_recentConfigList.Count - 1);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool RecentConfigListRemove(string fileName)
        {
            try
            {
                lock (RecentConfigLockObject)
                {
                    if (_recentConfigList.Contains(fileName))
                    {
                        _recentConfigList.Remove(fileName);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public static bool RecentConfigListCleanup()
        {
            try
            {
                lock (RecentConfigLockObject)
                {
                    int index = 0;
                    while (index < _recentConfigList.Count)
                    {
                        if (!File.Exists(_recentConfigList[index]))
                        {
                            _recentConfigList.RemoveAt(index);
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}
