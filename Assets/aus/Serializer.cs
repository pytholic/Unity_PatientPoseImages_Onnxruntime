// https://bitbucket.org/alkee/aus

using System.Runtime.Serialization;
using UnityEngine;

namespace aus
{

    public static class Formatter
    {
        public static System.Runtime.Serialization.Formatters.Binary.BinaryFormatter CreateSerializeFormatter()
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var ss = new SurrogateSelector();
            ss.AddSurrogate(
                typeof(Vector3), new StreamingContext(StreamingContextStates.All),
                new Vector3SerializationSurrogate()
                );
            ss.AddSurrogate(
                typeof(Quaternion), new StreamingContext(StreamingContextStates.All),
                new QuaternionSerializationSurrogate()
                );
            formatter.SurrogateSelector = ss;
            return formatter;
        }
    }

    #region ISerializationSurrogate implementations

    sealed class Vector3SerializationSurrogate : ISerializationSurrogate
    {
        // Method called to serialize a Vector3 object
        public void GetObjectData(object obj,
                                  SerializationInfo info, StreamingContext context)
        {

            Vector3 v3 = (Vector3)obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }

        // Method called to deserialize a Vector3 object
        public object SetObjectData(object obj,
                                           SerializationInfo info, StreamingContext context,
                                           ISurrogateSelector selector)
        {

            Vector3 v3 = (Vector3)obj;
            v3.x = (float)info.GetValue("x", typeof(float));
            v3.y = (float)info.GetValue("y", typeof(float));
            v3.z = (float)info.GetValue("z", typeof(float));
            obj = v3;
            return obj;   // Formatters ignore this return value //Seems to have been fixed!
        }
    }

    sealed class QuaternionSerializationSurrogate : ISerializationSurrogate
    {
        // Method called to serialize a Vector3 object
        public void GetObjectData(object obj,
                                  SerializationInfo info, StreamingContext context)
        {

            Quaternion q = (Quaternion)obj;
            info.AddValue("w", q.w);
            info.AddValue("x", q.x);
            info.AddValue("y", q.y);
            info.AddValue("z", q.z);
        }

        // Method called to deserialize a Vector3 object
        public object SetObjectData(object obj,
                                           SerializationInfo info, StreamingContext context,
                                           ISurrogateSelector selector)
        {

            Quaternion q = (Quaternion)obj;
            q.w = (float)info.GetValue("w", typeof(float));
            q.x = (float)info.GetValue("x", typeof(float));
            q.y = (float)info.GetValue("y", typeof(float));
            q.z = (float)info.GetValue("z", typeof(float));
            obj = q;
            return obj;   // Formatters ignore this return value //Seems to have been fixed!
        }
    }

    #endregion
}
