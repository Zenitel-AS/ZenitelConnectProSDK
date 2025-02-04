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

### 2. Getting Started
- **Build Instructions**:
  1. Clone the repository or download the package.
  2. Build the required module for `.NET Framework 4.8`.
  3. Reference the appropriate DLLs in your project.

- **Basic Configuration**:
  ```csharp
  var core = new ConnectPro.Core();

  core.Configuration.ServerAddr = "169.254.1.5";
  core.Configuration.Port = "8087";
  core.Configuration.UserName = "admin";
  core.Configuration.Password = "password";

  core.Start();
  core.ConnectionHandler?.OpenConnection();
  ```

---

### 3. Core Components
#### **SharedComponents**
- Core shared library used across all projects, including:
  - `Debug` utilities for logging and monitoring.
  - `Collections` for managing data.
  - `Events` for event-driven programming.

#### **WampClient**
- Forked and optimized WAMP library:
  - `WampClient.NET48`: For .NET Framework 4.8.

#### **Integration Modules**
- Backend DLLs for `.NET Framework 4.8`.

---

### 4. Usage Instructions

#### Event Handlers
The SDK provides robust event handling to track changes and updates in real time. Below are the key event handlers available:

| Event Name                   | Description                                                                 |
|------------------------------|-----------------------------------------------------------------------------|
| `OnConnectionChanged`        | Triggers on changes in connection status with Zenitel Connect.              |
| `OnDeviceListChange`         | Fires when the list of devices is updated.                                 |
| `OnDeviceStateChange`        | Notifies changes in the state of a specific device.                        |
| `OnDeviceRetrievalStart`     | Signals the start of the device retrieval process.                         |
| `OnDeviceRetrievalEnd`       | Signals the end of the device retrieval process.                           |
| `OnLogEntryAdded`            | Triggered when a log entry is successfully added.                          |

#### Collections
Collections within the SDK serve as repositories for various data entities. Below is a detailed list:

| Property Name      | Description                                      | Type               |
|--------------------|--------------------------------------------------|--------------------|
| `RegisteredDevices`| Collection of all devices registered.            | `List<Device>`     |
| `ActiveCalls`      | Collection of currently active calls.            | `List<CallElement>`|
| `CallQueue`        | Collection of queued calls.                      | `List<CallLegElement>`|
| `Groups`           | Collection of user groups.                       | `List<Group>`      |
| `Directories`      | Collection of registered directory numbers.      | `List<DirectoryNumber>`|

#### Models
The SDK provides several data models representing core entities. Key models include:

- **Device**:
  - Properties: `DeviceId`, `NodeNumber`, `DeviceState`, `CallState`.
  - Methods: `SetValuesFromSDK`, `NewDeviceFromSdkElement`.

- **Group**:
  - Properties: `Id`, `Dirno`, `Members`, `Priority`.
  - Methods: `SetValuesFromSDK`, `NewDeviceFromSdkElement`.

#### Handlers
Handlers are responsible for specific functionality within the SDK. Below is a detailed description:

- **AccessControlHandler**:
  - **Purpose**: Handles door access control.
  - **Methods**:
    - `OpenDoor(Device device)`: Opens the door associated with the specified device.

- **BroadcastingHandler**:
  - **Purpose**: Manages group operations.
  - **Methods**:
    - `RetrieveGroups()`: Retrieves groups asynchronously.

- **LogHandler**:
  - **Purpose**: Manages application logging.
  - **Methods**:
    - `AddLogEntry(string message)`: Adds a log entry.

---

### 5. Authors and Acknowledgment
- **Toni Gregov**: Project Lead
- **WampClient Fork**: Erik Mortensen

---

### 6. License
Include details of the license here (e.g., MIT, proprietary, etc.).

