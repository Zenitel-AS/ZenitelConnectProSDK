# Getting Started

## **System Requirements**
Before setting up the **Zenitel Connect Pro SDK**, ensure that your development environment meets the following requirements:

- **Operating System**: Windows 10/11 (64-bit)
- **.NET Framework**: 4.8 or higher
- **.NET Standard**: 2.1 compatibility for cross-platform support
- **Development Tools**: Visual Studio 2019 or later (recommended)
- **Dependencies**: NuGet Package Manager

> **Note:** The SDK includes all required dependencies, including **WampSharp**, so no additional package installations are necessary.

---

## **Installation Instructions**
You can install the **Zenitel Connect Pro SDK** using one of the following methods:

### **1. Install via NuGet (Recommended)**
Run the following command in the **NuGet Package Manager Console**:

```powershell
Install-Package Zenitel.ConnectPro.SDK
```

### **2. Manual Installation**
1. Clone the repository or download the package.
2. Locate the compiled **Zenitel.ConnectPro.SDK.dll** files in the `bin` folder.
3. Reference the **Zenitel.ConnectPro.SDK.dll** in your project.

---

## **Build Instructions**
To build the SDK from source:

1. Clone the repository:
   ```sh
   git clone https://github.com/Zenitel/ConnectProSDK.git
   cd ConnectProSDK
   ```
2. Open the solution file (`Zenitel.ConnectPro.sln`) in **Visual Studio**.
3. Ensure the **target framework** is set to **.NET Framework 4.8** or **.NET Standard 2.1**.
4. Build the solution (`Ctrl + Shift + B`).

---

## **Core Initialization**
Once installed, initialize and configure the **Zenitel Connect Pro SDK** using a **singleton pattern** to maintain the state of the `Core` object:

```csharp
// Recommended: Use Core as a Singleton
public class CoreInstance
{
    private static readonly Lazy<ConnectPro.Core> _coreInstance = new(() =>
    {
        var core = new ConnectPro.Core();
        core.Start(); // Core must be started once
        return core;
    });

    public static ConnectPro.Core Instance => _coreInstance.Value;
}
```

---

## **Establishing a Connection**
After ensuring the `Core` instance is initialized, you can set the connection credentials **once** and establish a connection.

```csharp
// Access the Core instance
var core = CoreInstance.Instance;

// Set connection credentials only once
if (!core.ConnectionHandler.IsConnected)
{
    core.Configuration.ServerAddr = "169.254.1.5";
    core.Configuration.Port = "8087";
    core.Configuration.UserName = "admin";
    core.Configuration.Password = "password";
    core.ConnectionHandler.OpenConnection();
}
```

✅ **The Core instance persists throughout the application lifecycle.**  
✅ **Credentials are only set once, and `OpenConnection()` is only called if not already connected.**  

---

## **Important Notes**
- **Core must be a singleton** to prevent re-creation and maintain system state.
- **Only call `OpenConnection()` when needed** to avoid redundant reconnections.
- **Ensure `Core.Start()` is only called once**, during initialization.

---

## **Next Steps**
Once the SDK is installed and configured, proceed to the **Usage Instructions** section to learn how to interact with devices, manage calls, and handle events.

🔗 [Continue to Usage Instructions](usage-instructions.md)

---