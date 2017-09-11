# IRegistry
Adds RegistrySerializer to your project.
The Class library provides functions that allow you to store primitives, arrays, collections, and classes (with reflection) in the system registry.

Repo contains two projects:
* **IRegistry** - Class library.
* **IRegistryTest** - C# Sample Project, examine IRegistry class library usage.

### IRegistry class library usage:
#### C#

```C#
using IRegisty;

// define your application registry path
RegistryKey reg = Registry.CurrentUser.CreateSubKey(@"Software\IRegistryTest);

// store your data to custom class instance (for example)
clsSample objecttoserialize = new clsSample(...);

// save your data in system registry
IRegistrySerializer.Serialize(objecttoserialize, reg);

// load your data back to object
clsSample deserializedobject = (clsSample)IRegistrySerializer.Deserialize(typeof(clsSample), reg);
```

### Parameters:

* **obj** - The object to store in registry
* **regkey** - The RegistryKey, which determine registry path

### Attributes:

* **RegistrySerializerIgnorable(bool Ignorable)** - The property attribute allow you to ignore your custom class property
* **RegistrySerializerAlias(string Alias)** - The property attribute allow you to rename your custom class property

* **RegistrySerializerBeforeSerialize(bool BeforeSerialize)** - The method attribute allow you to invoke method before serialize
* **RegistrySerializerAfterSerialize(bool AfterSerialize)** - The method attribute allow you to invoke method after serialize
* **RegistrySerializerAfterDeserialize(bool AfterDeserialize)** - The method attribute allow you to invoke method after deserialize

### Attributes usage:
#### C#

```C#

public class clsSample
{
    [RegistrySerializerAlias("SamplePropertyStringAlias")]
    public string SamplePropertyString
    {
        get { return _SamplePropertyString; }
        set { _SamplePropertyString = value; }
    }
    
    [RegistrySerializerBeforeSerialize(true)]
    public void MethodBeforeSerialize ()
    {
        Console.Write("Method invoked before serialize ... ");
    }
}
```
