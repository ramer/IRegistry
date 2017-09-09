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
