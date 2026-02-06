### Zenitel Connect Pro SDK

---

### Table of Contents
1. [Description](#description)
2. [Getting Started](#getting-started)
3. [Core Components](#core-components)
4. [Usage Instructions](#usage-instructions)
    - [Event Handlers](#event-handlers)
    - [Collections](#collections)
    - [Models](#models)
    - [Handlers](#handlers)
5. [Authors and Acknowledgment](#authors-and-acknowledgment)
6. [License](#license)

---

### 1. Description
The **Zenitel Connect Pro SDK** is a comprehensive toolset for integrating Zenitel's communication systems, including intercoms, public address systems, and critical communication solutions, into .NET-based applications. It supports both legacy systems and modern frameworks, offering flexibility, extensibility, and powerful integration capabilities.

---

### 2. Core Components

The **Zenitel Connect Pro SDK** is a comprehensive library built for **.NET Framework 4.8** and **netstandard 2.1**, ensuring compatibility with legacy and modern systems. It is designed as an **SDK-style library** to support robust integration with third-party applications.

---

#### **SharedComponents**

The **SharedComponents** library is the foundation of the SDK, offering a rich set of tools, utilities, functionalities, and models that are used across all projects. This shared codebase simplifies development by providing reusable components and a consistent structure, making integration and maintenance efficient.

**Key Features**:

1. **Debug Utilities**:  
   - Provide centralized logging and monitoring capabilities to ensure application behavior is tracked effectively. Utilities like `Debug.Log` and `SystemMonitor` assist in error tracking, system health monitoring, and performance logging.

2. **Collections**:  
   - Offer a robust framework for managing system entities, including devices, calls, groups, and directory numbers, ensuring seamless data organization and retrieval.

3. **Events**:  
   - Enable a powerful event-driven programming model that handles notifications, state changes, and UI updates. This simplifies interactions between various SDK components.

4. **Enums**:  
   - Define constants and system states, ensuring consistency and reducing hard-coded values across the SDK.

5. **Interfaces**:  
   - Provide abstractions such as `ICamera` to ensure modularity and extensibility within the system.

6. **Models**:  
   - Represent the core entities of the system, including configurations, devices, calls, and audio events. These models offer a structured approach to data handling, allowing developers to interact with system objects efficiently. Each model is designed to reflect real-world entities with properties and behaviors essential for SDK operations.

7. **Tools**:  
   - Include various utilities that enhance system reliability and developer productivity:
     - `AssemblyChecker`: Verifies compatibility between assemblies.
     - `Cryptography`: Provides secure data handling with encryption tools.
     - `ErrorLogger`: Offers centralized exception logging with detailed error metadata.
     - `IconManager`: Handles application icons for consistent UI presentation.
     - `ObjectConverter`: Simplifies serialization and data conversion.
     - `ProcessManager`: Manages system-level processes and optimizes resource usage.

---

#### **WampClient**

The **WampClient** is a critical communication layer built using the **WampSharp** library. It is fully integrated into the SDK to enable real-time messaging and communication between system components.

**Key Features**:

- **Forked and Optimized for Zenitel Connect Pro SDK**:
  - Custom modifications to suit the SDK's requirements.
  - Provides enhanced stability and performance for WAMP-based communication.

- **Target Frameworks**:
  - Designed for both **.NET Framework 4.8** and **netstandard 2.1**, ensuring seamless integration with diverse applications.

- **SharedComponents Integration**:
  - Works seamlessly with the shared library to handle:
    - Device communication.
    - Call events and synchronization.
    - Audio analytics and event processing.
    - Broadcasting and access control.

---

#### **Integration Modules**

The **Integration Modules** are backend libraries. These modules implement the shared code and WampClient to provide the following features:

1. **Access Control**:
   - Securely manage access to restricted areas, including unlocking doors and validating permissions.

2. **Audio Analytics**:
   - Integrates with third-party audio detection systems to process audio events like gunshots or glass breaking.

3. **Device Management**:
   - Handles device registration, state updates, and interactions.

4. **Call Management**:
   - Manages active and queued calls, including prioritization and operator interactions.

5. **Broadcasting**:
   - Enables broadcasting of audio messages and announcements to groups or devices.

6. **GPIO Management**:
   - Monitor GPI (input) and GPO (output) states on devices in real time.
   - Control GPO outputs (e.g., activate/deactivate relays) via the `Device.Gpio` model.
   - Receive real-time GPIO state change events through `DeviceGpio.Changed` or `CoreHandler.Core.Events.OnGpioEvent`.

---

### **Key Notes**

- **SDK Architecture**:
  The entire project is structured as an SDK-style library, making it flexible and extensible for third-party integrations.

- **Target Frameworks**:
  All components of the SDK, including **SharedComponents**, **WampClient**, and **Integration Modules**, are developed for:
  - **.NET Framework 4.8**: For compatibility with enterprise and legacy systems.
  - **netstandard 2.1**: For cross-platform support and modern .NET applications.

- **Comprehensive Shared Code**:
  The shared library (`SharedComponents`) includes:
  - Core utilities (e.g., debugging, logging, cryptography).
  - Interfaces for modular development.
  - Tools for process management, data conversion, and more.

---

Here is the **updated "Getting Started"** section with the recommended improvements:

---

### **3. Getting Started**

#### **System Requirements**
Before setting up the **Zenitel Connect Pro SDK**, ensure that your development environment meets the following requirements:

- **Operating System**: Windows 10/11 (64-bit)
- **.NET Framework**: 4.8 or higher
- **.NET Standard**: 2.1 compatibility for cross-platform support
- **Development Tools**: Visual Studio 2019 or later (recommended)

> 🛠 **Note:** The SDK includes all required dependencies, including **WampSharp**, so no additional package installations are necessary.

---

#### **Installation Instructions**
You can install the **Zenitel Connect Pro SDK** using one of the following methods:

##### **1. Install via NuGet (Recommended)**
Run the following command in the **NuGet Package Manager Console**:
```powershell
Install-Package Zenitel.ConnectPro.SDK
```

##### **2. Manual Installation**
1. Clone the repository or download the package.
2. Locate the compiled **Zenitel.ConnectPro.SDK.dll** files in the `bin` folder.
3. Reference the **Zenitel.ConnectPro.SDK.dll** in your project.

---

#### **Build Instructions**
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

#### **Basic Configuration**
Once installed, initialize and configure the **Zenitel Connect Pro SDK** as follows:

```csharp
// Recommended: Use Core as a Singleton
public static class CoreInstance
{
    private static readonly Lazy<ConnectPro.Core> _coreInstance = new(() =>
    {
        var core = new ConnectPro.Core();
        core.Configuration.ServerAddr = "169.254.1.5";
        core.Configuration.Port = "8087";
        core.Configuration.UserName = "admin";
        core.Configuration.Password = "password";
        core.Start();
        core.ConnectionHandler?.OpenConnection();
        return core;
    });

    public static ConnectPro.Core Instance => _coreInstance.Value;
}

// Access the Core instance
var core = CoreInstance.Instance;
```

🔹 **Ensure the credentials are correct** before calling `core.Start()`.  
🔹 **Replace default values** (`169.254.1.5`, `8087`, `admin`, `password`) with actual server credentials.  

---

### **⚠️ Important Note on Core Usage**
- It is **strongly recommended** to use the **Core as a singleton** to maintain a **consistent state** throughout the application.
- **One core instance should be used per Zenitel Connect Pro connection.**
- Avoid creating multiple instances, as this can lead to **unexpected behavior and connection issues**.

---

### **3. Usage Instructions**  

Before diving into the SDK usage, it's essential to understand the **Core object**, which serves as the **central static entry point** for accessing all major functionalities.

---

### **1. The Core Object – The Static Entry Point**  

The SDK is designed around a **static Core object** (`CoreHandler.Core`), which acts as a **singleton-style entry point** for accessing all major components, including **handlers, collections, events, and configurations**. This eliminates the need for manually instantiating multiple service objects.

#### **Core Components of `CoreHandler.Core`**:
  - `CoreHandler.Core.Configuration`: Holds global SDK settings.
  - `CoreHandler.Core.ConnectionHandler`: Manages WAMP connections.
  - `CoreHandler.Core.Events`: Handles all SDK-wide events.
  - `CoreHandler.Core.CallHandler`: Manages active and queued calls.
  - `CoreHandler.Core.DeviceHandler`: Handles device registration and retrieval.
  - `CoreHandler.Core.AccessControlHandler`: Manages access control operations.
  - `CoreHandler.Core.BroadcastingHandler`: Controls audio messaging and group broadcasts.
  - `CoreHandler.Core.Collection`: Provides access to dynamically updated data structures.
  - `CoreHandler.Core.Log`: Centralized logging and debugging.

Since `CoreHandler.Core` is a **static reference**, all SDK interactions rely on it, ensuring **global availability** throughout the application.

#### **Example Usage of CoreHandler.Core**
```csharp
// Accessing the active call list
var activeCalls = CoreHandler.Core.Collection.ActiveCalls;

// Posting a new call
CoreHandler.Core.CallHandler.PostCall("1001", "1002", "setup", true);

// Opening a door through Access Control
CoreHandler.Core.AccessControlHandler.OpenDoor(selectedDevice);

// Checking Connection Status
bool isConnected = CoreHandler.Core.ConnectionHandler.IsConnected;
```

This **static architecture** ensures **centralized control**, reducing the need for multiple object instances and providing **optimized performance**.

---

### **2. Operator Directory Number Must Be Set Before Performing Any Actions**

After the connection is **established** and **devices are retrieved**, the **operator's directory number must be set before any action** (such as **placing calls, sending group messages, or using access control**).  
Failure to set this value will **prevent call initiation** and **other critical operations**.

#### **Setting the Operator's Directory Number**
```csharp
CoreHandler.Core.Configuration.OperatorDirNo = "1001";
```

✅ **Set it only after the connection is established and devices are available**.

🚨 **Do not attempt to place calls or send messages before setting this value**.

---

### **3. Dynamic Data Handling (Collections Are Auto-Updated)**  

The SDK follows an **event-driven architecture**, meaning **all collections are updated automatically** when changes occur in **Zenitel Connect Pro**.  

#### **Device Collection (Auto-Populated)**
- When a connection is **established**, **all registered devices** are automatically retrieved and **added to the collection**.
- **No work is required** from the developer to fetch or refresh devices.
- If a device is **added, removed, renamed, or moved**, the collection updates automatically.

```csharp
// Retrieve devices (already auto-populated)
var devices = CoreHandler.Core.Collection.RegisteredDevices;
```

#### **Applies to All Collections**
The same **auto-update principle** applies to:
- **Active Calls**
- **Queued Calls**
- **Groups**
- **Audio Messages**
- **Directory Numbers**  

Whenever changes occur **inside Zenitel Connect Pro**, the SDK **syncs the updates automatically**.

---

### **4. Exception Handling with `OnExceptionThrown` (Central Logging)**  

The SDK provides a **centralized exception handling mechanism** using `OnExceptionThrown`.  
- Developers **do not need to manually handle exceptions** inside handlers or event listeners.  
- Any unexpected errors **automatically trigger** `OnExceptionThrown`, allowing for **custom log handling**.

#### **Example: Implementing a Custom Exception Logger**
```csharp
CoreHandler.Core.Events.OnExceptionThrown += (sender, ex) =>
{
    Debug.Log($"Error: {ex.Message}");
    MessageBox.Show("An error occurred: " + ex.Message);
};
```

This approach ensures **centralized exception logging** without modifying individual components.

---

### **5. Core Functionalities & How to Use Them**  

The SDK provides modular components that allow developers to interact with **devices, calls, audio analytics, access control, and event-driven programming**.

---

### **A. Device Management**  

#### **Retrieve Registered Devices**
```csharp
// Devices are automatically populated when connected to Zenitel Connect Pro.
var devices = CoreHandler.Core.Collection.RegisteredDevices;
```

#### **Call a Selected Device**
```csharp
Task.Run(async () =>
    await CoreHandler.Core.CallHandler.PostCall(
        CoreHandler.Core.Configuration.OperatorDirNo,
        selectedDevice.dirno,
        "setup"
    )
);
```

#### **Perform a Tone Test on a Device**
```csharp
CoreHandler.Core.DeviceHandler.InitiateToneTest(selectedDevice.dirno, "3");
```

---

### **B. Call Handling**  

#### **Initiate a Call**
```csharp
CoreHandler.Core.CallHandler.PostCall("1001", "1002", "setup", true);
```

#### **Listen for Active Call Changes**
```csharp
CoreHandler.Core.Events.OnActiveCallListValueChange += HandleActiveCallListChange;
```

#### **End an Active Call**
```csharp
CoreHandler.Core.CallHandler.DeleteCall(device.dirno);
```

#### **Answer a Queued Call**
```csharp
Task.Run(async () => await CoreHandler.Core.CallHandler.AnswerQueuedCall(queuedCall));
```

---

### **C. Audio Analytics**  

#### **Process Audio Events**
```csharp
CoreHandler.Core.Events.AudioAnalytics.AudioEventDetected += HandleAudioEventDetection;
```

```csharp
public void HandleAudioEventDetection(object sender, AudioEventDetected audioEvent)
{
    Console.WriteLine($"Audio event detected: {audioEvent.EventType} at {audioEvent.Time}");
}
```

---

### **D. Broadcasting Messages**  

#### **Retrieve Groups and Audio Messages**
```csharp
Task.Run(async () =>
{
    await CoreHandler.Core.BroadcastingHandler.RetrieveGroups();
    await CoreHandler.Core.BroadcastingHandler.RetrieveAudioMessages();
});
```

#### **Play an Audio Message**
```csharp
CoreHandler.Core.BroadcastingHandler.PlayAudioMessage(audioMessage, targetGroup);
```

#### **Stop an Audio Message**
```csharp
CoreHandler.Core.BroadcastingHandler.StopAudioMessage(audioMessage);
```

---

### **E. Access Control Management**  

#### **Open a Door for a Device**
```csharp
CoreHandler.Core.AccessControlHandler.OpenDoor(selectedDevice);
```

---

### **F. GPIO (General Purpose Input/Output) Management**

The SDK provides full support for monitoring and controlling **GPIO** (General Purpose Input/Output) lines on Zenitel devices. This includes both **GPI** (inputs, e.g., door sensors, buttons) and **GPO** (outputs, e.g., relays, LEDs).

GPIO state is managed per-device through the `Device.Gpio` property, which exposes a `DeviceGpio` runtime model. This model:
- Automatically subscribes to real-time GPI/GPO change events via WAMP.
- Loads an initial snapshot of all GPIO points when the device is retrieved.
- Provides methods to activate or deactivate GPO outputs.

#### **Accessing GPIO State for a Device**

```csharp
var device = CoreHandler.Core.Collection.RegisteredDevices
    .FirstOrDefault(d => d.dirno == "1001");

if (device?.Gpio != null)
{
    // Read all GPI (input) points
    foreach (var input in device.Gpio.Inputs)
    {
        Console.WriteLine($"GPI {input.Id}: {input.State}");
    }

    // Read all GPO (output) points
    foreach (var output in device.Gpio.Outputs)
    {
        Console.WriteLine($"GPO {output.Id}: {output.State}");
    }
}
```

#### **Listening for GPIO Changes**

##### **Per-Device: `DeviceGpio.Changed` Event**
```csharp
device.Gpio.Changed += (sender, args) =>
{
    Console.WriteLine($"Device {args.Dirno} - GPIO {args.Point.Id} " +
                      $"({args.Point.Direction}): {args.Point.State}");
};
```

##### **Global: `CoreHandler.Core.Events.OnGpioEvent`**
```csharp
CoreHandler.Core.Events.OnGpioEvent += (sender, gpioEventArgs) =>
{
    Console.WriteLine($"GPIO event on device {gpioEventArgs.Dirno}: " +
                      $"{gpioEventArgs.Element.id} = {gpioEventArgs.Element.state}");
};
```

#### **Controlling GPO Outputs**
```csharp
// Activate a relay indefinitely
await device.Gpio.ActivateAsync("relay1", timeSeconds: null, CancellationToken.None);

// Activate a relay for 5 seconds
await device.Gpio.ActivateAsync("relay1", timeSeconds: 5, CancellationToken.None);

// Deactivate a relay
await device.Gpio.DeactivateAsync("relay1", CancellationToken.None);
```

#### **Refreshing GPIO State**
```csharp
await device.Gpio.RefreshAsync(CancellationToken.None);
```

---

### **G. Event Handling in the SDK**

The SDK provides a robust **event-driven architecture**, allowing UI and logic components to react dynamically.

#### **Listening to Device List Changes**
```csharp
CoreHandler.Core.Events.OnDeviceListChange += HandleDeviceListChange;
```

#### **Listening to Operator Directory Number Changes**
```csharp
CoreHandler.Core.Events.OnOperatorDirNoChange += HandleOperatorDirnoChange;
```

#### **Listening to Connection Changes**
```csharp
CoreHandler.Core.Events.OnConnectionChanged += HandleConnectionChange_Client;
```

---

### **6. Summary**
The **Zenitel Connect Pro SDK** provides a **modular, event-driven, and centralized framework** for managing:
- **Devices** (Automatically Populated)
- **Calls (Active & Queued)**
- **Audio analytics**
- **Broadcasting**
- **Access control**
- **GPIO (General Purpose I/O)** – monitor inputs, control outputs, and receive real-time state changes
- **Real-time event handling**
- **Seamless UI integration**

### **Important Notes**:
✅ **Collections are auto-populated**; no manual fetching is required.  
✅ **Set `OperatorDirNo` after connection & device retrieval, before placing calls or sending messages**.  
✅ **Use `OnExceptionThrown` for centralized error handling**.  
✅ **GPIO state is auto-loaded per device**; use `Device.Gpio` to read inputs/outputs and subscribe to changes.

---

#### Event Handlers
The SDK provides robust event handling to track changes and updates in real time. Below are the key event handlers available:

| Event Name                      | Description                                                                 |
|----------------------------------|-----------------------------------------------------------------------------|
| `OnActiveVideoFeedChange`       | Fires when a change in the active camera feed is detected.                  |
| `OnOperatorDirNoChange`         | Occurs when the operator directory number is updated.                       |
| `OnDeviceListChange`            | Fires when the list of devices is updated.                                  |
| `OnDeviceStateChange`           | Notifies changes in the state of a specific device.                         |
| `OnActiveCallListValueChange`   | Fires when the value of the active call list is updated.                    |
| `OnCallQueueListValueChange`    | Occurs when the queued call list undergoes modifications.                    |
| `OnDeviceRetrievalStart`        | Signals the start of the device retrieval process.                          |
| `OnDeviceRetrievalEnd`          | Signals the end of the device retrieval process.                            |
| `OnLogEntryRequested`           | Fires when a new log entry request is initiated but not yet saved.          |
| `OnLogEntryAdded`               | Triggered when a log entry is successfully added.                           |
| `OnDebugChanged`                | Fires when debugging information is modified or added.                      |
| `OnExceptionThrown`             | Used to handle exceptions and create error log entries.                     |
| `OnQueuesAndCallsSync`          | Signals the CallHandler to retrieve all active and queued calls.            |
| `OnConnectionChanged`           | Triggers on changes in connection status with Zenitel Connect.              |
| `OnConfigurationChanged`        | Fires when configuration data changes.                                      |
| `CallHandlerPopupRequested`     | Used to open or close a popup window.                                       |
| `OnManualVideoFeedChange`       | Fires when a manual switch of the camera feed is requested.                 |
| `OnGroupsListChange`            | Occurs when the list of groups changes.                                     |
| `OnGroupsMsgUpdate`             | Fires when a group broadcast update is received.                            |
| `OnDoorOpen`                    | Fires when a door open event is detected.                                   |
| `OnAudioMessagesChange`         | Fires when an audio message update occurs.                                  |
| `OnGpioEvent`                   | Fires when a GPIO (GPI/GPO) state change is received for any device.        |

##### Audio Analytics Events
The SDK also provides specialized event handlers for audio analytics:

| Event Name                   | Description                                                                  |
|------------------------------|------------------------------------------------------------------------------|
| `DataReceived`               | Fires when the Audio Event Detector receives data.                           |
| `Heartbeat`                  | Occurs when the Audio Event Detector sends a heartbeat signal every minute.  |
| `AudioEventDetected`         | Fires when an audio event is detected (e.g., gunshot, aggression, glass break). |

These event handlers allow developers to seamlessly integrate real-time monitoring, logging, and interaction functionalities within the system.

---

#### Collections
The SDK provides structured collections to manage various elements within Zenitel Connect. These collections help in organizing devices, active and queued calls, groups, and directory numbers efficiently.

| Collection Name         | Description                                                                 |
|------------------------|-----------------------------------------------------------------------------|
| `RegisteredDevices`    | Holds all Zenitel devices registered with Zenitel Connect.                  |
| `ActiveCalls`         | Contains the list of currently active calls.                                |
| `CallQueue`          | Stores calls that are currently queued, waiting to be answered or processed. |
| `Groups`             | Holds the groups defined within Zenitel Connect Pro for communication.        |
| `AudioMessages`      | Contains uploaded system notifications, alerts, or pre-recorded messages.    |
| `DirectoryNumbers`   | Stores directory numbers used for managing communication endpoints.          |

Each collection is lazily initialized to optimize memory usage, ensuring that objects are only created when accessed.
These collections facilitate quick retrieval and management of devices, calls, and messages, allowing seamless integration within the Zenitel Connect ecosystem.

---

---

#### Models
Here are the **tables for each model** with their properties and methods, ensuring nothing is skipped:

### **AudioEventDetected**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| FromDirno         | string     | Directory number of the device that detected the event. |
| FromName          | string     | Name of the device that detected the event.             |
| EventType         | string     | Type of detected audio event (e.g., gunshot, glass break). |
| Probability       | string     | Probability of the event as a string.                  |
| Time              | DateTime   | UTC time when the audio event was detected.            |

**Methods**: None

---

### **DataReceived**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| FromDirno         | string     | Directory number of the device that sent the data.      |
| FromName          | string     | Name of the device that sent the data.                  |
| Status            | string     | Status of the received audio data.                     |
| UTC_Time          | DateTime   | Timestamp of the data reception.                       |

**Methods**: None

---

### **Heartbeat**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| FromDirno         | string     | Directory number of the device that sent the heartbeat. |
| FromName          | string     | Name of the device that sent the heartbeat.             |
| UTC_Time          | DateTime   | Timestamp of the heartbeat signal.                     |

**Methods**: None

---

### **AudioMessage**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| Id                | string     | Unique identifier for the audio message.               |
| Dirno             | string     | Directory number associated with the message.          |
| FileName          | string     | Name of the audio file.                                |
| FilePath          | string     | Path to the stored file.                               |
| FileSize          | long       | Size of the file in bytes.                             |
| Duration          | int        | Duration of the audio message in seconds.              |
| IsPlaying         | bool       | Indicates if the message is currently playing.         |

**Methods**:

| **Method Name**                     | **Description**                                                                                 |
|-------------------------------------|-------------------------------------------------------------------------------------------------|
| `OnPropertyChanged(string propertyName)` | Invokes a property changed event when a property value is updated.                             |
| `SetValuesFromSDK(WampAudioMessageElement sdkAudioMessageElement)` | Updates the instance with values from the SDK message element.                                  |
| `NewDeviceFromSdkElement(WampAudioMessageElement sdkAudioMessageElement)` | Creates a new instance of `AudioMessage` from the SDK element.                                |

---

### **CallElement**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| CallId            | string     | Unique identifier for the call.                        |
| CallType          | string     | Type of call (e.g., normal, queued).                   |
| CallState         | string     | Current state of the call.                             |
| FromDirno         | string     | Directory number of the caller.                       |
| ToDirno           | string     | Directory number of the recipient.                    |
| Priority          | int        | Priority level of the call.                           |

**Methods**:

| **Method Name**                                  | **Description**                                                    |
|--------------------------------------------------|----------------------------------------------------------------------|
| `NewCallElementFromSdkCallElement(WampCallElement wampCallElement)` | Creates a new instance of `CallElement` from the WAMP call element. |

---

### **CallLegElement**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| CallId            | string     | Unique call identifier.                                |
| Dirno             | string     | Directory number of the associated call leg.           |
| Name              | string     | Name of the associated call leg.                       |
| Priority          | int        | Priority level of the call leg.                        |

**Methods**: None

---

### **CallLog**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| CallId            | string     | Unique identifier for the call.                        |
| StartTime         | DateTime   | Timestamp when the call started.                      |
| EndTime           | DateTime   | Timestamp when the call ended.                        |
| Participants      | List<string>| List of participants in the call.                    |

**Methods**: None

---

### **Camera**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| Id                | string     | Unique identifier for the camera.                     |
| FQID              | string     | Fully Qualified Identifier for the camera.            |
| Name              | string     | Name of the camera.                                   |

**Methods**: None

---

### **Configuration**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| ErrorLogFilePath  | string     | File path for error logging.                           |

**Methods**:

| **Method Name**                      | **Description**                                     |
|--------------------------------------|-----------------------------------------------------|
| `GetLogPath()`                       | Retrieves the path for the error log file.          |
| `GetDefaultConfiguration()`          | Creates and returns a default configuration.        |

---

### **Device**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| DeviceId          | string     | Unique identifier for the device.                     |
| NodeNumber        | int        | Node number assigned to the device.                   |
| DeviceState       | string     | Current state of the device.                          |

**Methods**: None

---

### **DeviceCamera**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| DeviceId          | string     | ID of the device.                                      |
| CameraId          | string     | ID of the associated camera.                          |

**Methods**: None

---

### **DeviceSettings**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| Volume            | int        | Volume level of the device.                            |
| Brightness        | int        | Brightness level.                                      |

**Methods**: None

---

### **DirectoryNumber**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| Dirno             | string     | Directory number.                                      |
| Name              | string     | Name associated with the directory number.            |

**Methods**: None

---

### **ExceptionLog**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| ExceptionMessage  | string     | Message describing the exception.                      |
| Timestamp         | DateTime   | When the exception occurred.                           |

**Methods**: None

---

### **Group**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| GroupId           | string     | Unique identifier for the group.                       |
| Name              | string     | Name of the group.                                     |

**Methods**: None

---

### **Operator**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| OperatorId        | string     | Unique identifier for the operator.                    |
| Name              | string     | Name of the operator.                                  |

**Methods**: None

---

### **PrerecordedMessage**

| **Property Name** | **Type**   | **Description**                                         |
|-------------------|------------|---------------------------------------------------------|
| MessageId         | string     | Unique identifier for the message.                     |
| FileName          | string     | Name of the audio file.                                |
| Duration          | int        | Duration of the message.                               |

**Methods**: None

---

Here’s the enhanced **Handlers** description with method return types included:

---

### **AccessControlHandler**

**Description**: Manages access control features, such as unlocking doors and interacting with access-related devices.  
**Key Features**:
- Handles the IP address of the parent device.
- Uses the WAMP client for communication.

**Properties**:
| **Property**         | **Type**   | **Description**                                 |
|-----------------------|------------|-------------------------------------------------|
| `ParentIpAddress`    | string     | IP address of the parent device.                |

**Methods**:
| **Method**             | **Return Type** | **Description**                                                   |
|-------------------------|-----------------|-------------------------------------------------------------------|
| `#ctor`               | void            | Initializes the handler with references to events, WAMP client, and parent IP. |
| `OpenDoor(Device ele)`| bool            | Opens a door associated with the specified device.                 |

---

### **AudioEventHandler**

**Description**: Processes audio events detected by the system, such as data receiving, audio event detection, and detector alive signals.  
**Key Features**:
- Interacts with the WAMP client to handle real-time audio analytics events.

**Properties**:
| **Property**         | **Type**   | **Description**                                 |
|-----------------------|------------|-------------------------------------------------|
| `ParentIpAddress`    | string     | IP address of the parent device.                |

**Methods**:
| **Method**                           | **Return Type** | **Description**                                                       |
|---------------------------------------|-----------------|-----------------------------------------------------------------------|
| `#ctor`                              | void            | Initializes the handler with references to events, WAMP client, and parent IP. |
| `HandleAudioEventDetection`          | void            | Handles audio event detection and triggers the related event.         |
| `HandleAudioDataReceivingEvent`      | void            | Handles audio data reception and triggers the data received event.    |
| `HandleAudioDetectorAliveEvent`      | void            | Processes detector alive signals and triggers the heartbeat event.    |

---

### **BroadcastingHandler**

**Description**: Manages group and audio message broadcasting. Handles retrieving, playing, stopping, and managing audio messages.  
**Key Features**:
- Manages group and audio message retrieval using WAMP.
- Plays or stops audio messages for defined groups.

**Properties**:
| **Property**                        | **Type**   | **Description**                                                    |
|--------------------------------------|------------|--------------------------------------------------------------------|
| `IsExecutingGroupRetrieval`         | bool       | Indicates if group retrieval is in progress.                       |
| `IsExecutingAudioMessagesRetrieval` | bool       | Indicates if audio message retrieval is in progress.               |
| `ParentIpAddress`                   | string     | IP address of the parent device.                                   |

**Methods**:
| **Method**                             | **Return Type** | **Description**                                                       |
|-----------------------------------------|-----------------|-----------------------------------------------------------------------|
| `#ctor`                                | void            | Initializes the handler with references to collections, events, WAMP client, and parent IP. |
| `HandleConnectionChange`               | void            | Updates group and audio message lists when a connection change occurs. |
| `RetrieveGroups`                       | bool            | Fetches groups from the server and updates the collection.            |
| `RetrieveAudioMessages`                | bool            | Fetches audio messages from the server and updates the collection.    |
| `PlayAudioMessage(AudioMessage, ...)`  | bool            | Plays the specified audio message for a group.                        |
| `StopAudioMessage(AudioMessage msg)`   | bool            | Stops the playback of the specified audio message.                    |

---

### **CallHandler**

**Description**: Processes call-related operations, such as managing active calls, queued calls, and synchronization with the system.  
**Key Features**:
- Handles call initiation, updates, and synchronization of active and queued calls.
- Manages operator and device interactions during calls.

**Properties**:
| **Property**       | **Type**   | **Description**                                 |
|---------------------|------------|-------------------------------------------------|
| `ParentIpAddress`  | string     | IP address of the parent device.                |
| `Operator`         | Device     | The operator device.                            |
| `ActiveDevice`     | Device     | The currently active device.                    |

**Methods**:
| **Method**                               | **Return Type** | **Description**                                                      |
|-------------------------------------------|-----------------|----------------------------------------------------------------------|
| `#ctor`                                  | void            | Initializes the handler with references to collections, events, WAMP client, and parent IP. |
| `HandleCallStatusChangedEvent`           | void            | Updates active calls or queues based on call status changes.         |
| `HandleCallQueueChange`                  | void            | Updates the queued calls collection on queue changes.                |
| `PostCall`                               | bool            | Initiates a new call with specified parameters.                      |
| `AddToActiveCalls(Device)`               | bool            | Adds a device to the active calls list.                              |
| `RemoveFromActiveCall(Device)`           | bool            | Removes a device from the active calls list.                         |
| `AnswerQueuedCall(CallLegElement)`       | bool            | Answers a queued call.                                               |
| `GetAllCalsAndQueues`                    | List<CallElement>| Retrieves and synchronizes all active and queued calls.              |

---

### **ConnectionHandler**

**Description**: Manages the WAMP server connection, including reconnection attempts and configuration updates.  
**Key Features**:
- Monitors and updates the system connection state.

**Properties**:
| **Property**       | **Type**   | **Description**                                 |
|---------------------|------------|-------------------------------------------------|
| `IsConnected`      | bool       | Indicates whether the system is connected.      |
| `IsReconnecting`   | bool       | Indicates if the system is attempting to reconnect. |
| `ParentIpAddress`  | string     | IP address of the parent device.                |

**Methods**:
| **Method**                       | **Return Type** | **Description**                                                      |
|-----------------------------------|-----------------|----------------------------------------------------------------------|
| `#ctor`                          | void            | Initializes the handler with references to events, WAMP client, and configuration. |
| `HandleConnectionChangeEvent`    | void            | Processes connection status changes.                                |
| `HandleConfigurationChangeEvent` | void            | Updates WAMP client settings based on configuration changes.        |
| `OpenConnection`                 | bool            | Opens a new WAMP server connection.                                 |
| `Reconnect`                      | bool            | Attempts to reconnect to the server.                                |

---

### **DatabaseHandler**

**Description**: Synchronizes internal collections with the database.  
**Key Features**:
- Provides synchronization between runtime data and persistent storage.

**Methods**:
| **Method**                       | **Return Type** | **Description**                                                      |
|-----------------------------------|-----------------|----------------------------------------------------------------------|
| `#ctor`                          | void            | Initializes the handler with collections, events, and the WAMP client. |

---

### **DeviceHandler**

**Description**: Manages device registration, retrieval, and state updates.  
**Key Features**:
- Interacts with the WAMP client to manage device-related operations.
- Tracks device states and handles retrieval tasks.

**Properties**:
| **Property**                   | **Type**   | **Description**                                    |
|---------------------------------|------------|----------------------------------------------------|
| `DeviceRetrievalTimer`         | Timer      | Timer for periodically retrieving registered devices. |
| `IsExecutingDeviceRetrieval`   | bool       | Indicates if device retrieval is in progress.      |
| `ParentIpAddress`              | string     | IP address of the parent device.                  |

**Methods**:
| **Method**                             | **Return Type** | **Description**                                                      |
|-----------------------------------------|-----------------|----------------------------------------------------------------------|
| `#ctor`                                | void            | Initializes the handler with references to collections, events, and WAMP client. |
| `HandleDeviceRegistration`             | void            | Registers a device received from the WAMP client.                   |
| `RetrieveRegisteredDevices`            | List<Device>    | Fetches registered devices and updates internal collections.         |
| `HandleDeviceGPIOStatusEvent`          | void            | Routes incoming GPI/GPO change events to the matching device model.  |
| `SimulateKeyPress(string, string, ...)`| bool            | Simulates a key press on a device.                                   |
| `InitiateToneTest(string, string)`     | bool            | Starts a tone test on the specified device.                          |

---

---

### 5. Authors and Acknowledgment
- **Toni Gregov**: Project Lead
- **WampClient Fork**: Erik Mortensen

---

### 6. License


