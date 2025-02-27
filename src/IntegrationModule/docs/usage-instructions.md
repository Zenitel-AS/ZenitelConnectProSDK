# Usage Instructions

## **1. Connection Management**
The `ConnectionHandler` is responsible for managing the connection lifecycle.

---

### **Opening a Connection**
Before establishing a connection, ensure that the necessary configuration parameters (such as **server address, port, username, and password**) are correctly set. Once the values are assigned, the `OnConfigurationChanged` event must be triggered to **apply the changes system-wide**. This ensures that all dependent components are updated before attempting to connect.

```csharp
if (!core.ConnectionHandler.IsConnected)
{
    core.Configuration.ServerAddr = "169.254.1.5";
    core.Configuration.Port = "8087";
    core.Configuration.UserName = "admin";
    core.Configuration.Password = "password";

    // Apply changes system-wide
    core.Events.OnConfigurationChanged?.Invoke(this, core.Configuration);

    // Attempt to establish a connection
    core.ConnectionHandler.OpenConnection();
}
```

✅ **This ensures that all system components receive the updated configuration before attempting a connection.**  
✅ **Only attempts a connection if not already connected, preventing redundant operations.**  

---

### **Reconnecting if Disconnected**  
If the connection to the system is lost, the `Reconnect()` method attempts to **re-establish** it. This method follows a retry mechanism, ensuring that the system automatically tries to restore connectivity without requiring manual intervention.

```csharp
core.ConnectionHandler.Recconect();
```

✅ **Automatically attempts to reconnect when the connection is lost.**  
✅ **Prevents unnecessary manual reconnection attempts.**  
❌ **Should not be used manually unless an intentional connection interruption is required (e.g., switching to another server address).**  

**⚠️ Important:**  
- The SDK **automatically manages reconnections**, so calling this method manually is **not recommended** under normal circumstances.  
- Only use `Reconnect()` if there is a need to **forcefully disconnect and reinitialize the connection** (e.g., when switching between different server configurations).  

---

### **Listening for Connection Changes**  
The SDK provides an event-driven approach to **monitoring the connection state**. The `OnConnectionChanged` event fires whenever the connection is established or lost, allowing the application to react accordingly (e.g., updating UI elements or triggering alerts).

```csharp
core.Events.OnConnectionChanged += (sender, isConnected) =>
{
    Console.WriteLine($"Connection Status: {(isConnected ? "Connected" : "Disconnected")}");
};
```

✅ **Provides real-time status updates for connection changes.**  
✅ **Can be used to trigger notifications or handle reconnection logic dynamically.**  

---

**Ensuring Connection Stability:**  
- The SDK **automatically retries** failed connections within a safe limit.  
- **Events keep track of connection status**, allowing **system-wide responses** to failures.  
- **Manual reconnection is only necessary if switching servers or handling network reconfigurations.**  

---

## **2. Device Management**  
The `DeviceHandler` is responsible for **managing Zenitel devices**, including **automatic retrieval, registration, and status tracking**.

### **Device Retrieval Process**  
- **Device retrieval is handled automatically** by the SDK.  
- The collection of **registered devices** (`core.Collection.RegisteredDevices`) is **populated dynamically** as devices become available.  
- Any **change in the device list** triggers the `OnDeviceListChange` event, which ensures that the system remains updated in real time.  

### **Listening for Device List Changes**  
Whenever a **change occurs**, such as:
- **Initial population of the device list** during startup,  
- **Device status updates**,  
- **New devices being added or removed** in the **Zenitel Connect Pro web interface**,  
- **Device failures or connectivity changes**,  

The `OnDeviceListChange` event **notifies the application** that the list has been updated.

```csharp
core.Events.OnDeviceListChange += (sender, e) =>
{
    Console.WriteLine("Device list updated.");
};
```

### **Retrieving Registered Devices**  
Although devices are automatically retrieved, the updated **Registered Devices** list can be accessed at any time:

