using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BmwFileReader
{
    /// <summary>
    /// Base on https://weblogs.asp.net/pwelter34/444961
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable, ICloneable
    {
        // XmlSerializer.Deserialize() will create a new Object, and then call ReadXml()
        // So cannot use instance field, use class field.

        public static string itemTag = "item";
        public static string keyTag = "key";
        public static string valueTag = "value";

        public SerializableDictionary()
        {
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) :
            base(dictionary)
        {
        }

        public SerializableDictionary(IEqualityComparer<TKey> comparer) :
            base(comparer)
        {
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) :
            base(dictionary, comparer)
        {
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;

            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            reader.ReadStartElement();

            // IsStartElement() will call MoveToContent()
            // reader.MoveToContent();
            while (reader.IsStartElement(itemTag))
            {
                reader.ReadStartElement(itemTag);

                reader.ReadStartElement(keyTag);
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement(valueTag);
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadEndElement();
                this.Add(key, value);

                // IsStartElement() will call MoveToContent()
                // reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (var kvp in this)
            {
                writer.WriteStartElement(itemTag);

                writer.WriteStartElement(keyTag);
                keySerializer.Serialize(writer, kvp.Key);
                writer.WriteEndElement();

                writer.WriteStartElement(valueTag);
                valueSerializer.Serialize(writer, kvp.Value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        public SerializableDictionary<TKey, TValue> Clone()
        {
            SerializableDictionary<TKey, TValue> other = new SerializableDictionary<TKey, TValue>(Comparer);
            foreach (KeyValuePair<TKey, TValue> keyValue in this)
            {
                other.Add(keyValue.Key, keyValue.Value);
            }
            return other;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
