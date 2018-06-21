using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiteNetworking
{
    public class CustomSerializer : Attribute
    {
        public Type overrider;
        public Type thisAttributeOnly;

        public CustomSerializer(Type overrider)
        {
            this.overrider = overrider;
        }

        public CustomSerializer(Type overrider, Type thisAttributeOnly)
        {
            this.overrider = overrider;
            this.thisAttributeOnly = thisAttributeOnly;
        }
    }

    public class DataSerialization
    {

        // Maps {base type} to a list of all < serializer class, attribute in which base class must have >
        public static Dictionary<Type,List<KeyValuePair<Type,Type>>> GetAllSerializers()
        {
            //  maps the base data type --> the class that will serialize it
            Dictionary<Type, List<KeyValuePair<Type, Type>>> allSerializers = new Dictionary<Type, List<KeyValuePair<Type, Type>>>();

            // Map the base serializers 
            var subclasses =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(LiteByteSerializer<>)
                select type;


            foreach(Type t in subclasses)
            {
                if (t.GetCustomAttributes(typeof(CustomSerializer), true).Length > 0) continue;
                Type baseDataType = t.BaseType.GetGenericArguments()[0];

                if (!allSerializers.ContainsKey(baseDataType)) allSerializers[baseDataType] = new List<KeyValuePair<Type, Type>>();
                allSerializers[baseDataType].Add(new KeyValuePair<Type, Type>(t, null));
            }

            // Map the serializer overrides
            var attrClases =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.GetCustomAttributes(typeof(CustomSerializer), true).Length > 0
                select type;

            foreach(Type t in attrClases)
            {
                CustomSerializer srz = t.GetCustomAttributes(typeof(CustomSerializer), true)[0] as CustomSerializer;
                Type baseDataType = t.BaseType.GetGenericArguments()[0];

                if (!allSerializers.ContainsKey(baseDataType)) allSerializers[baseDataType] = new List<KeyValuePair<Type, Type>>();
                allSerializers[baseDataType].Add(new KeyValuePair<Type, Type>(t, srz.thisAttributeOnly));
            }

            return allSerializers;

        }
    }



}