```csharp
var devices = core.Collection.RegisteredDevices;
foreach (var device in devices)
{
    Console.WriteLine($"Device: {device.Name} ({device.Dirno}) - {device.DeviceState}");
}
```

### **Manually Triggering Device Retrieval**  
While device retrieval is **automatic**, it can be **manually triggered** if necessary:

```csharp
await core.DeviceHandler.RetrieveRegisteredDevices();
```

✅ **Device retrieval is fully automated—manual triggering is rarely needed.**  
✅ **All device list modifications are reflected system-wide via `OnDeviceListChange`.**  
✅ **Ensures real-time synchronization with Zenitel Connect Pro.**  

---

### **Simulating a Key Press**  
The `SimulateKeyPress` method allows sending a **remote key press** to a Zenitel device. This can be used for **triggering specific device functions**, such as **activating relays, initiating calls, or executing predefined actions**.

```csharp
bool success = core.DeviceHandler.SimulateKeyPress("1001", "p1", "press");
Console.WriteLine(success ? "Key press successful" : "Key press failed");
```

#### **Parameters:**
- **`deviceid`** → The directory number (Dirno) or MAC address of the target device.
- **`key`** → The key to be pressed (e.g., `"p1"`, `"m"`, `"0-9"`, `"save-autoanswer"`).
- **`edge`** → Defines the type of key press event (`"press"`, `"tap"`, `"release"`).

✅ **Used for triggering custom actions on intercom and PA devices.**  
✅ **Can be integrated with automation scripts for hands-free operation.**  

---

### **Initiating a Tone Test**  
The `InitiateToneTest` method allows sending a **test tone signal** to a specified device. This function is useful for **verifying audio output, diagnosing speaker/microphone issues, or testing PA system announcements**.

```csharp
bool testSuccess = core.DeviceHandler.InitiateToneTest("1001", "1");
Console.WriteLine(testSuccess ? "Tone test successful" : "Tone test failed");
```

#### **Parameters:**
- **`dirno`** → The directory number (Dirno) of the target device.
- **`toneGroup`** → The tone group to be tested (e.g., `"1"`, `"2"`, `"3"` for different tone sets).

✅ **Ensures that audio functionality is working correctly before deployment.**  
✅ **Useful for remote troubleshooting of intercom devices.**  

---

## **3. Call Handling**  
The `CallHandler` manages **call operations**, including **initiating, answering, ending, and monitoring calls**.  

### **Initiating a Call**  
Start a new call between two devices.  
```csharp
core.CallHandler.PostCall("1001", "1002", "setup", true);
```
✅ **"1001"** → Calling device  
✅ **"1002"** → Target device  
✅ **"setup"** → Call type  

---

### **Answering a Queued Call**  
Accept an incoming queued call.  
```csharp
core.CallHandler.AnswerQueuedCall(queuedCall);
```
✅ `queuedCall` is retrieved from **active call lists**  

---

### **Ending an Active Call**  
Terminate an ongoing call.  
```csharp
core.CallHandler.DeleteCall("1001");
```
✅ **"1001"** → Device to disconnect  

---

### **Retrieving Active Calls**  
Get a list of all active calls.  
```csharp
var activeCalls = core.CallHandler.GetAllCals("", "", "");
foreach (var call in activeCalls)
{
    Console.WriteLine($"Active Call: {call.CallId} - {call.CallType}");
}
```
✅ Retrieves **all active calls**  
✅ Displays **Call ID and Type**  

---

### **Listening for Active Call Changes**  
Trigger an event when call status changes.  
```csharp
core.Events.OnActiveCallListValueChange += (sender, e) =>
{
    Console.WriteLine("Active call list updated.");
};
```
✅ **Automatically detects** new, ended, or modified calls  

---

**📌 Key Notes:**  
- **Calls are dynamically updated** in the internal collections.  
- **Events provide real-time updates**, reducing the need for polling.  
- **Designed for real-time intercom and security applications.**  

## **4. Audio Analytics**
The `AudioEventHandler` processes **audio events**, such as **gunshots or glass breaks**.

