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
            
            if (T.IsPrimitive | T.IsClass & T == typeof(string))
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
                foreach (PropertyInfo prop in T.GetProperties())
                {

                    if (!prop.CanRead) continue;

                    string keyname = prop.Name;

                    RegistrySerializerAttribute attr = prop.GetCustomAttribute<RegistrySerializerAttribute>();
                    if (attr != null)
                    {
                        if (attr.Ignorable)
                            continue;
                        if (!string.IsNullOrEmpty(attr.Name))
                            keyname = attr.Name;
                    }

                    RegistryKey childkey = regkey.CreateSubKey(keyname);
                    object value = prop.GetValue(obj);

                    Serialize(value, childkey);

                }
                
            }
            else
            {

                Console.WriteLine(string.Format("Unsupported type: {0} - {1}", T.Name, obj.ToString()));

            }

        }

        public static object Deserialize(Type T, RegistryKey regkey)
        {
            
            if (T.IsPrimitive | T.IsClass & T == typeof(string))
            {

                return regkey.GetValue(null);
                
            }
            else if (T == typeof(DateTime))
            {

                return DateTime.FromFileTime(long.Parse(regkey.GetValue(null).ToString()));
                
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
                    if (!prop.CanWrite)
                        continue;

                    string keyname = prop.Name;

                    RegistrySerializerAttribute attr = prop.GetCustomAttribute<RegistrySerializerAttribute>();
                    if (attr != null)
                    {
                        if (attr.Ignorable)
                            continue;
                        if (!string.IsNullOrEmpty(attr.Name))
                            keyname = attr.Name;
                    }

                    RegistryKey childkey = regkey.OpenSubKey(keyname);
                    prop.SetValue(cls, Deserialize(prop.PropertyType, childkey));
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
    public class RegistrySerializerAttribute : Attribute
    {

        private bool _ignorable;

        private string _name;
        public RegistrySerializerAttribute(bool Ignorable, string Name)
        {
            _ignorable = Ignorable;
            _name = Name;
        }

        public RegistrySerializerAttribute(bool Ignorable)
        {
            _ignorable = Ignorable;
        }

        public bool Ignorable
        {
            get { return _ignorable; }
            set { _ignorable = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

    }

    #endregion
}
