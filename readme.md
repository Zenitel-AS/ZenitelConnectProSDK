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

Here is the expanded README section based on the provided code:

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

##### Audio Analytics Events
The SDK also provides specialized event handlers for audio analytics:

| Event Name                   | Description                                                                  |
|------------------------------|------------------------------------------------------------------------------|
| `DataReceived`               | Fires when the Audio Event Detector receives data.                           |
| `Heartbeat`                  | Occurs when the Audio Event Detector sends a heartbeat signal every minute.  |
| `AudioEventDetected`         | Fires when an audio event is detected (e.g., gunshot, aggression, glass break). |

These event handlers allow developers to seamlessly integrate real-time monitoring, logging, and interaction functionalities within the system.

Here is the expanded README section incorporating the `Collections` class:

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

Here are the **tables for each model** with their properties and methods, ensuring nothing is skipped:

---

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
| `SimulateKeyPress(string, string, ...)`| bool            | Simulates a key press on a device.                                   |
| `InitiateToneTest(string, string)`     | bool            | Starts a tone test on the specified device.                          |

---

---

### 5. Authors and Acknowledgment
- **Toni Gregov**: Project Lead
- **WampClient Fork**: Erik Mortensen

---

### 6. License