### **Listening for Detected Audio Events**
```csharp
core.Events.AudioAnalytics.AudioEventDetected += (sender, audioEvent) =>
{
    Console.WriteLine($"Audio Event: {audioEvent.EventType} detected at {audioEvent.Time}");
};
```

### **Listening for Data Reception Events**
```csharp
core.Events.AudioAnalytics.DataReceived += (sender, dataEvent) =>
{
    Console.WriteLine($"Data Received from: {dataEvent.FromName}");
};
```

✅ The system automatically handles incoming audio events **without manual intervention**.

---

## **5. Broadcasting (PA Announcements)**
The `BroadcastingHandler` manages **group messaging and audio announcements**.

### **Retrieving Available Groups**
```csharp
core.BroadcastingHandler.RetrieveGroups();
```

### **Retrieving Audio Messages**
```csharp
var messages = core.BroadcastingHandler.GetAudioMessages();
foreach (var msg in messages.AudioMessages)
{
    Console.WriteLine($"Audio Message: {msg.FileName} ({msg.Duration} sec)");
}
```

### **Playing an Audio Message**
```csharp
core.BroadcastingHandler.PlayAudioMessage(audioMessage, "1001", "play", 1);
```

### **Stopping an Audio Message**
```csharp
core.BroadcastingHandler.StopAudioMessage(audioMessage);
```

✅ Broadcasting is **asynchronous** and allows **group-based messaging**.

---

## **6. Access Control (Door Unlocking)**
The `AccessControlHandler` provides **door unlocking capabilities**.

### **Unlocking a Door**
```csharp
core.AccessControlHandler.OpenDoor(selectedDevice);
```

### **Listening for Door Events**
```csharp
core.Events.OnDoorOpen += (sender, e) =>
{
    Console.WriteLine("A door was opened.");
};
```

✅ **Access control operations** can be logged for security monitoring.

---

## **7. Database Synchronization**
The `DatabaseHandler` is intended for **synchronizing SDK data with a database**.

> ⚠️ **Note:** This class is **not fully implemented** in the SDK.

---

## **8. Logging & Debugging**
The SDK provides a **centralized logging system**.

### **Enabling Error Logging**
```csharp
core.Log.EnableLogging = true;
```

### **Listening for Errors**
```csharp
core.Events.OnExceptionThrown += (sender, ex) =>
{
    Console.WriteLine($"Error: {ex.Message}");
};
```

✅ Logs can be **stored locally** or **monitored in real-time**.

---

## **9. Exception Handling**
The SDK provides structured exception handling.

### **Handling General Exceptions**
```csharp
try
{
    core.CallHandler.PostCall("1001", "1002", "setup", true);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

✅ Using **try-catch** ensures **application stability**.

---

## **10. Summary of Key Methods**
| Handler                  | Functionality                                | Key Methods |
|--------------------------|--------------------------------------------|-------------|
| **ConnectionHandler**    | Connection lifecycle management            | `OpenConnection()`, `Recconect()` |
| **DeviceHandler**        | Device registration, control, and retrieval| `RetrieveRegisteredDevices()`, `SimulateKeyPress()`, `InitiateToneTest()` |
| **CallHandler**          | Call operations (initiate, answer, end)    | `PostCall()`, `DeleteCall()`, `GetAllCals()` |
| **AudioEventHandler**    | Real-time audio event detection            | `AudioEventDetected`, `DataReceived` |
| **BroadcastingHandler**  | Group messaging & PA announcements         | `RetrieveGroups()`, `PlayAudioMessage()`, `StopAudioMessage()` |
| **AccessControlHandler** | Security & door access control             | `OpenDoor()` |
| **Logging & Debugging**  | Centralized error handling                 | `EnableLogging`, `OnExceptionThrown` |

✅ **All functionalities are accessed through `CoreInstance.Instance`.**

---

## **Next Steps**
- **For API documentation:** [API Reference](technical/api-reference.md)
- **For troubleshooting issues:** [Troubleshooting Guide](troubleshooting/common-issues.md)

---