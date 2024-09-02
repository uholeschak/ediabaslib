// *******************************
// *** INIFile class V2.1      ***
// *******************************
// *** (C)2009-2013 S.T.A. snc ***
// *******************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseNullPropagation
// ReSharper disable RedundantAssignment
// ReSharper disable TooWideLocalVariableScope

namespace EdiabasLib
{

    public class IniFile
    {

        #region "Declarations"

        // *** Lock for thread-safe access to file and local cache ***
        private readonly object _mLock = new object();

        // *** File name ***
        private string _mFileName;

        public string FileName
        {
            get { return _mFileName; }
        }

        // *** Lazy loading flag ***
        private bool _mLazy;

        // *** Automatic flushing flag ***
        private bool _mAutoFlush;

        // *** Local cache ***
        private readonly Dictionary<string, Dictionary<string, string>> _mSections =
            new Dictionary<string, Dictionary<string, string>>();

        private readonly Dictionary<string, Dictionary<string, string>> _mModified =
            new Dictionary<string, Dictionary<string, string>>();

        // *** Local cache modified flag ***
        private bool _mCacheModified;

        #endregion

        #region "Methods"

        // *** Constructor ***
        public IniFile(string fileName)
        {
            Initialize(fileName, false, false);
        }

        public IniFile(string fileName, bool lazy, bool autoFlush)
        {
            Initialize(fileName, lazy, autoFlush);
        }

        // *** Initialization ***
        private void Initialize(string fileName, bool lazy, bool autoFlush)
        {
            _mFileName = fileName;
            _mLazy = lazy;
            _mAutoFlush = autoFlush;
            if (!_mLazy) Refresh();
        }

        // *** Parse section name ***
        private string ParseSectionName(string line)
        {
            if (!line.StartsWith("[")) return null;
            if (!line.EndsWith("]")) return null;
            if (line.Length < 3) return null;
            return line.Substring(1, line.Length - 2);
        }

        // *** Parse key+value pair ***
        private bool ParseKeyValuePair(string line, ref string key, ref string value)
        {
            // *** Check for key+value pair ***
            int i;
            if ((i = line.IndexOf('=')) <= 0) return false;

            int j = line.Length - i - 1;
            key = line.Substring(0, i).Trim();
            if (key.Length <= 0) return false;

            value = (j > 0) ? (line.Substring(i + 1, j).Trim()) : ("");
            return true;
        }

