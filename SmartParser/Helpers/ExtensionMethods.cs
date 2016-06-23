using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SmartParser.Helpers
{
    public static class ExtensionMethods
    {
        public static List<string> CloneList(this List<string> source) 
        {
            List<string> output = new List<string>();
            for (int i = 0; i < source.Count; i++)
            {
                output.Add("" + source[i]);
            }
            return output;
        }

        public static string[] CloneArray(this string[] source)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < source.Length; i++)
            {
                output.Add("" + source[i]);
            }
            return output.ToArray<string>();
        }

        public static List<T> CloneList<T>(this List<T> source)
        {
            List<T> output = new List<T>();
            for (int i = 0; i < source.Count; i++)
            {
                output.Add(source[i].Clone());
            }
            return output.ToList<T>();
        }

        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
