using System;
using System.Collections.Generic;
using IRegisty;

namespace IRegistryTest
{
    public class clsSample
    {

        private string _SamplePropertyString;
        private int _SamplePropertyInteger;
        private DateTime _SamplePropertyDate;
        private bool _SamplePropertyBoolean;
        private string[] _SamplePropertyArrayOfString;
        private List<string> _SamplePropertyListOfString = new List<string>();
        private Dictionary<string, int> _SamplePropertyDictionaryOfInteger = new Dictionary<string, int>();

        public clsSample()
        {
        }

        public clsSample(string SamplePropertyString, int SamplePropertyInteger, DateTime SamplePropertyDate, bool SamplePropertyBoolean, string[] SamplePropertyArrayOfString, List<string> SamplePropertyListOfString, Dictionary<string, int> SamplePropertyDictionaryOfInteger)
        {
            _SamplePropertyString = SamplePropertyString;
            _SamplePropertyInteger = SamplePropertyInteger;
            _SamplePropertyDate = SamplePropertyDate;
            _SamplePropertyBoolean = SamplePropertyBoolean;
            _SamplePropertyArrayOfString = SamplePropertyArrayOfString;
            _SamplePropertyListOfString = SamplePropertyListOfString;
            _SamplePropertyDictionaryOfInteger = SamplePropertyDictionaryOfInteger;
        }

        [RegistrySerializerAlias("SamplePropertyStringAlias")]
        public string SamplePropertyString
        {
            get { return _SamplePropertyString; }
            set { _SamplePropertyString = value; }
        }

        [RegistrySerializerIgnorable(false)]
        public int SamplePropertyInteger
        {
            get { return _SamplePropertyInteger; }
            set { _SamplePropertyInteger = value; }
        }

        public DateTime SamplePropertyDate
        {
            get { return _SamplePropertyDate; }
            set { _SamplePropertyDate = value; }
        }

        public bool SamplePropertyBoolean
        {
            get { return _SamplePropertyBoolean; }
            set { _SamplePropertyBoolean = value; }
        }

        [RegistrySerializerAlias("SamplePropertyArrayOfStringAlias")]
        public string[] SamplePropertyArrayOfString
        {
            get { return _SamplePropertyArrayOfString; }
            set { _SamplePropertyArrayOfString = value; }
        }

        [RegistrySerializerAlias("SamplePropertyListOfStringAlias")]
        public List<string> SamplePropertyListOfString
        {
            get { return _SamplePropertyListOfString; }
            set { _SamplePropertyListOfString = value; }
        }

        public Dictionary<string, int> SamplePropertyDictionaryOfInteger
        {
            get { return _SamplePropertyDictionaryOfInteger; }
            set { _SamplePropertyDictionaryOfInteger = value; }
        }

        [RegistrySerializerBeforeSerialize(true)]
        public void MyMethod ()
        {
            Console.Write("Method invoked before serialize ... ");
        }
    }
}
