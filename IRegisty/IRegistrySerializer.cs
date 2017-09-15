using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace IRegisty
{
    public class IRegistrySerializer
    {
        
        public IRegistrySerializer()
        {
        }
        
        public static void Serialize(object obj, RegistryKey regkey)
        {
            if (obj == null) return;

            Type T = obj.GetType();
            
            if (T == typeof(bool))
            {

                regkey.SetValue(null, obj);

            }
            else if (T.IsPrimitive | T.IsClass & T == typeof(string))
            {

                regkey.SetValue(null, obj);

            }
            else if (T == typeof(DateTime))
            {

                regkey.SetValue(null, ((DateTime)obj).ToFileTime());
                
            }
            else if (T.IsArray)
            {

                Array arr = (Array)obj;
                for (int i = 0; i < arr.Length; i++)
                {
                    Serialize(((Array)obj).GetValue(i), regkey.CreateSubKey(i.ToString()));
                }
                
            }
            else if (T.IsGenericType)
            {
                
                if (typeof(IEnumerable).IsAssignableFrom(T))
                {

                    int i = 0;
                    foreach (object item in (IEnumerable)obj)
                    {
                        if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                        {
                            Serialize(item, regkey);
                        }
                        else
                        {
                            Serialize(item, regkey.CreateSubKey(i.ToString()));
                        }
                        i += 1;
                    }
                    
                }
                else if (T.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {

                    Type keytype = T.GetGenericArguments()[0];

                    if (keytype.IsPrimitive | keytype.IsClass & keytype == typeof(string))
                    {
                        dynamic keypi = T.GetProperty("Key");
                        dynamic valuepi = T.GetProperty("Value");
                        dynamic keyobj = keypi.GetValue(obj, null);
                        dynamic valueobj = valuepi.GetValue(obj, null);
                        Serialize(valueobj, regkey.CreateSubKey(keyobj.ToString()));
                    }

                }
                
            }
            else if (T.IsClass)
            {

                foreach (MethodInfo meth in T.GetMethods())
                {
                    if (meth.GetCustomAttribute<RegistrySerializerBeforeSerializeAttribute>() != null && meth.GetCustomAttribute<RegistrySerializerBeforeSerializeAttribute>().BeforeSerialize)
                    {
                        meth.Invoke(obj, null);
                    }
                }

                foreach (PropertyInfo prop in T.GetProperties())
                {

                    if (!prop.CanRead) continue;

                    string keyname = prop.Name;

                    if (prop.GetCustomAttribute<RegistrySerializerIgnorableAttribute>() != null && prop.GetCustomAttribute<RegistrySerializerIgnorableAttribute>().Ignorable)
                    {
                        continue;
                    }

                    if (prop.GetCustomAttribute<RegistrySerializerAliasAttribute>() != null && !string.IsNullOrEmpty(prop.GetCustomAttribute<RegistrySerializerAliasAttribute>().Alias))
                    {
                        keyname = prop.GetCustomAttribute<RegistrySerializerAliasAttribute>().Alias;
                    }

                    RegistryKey childkey = regkey.CreateSubKey(keyname);
                    object value = prop.GetValue(obj);

                    Serialize(value, childkey);

                }

                foreach (MethodInfo meth in T.GetMethods())
                {
                    if (meth.GetCustomAttribute<RegistrySerializerAfterSerializeAttribute>() != null && meth.GetCustomAttribute<RegistrySerializerAfterSerializeAttribute>().AfterSerialize)
                    {
                        meth.Invoke(obj, null);
                    }
                }

            }
            else
            {

                Console.WriteLine(string.Format("Unsupported type: {0} - {1}", T.Name, obj.ToString()));

            }

        }

        public static object Deserialize(Type T, RegistryKey regkey)
        {
            if (regkey == null) { return null; }

            if (T == typeof(bool))
            {

                return Convert.ToBoolean(regkey.GetValue(null));

            }
            else if (T == typeof(DateTime))
            {

                return DateTime.FromFileTime(long.Parse(regkey.GetValue(null).ToString()));

            }
            else if (T.IsPrimitive | T.IsClass & T == typeof(string))
            {

                return regkey.GetValue(null);
                
            }
            else if (T.IsArray)
            {

                dynamic ET = T.GetElementType();
                dynamic arr = Activator.CreateInstance(typeof(List<>).MakeGenericType(T.GetElementType()));

                for (int i = 0; i < regkey.GetSubKeyNames().Length; i++)
                {
                    arr.Add(Deserialize(ET, regkey.OpenSubKey(i.ToString())));
                }
                return arr.ToArray();
                
            }
            else if (T.IsGenericType)
            {

                if (T.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {

                    dynamic VT = T.GetGenericArguments()[1];
                    dynamic items = Activator.CreateInstance(T);

                    foreach (string key in regkey.GetSubKeyNames())
                    {
                        items.Add(key, Deserialize(VT, regkey.OpenSubKey(key)));
                    }
                    return items;
                                        
                }
                else if (typeof(IEnumerable).IsAssignableFrom(T))
                {

                    dynamic ET = T.GetGenericArguments()[0];
                    dynamic items = Activator.CreateInstance(T);

                    for (int i = 0; i < regkey.GetSubKeyNames().Length; i++)
                    {
                        items.Add(Deserialize(ET, regkey.OpenSubKey(i.ToString())));
                    }
                    return items;

                }
                
            }
            else if (T.IsClass)
            {

                dynamic cls = Activator.CreateInstance(T);

                foreach (PropertyInfo prop in T.GetProperties())
                {
                    if (!prop.CanWrite) continue;

                    string keyname = prop.Name;

                    if (prop.GetCustomAttribute<RegistrySerializerIgnorableAttribute>() != null && prop.GetCustomAttribute<RegistrySerializerIgnorableAttribute>().Ignorable) { 
                        continue;
                    }

                    if (prop.GetCustomAttribute<RegistrySerializerAliasAttribute>() != null && !string.IsNullOrEmpty(prop.GetCustomAttribute<RegistrySerializerAliasAttribute>().Alias))
                    {
                        keyname = prop.GetCustomAttribute<RegistrySerializerAliasAttribute>().Alias;
                    }

                    RegistryKey childkey = regkey.OpenSubKey(keyname);
                    prop.SetValue(cls, Deserialize(prop.PropertyType, childkey));
                }

                foreach (MethodInfo meth in T.GetMethods())
                {
                    if (meth.GetCustomAttribute<RegistrySerializerAfterDeserializeAttribute>() != null && meth.GetCustomAttribute<RegistrySerializerAfterDeserializeAttribute>().AfterDeserialize)
                    {
                        meth.Invoke(cls, null);
                    }
                }

                return cls;

            }
            else
            {

                Console.WriteLine(string.Format("Unsupported type: {0}", T.Name));

            }

            return null;

        }

        private T TCast<T>(object input)
        {
            return (T)input;
        }

        private T TConvert<T>(object input)
        {
            return (T)Convert.ChangeType(input, typeof(T));
        }

    }

    #region "Assembly Attributes"

    [AttributeUsage(AttributeTargets.Property)]
    public class RegistrySerializerIgnorableAttribute : Attribute
    {
        private bool _ignorable;
        public RegistrySerializerIgnorableAttribute(bool Ignorable)
        {
            _ignorable = Ignorable;
        }

        public bool Ignorable
        {
            get { return _ignorable; }
            set { _ignorable = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RegistrySerializerAliasAttribute : Attribute
    {
        private string _alias;
        public RegistrySerializerAliasAttribute(string Alias)
        {
            _alias = Alias;
        }

        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class RegistrySerializerBeforeSerializeAttribute : Attribute
    {
        private bool _beforeserialize;
        public RegistrySerializerBeforeSerializeAttribute(bool BeforeSerialize)
        {
            _beforeserialize = BeforeSerialize;
        }

        public bool BeforeSerialize
        {
            get { return _beforeserialize; }
            set { _beforeserialize = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RegistrySerializerAfterSerializeAttribute : Attribute
    {
        private bool _afterserialize;
        public RegistrySerializerAfterSerializeAttribute(bool AfterSerialize)
        {
            _afterserialize = AfterSerialize;
        }

        public bool AfterSerialize
        {
            get { return _afterserialize; }
            set { _afterserialize = value; }
        }
    }
    

    [AttributeUsage(AttributeTargets.Method)]
    public class RegistrySerializerAfterDeserializeAttribute : Attribute
    {
        private bool _afterdeserialize;
        public RegistrySerializerAfterDeserializeAttribute(bool AfterDeserialize)
        {
            _afterdeserialize = AfterDeserialize;
        }

        public bool AfterDeserialize
        {
            get { return _afterdeserialize; }
            set { _afterdeserialize = value; }
        }
    }
    #endregion
}
