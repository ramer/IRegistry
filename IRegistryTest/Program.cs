using System;
using System.Collections.Generic;
using IRegisty;
using Microsoft.Win32;

namespace IRegistryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(@"Software\RegistrySerializer\Sample");

            clsSample objecttoserialize = new clsSample("Hello World!", 12345, DateTime.Now, true,
                new string[] {"mno", "pqr", "stu", "vwx"},
                new List<string> {"abc", "def", "ghi", "jkl"},
                new Dictionary<string, int> {  {"one", 1}, {"two", 2}, {"three", 3}});

            Console.Write("Serializing object ... ");

            IRegistrySerializer.Serialize(objecttoserialize, reg);

            Console.WriteLine("done");

            Console.Write("Deserializing object ... ");

            clsSample deserializedobject = (clsSample)IRegistrySerializer.Deserialize(typeof(clsSample), reg);

            Console.WriteLine("done");

            Registry.CurrentUser.DeleteSubKeyTree(@"Software\RegistrySerializer");

            Console.ReadLine();
        }
    }
}