        // *** Read file contents into local cache ***
        public void Refresh()
        {
            lock (_mLock)
            {
                StreamReader sr = null;
                try
                {
                    // *** Clear local cache ***
                    _mSections.Clear();
                    _mModified.Clear();

                    // *** Open the INI file ***
                    try
                    {
                        sr = new StreamReader(_mFileName);
                    }
                    catch (FileNotFoundException)
                    {
                        return;
                    }

                    // *** Read up the file content ***
                    Dictionary<string, string> currentSection = null;
                    string s;
                    string sectionName;
                    string key = null;
                    string value = null;
                    while ((s = sr.ReadLine()) != null)
                    {
                        s = s.Trim();

                        // *** Check for section names ***
                        sectionName = ParseSectionName(s);
                        if (sectionName != null)
                        {
                            // *** Only first occurrence of a section is loaded ***
                            if (_mSections.ContainsKey(sectionName))
                            {
                                currentSection = null;
                            }
                            else
                            {
                                currentSection = new Dictionary<string, string>();
                                _mSections.Add(sectionName, currentSection);
                            }
                        }
                        else if (currentSection != null)
                        {
                            // *** Check for key+value pair ***
                            if (ParseKeyValuePair(s, ref key, ref value))
                            {
                                // *** Only first occurrence of a key is loaded ***
                                if (!currentSection.ContainsKey(key))
                                {
                                    currentSection.Add(key, value);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    // *** Cleanup: close file ***
                    if (sr != null) sr.Close();
                    sr = null;
                }
            }
        }

        // *** Flush local cache content ***
        public void Flush()
        {
            lock (_mLock)
            {
                PerformFlush();
            }
        }

        public void PerformFlush()
        {
            if (_mFileName == null)
            {
                return;
            }
            // *** If local cache was not modified, exit ***
            if (!_mCacheModified)
            {
                return;
            }
            _mCacheModified = false;

            // *** Check if original file exists ***
            bool originalFileExists = File.Exists(_mFileName);

            // *** Get temporary file name ***
            string tmpFileName = Path.ChangeExtension(_mFileName, "$n$");
            if (string.IsNullOrEmpty(tmpFileName))
            {
                return;
            }

            // *** Copy content of original file to temporary file, replace modified values ***
            StreamWriter sw = null;

            // *** Create the temporary file ***
            sw = new StreamWriter(tmpFileName);

            try
            {
                Dictionary<string, string> currentSection = null;
                if (originalFileExists)
                {
                    StreamReader sr = null;
                    try
                    {
                        // *** Open the original file ***
                        sr = new StreamReader(_mFileName);

                        // *** Read the file original content, replace changes with local cache values ***
                        string s;
                        string sectionName;
                        string key = null;
                        string value = null;
                        bool unmodified;
                        bool reading = true;
                        while (reading)
                        {
                            s = sr.ReadLine();
                            reading = (s != null);

                            // *** Check for end of file ***
                            if (reading)
                            {
                                unmodified = true;
                                s = s.Trim();
                                sectionName = ParseSectionName(s);
                            }
                            else
                            {
                                unmodified = false;
                                sectionName = null;
                            }

                            // *** Check for section names ***
                            if ((sectionName != null) || (!reading))
                            {
                                if (currentSection != null)
                                {
                                    // *** Write all remaining modified values before leaving a section ****
                                    if (currentSection.Count > 0)
                                    {
                                        foreach (string fkey in currentSection.Keys)
                                        {
                                            if (currentSection.TryGetValue(fkey, out value))
                                            {
                                                sw.Write(fkey);
                                                sw.Write('=');
                                                sw.WriteLine(value);
                                            }
                                        }
                                        sw.WriteLine();
                                        currentSection.Clear();
                                    }
                                }

                                if (reading)
                                {
                                    // *** Check if current section is in local modified cache ***
                                    if (!_mModified.TryGetValue(sectionName, out currentSection))
                                    {
                                        currentSection = null;
                                    }
                                }
                            }
                            else if (currentSection != null)
                            {
                                // *** Check for key+value pair ***
                                if (ParseKeyValuePair(s, ref key, ref value))
                                {
                                    if (currentSection.TryGetValue(key, out value))
                                    {
                                        // *** Write modified value to temporary file ***
                                        unmodified = false;
                                        currentSection.Remove(key);

                                        sw.Write(key);
                                        sw.Write('=');
                                        sw.WriteLine(value);
                                    }
                                }
                            }

                            // *** Write unmodified lines from the original file ***
                            if (unmodified)
                            {
                                sw.WriteLine(s);
                            }
                        }

                        // *** Close the original file ***
                        sr.Close();
                        sr = null;
                    }
                    finally
                    {
                        // *** Cleanup: close files ***
                        if (sr != null) sr.Close();
                        sr = null;
                    }
                }

                // *** Cycle on all remaining modified values ***
                foreach (KeyValuePair<string, Dictionary<string, string>> sectionPair in _mModified)
                {
                    currentSection = sectionPair.Value;
                    if (currentSection.Count > 0)
                    {
                        sw.WriteLine();

                        // *** Write the section name ***
                        sw.Write('[');
                        sw.Write(sectionPair.Key);
                        sw.WriteLine(']');

                        // *** Cycle on all key+value pairs in the section ***
                        foreach (KeyValuePair<string, string> valuePair in currentSection)
                        {
                            // *** Write the key+value pair ***
                            sw.Write(valuePair.Key);
                            sw.Write('=');
                            sw.WriteLine(valuePair.Value);
                        }
                        currentSection.Clear();
                    }
                }
                _mModified.Clear();

                // *** Close the temporary file ***
                sw.Close();
                sw = null;

                // *** Rename the temporary file ***
                File.Copy(tmpFileName, _mFileName, true);

                // *** Delete the temporary file ***
                File.Delete(tmpFileName);
            }
            finally
            {
                // *** Cleanup: close files ***
                if (sw != null)
                {
                    sw.Close();
                }
            }
        }

        // *** Read a value from local cache ***
        public List<string> GetKeys(string sectionName)
        {
            // *** Lazy loading ***
            if (_mLazy)
            {
                _mLazy = false;
                Refresh();
            }

            lock (_mLock)
            {
                // *** Check if the section exists ***
                Dictionary<string, string> section;
                if (!_mSections.TryGetValue(sectionName, out section))
                {
                    return null;
                }

                // *** Return the key list ***
                return section.Keys.ToList();
            }
        }

        // *** Read a value from local cache ***
        public string GetValue(string sectionName, string key, string defaultValue)
        {
            // *** Lazy loading ***
            if (_mLazy)
            {
                _mLazy = false;
                Refresh();
            }

            lock (_mLock)
            {
                // *** Check if the section exists ***
                Dictionary<string, string> section;
                if (!_mSections.TryGetValue(sectionName, out section))
                {
                    return defaultValue;
                }

                // *** Check if the key exists ***
                string value;
                if (!section.TryGetValue(key, out value))
                {
                    return defaultValue;
                }

                // *** Return the found value ***
                return value;
            }
        }

        // *** Insert or modify a value in local cache ***
        public void SetValue(string sectionName, string key, string value)
        {
            // *** Lazy loading ***
            if (_mLazy)
            {
                _mLazy = false;
                Refresh();
            }

            lock (_mLock)
            {
                // *** Flag local cache modification ***
                _mCacheModified = true;

                // *** Check if the section exists ***
                Dictionary<string, string> section;
                if (!_mSections.TryGetValue(sectionName, out section))
                {
                    // *** If it doesn't, add it ***
                    section = new Dictionary<string, string>();
                    _mSections.Add(sectionName, section);
                }

                // *** Modify the value ***
                if (section.ContainsKey(key)) section.Remove(key);
                section.Add(key, value);

                // *** Add the modified value to local modified values cache ***
                if (!_mModified.TryGetValue(sectionName, out section))
                {
                    section = new Dictionary<string, string>();
                    _mModified.Add(sectionName, section);
                }

                if (section.ContainsKey(key))
                {
                    section.Remove(key);
                }
                section.Add(key, value);

                // *** Automatic flushing : immediately write any modification to the file ***
                if (_mAutoFlush) PerformFlush();
            }
        }

        // *** Encode byte array ***
        private string EncodeByteArray(byte[] value)
        {
            if (value == null) return null;

            StringBuilder sb = new StringBuilder();
            foreach (byte b in value)
            {
                string hex = Convert.ToString(b, 16);
                int l = hex.Length;
                if (l > 2)
                {
                    sb.Append(hex.Substring(l - 2, 2));
                }
                else
                {
                    if (l < 2) sb.Append("0");
                    sb.Append(hex);
                }
            }
            return sb.ToString();
        }

        // *** Decode byte array ***
        private byte[] DecodeByteArray(string value)
        {
            if (value == null) return null;

            int l = value.Length;
            if (l < 2) return new byte[] {};

            l /= 2;
            byte[] result = new byte[l];
            for (int i = 0; i < l; i++) result[i] = Convert.ToByte(value.Substring(i*2, 2), 16);
            return result;
        }

        // *** Getters for various types ***
        public bool GetValue(string sectionName, string key, bool defaultValue)
        {
            string stringValue = GetValue(sectionName, key, defaultValue.ToString(CultureInfo.InvariantCulture));
            try
            {
                return int.Parse(stringValue) != 0;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public int GetValue(string sectionName, string key, int defaultValue)
        {
            string stringValue = GetValue(sectionName, key, defaultValue.ToString(CultureInfo.InvariantCulture));
            try
            {
                return int.Parse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public long GetValue(string sectionName, string key, long defaultValue)
        {
            string stringValue = GetValue(sectionName, key, defaultValue.ToString(CultureInfo.InvariantCulture));
            try
            {
                return long.Parse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public double GetValue(string sectionName, string key, double defaultValue)
        {
            string stringValue = GetValue(sectionName, key, defaultValue.ToString(CultureInfo.InvariantCulture));
            try
            {
                return double.Parse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public byte[] GetValue(string sectionName, string key, byte[] defaultValue)
        {
            string stringValue = GetValue(sectionName, key, EncodeByteArray(defaultValue));
            try
            {
                return DecodeByteArray(stringValue);
            }
            catch (FormatException)
            {
                return defaultValue;
            }
        }

        public DateTime GetValue(string sectionName, string key, DateTime defaultValue)
        {
            string stringValue = GetValue(sectionName, key, defaultValue.ToString(CultureInfo.InvariantCulture));
            try
            {
                return DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeLocal);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        // *** Setters for various types ***
        public void SetValue(string sectionName, string key, bool value)
        {
            SetValue(sectionName, key, (value) ? ("1") : ("0"));
        }

        public void SetValue(string sectionName, string key, int value)
        {
            SetValue(sectionName, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void SetValue(string sectionName, string key, long value)
        {
            SetValue(sectionName, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void SetValue(string sectionName, string key, double value)
        {
            SetValue(sectionName, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void SetValue(string sectionName, string key, byte[] value)
        {
            SetValue(sectionName, key, EncodeByteArray(value));
        }

        public void SetValue(string sectionName, string key, DateTime value)
        {
            SetValue(sectionName, key, value.ToString(CultureInfo.InvariantCulture));
        }

        #endregion

    }

}
