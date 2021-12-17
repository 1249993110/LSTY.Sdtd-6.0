using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;

namespace LSTY.Sdtd.Shared
{
    public static class ConfigurationLoader
    {
        public static T GetCallingAssemblySettings<T>(char splitSeparator = ';') where T : class, new ()
        {
            try
            {
                T t = new T();

                var settings = ConfigurationManager.OpenExeConfiguration(Assembly.GetCallingAssembly().Location).AppSettings?.Settings;

                if(settings != null)
                {
                    foreach (var property in typeof(T).GetProperties())
                    {
                        string value = settings[property.Name].Value;

                        if (property.PropertyType.IsArray)
                        {
                            Type elementType = property.PropertyType.GetElementType();
                            List<object> list = new List<object>();
                            foreach (var item in value.Split(splitSeparator))
                            {
                                list.Add(Convert.ChangeType(item, elementType));
                            }

                            var array = Array.CreateInstance(elementType, list.Count);
                            for (int i = 0; i < list.Count; ++i)
                            {
                                array.SetValue(list[i], i);
                            }

                            property.SetValue(t, array);
                        }
                        else
                        {
                            property.SetValue(t, Convert.ChangeType(value, property.PropertyType));
                        }
                    }
                }

                return t;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ConfigurationLoader.GetExecutingAssemblySettings", ex);
            }
        }
    }
}
