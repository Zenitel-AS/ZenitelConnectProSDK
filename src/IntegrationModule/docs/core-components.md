# Core Components

The **Zenitel Connect Pro SDK** is built on a modular architecture designed for flexibility and scalability. This section provides an overview of the core components that make up the SDK.

---

## **1. Overview**
The SDK consists of several key components that work together to provide a robust integration framework:

- **Shared Components** – Common utilities, models, and tools used across the SDK.
- **WampClient** – The real-time communication layer.
- **Integration Modules** – Custom modules for interacting with third-party systems.

Each of these components plays a critical role in the SDK’s functionality.

---

## **2. Shared Components**
The **SharedComponents** library is the foundation of the SDK, providing a rich set of tools, utilities, and models.

### **Key Features:**
- **Logging & Debugging Tools**  
  - Centralized logging and debugging utilities.
- **Collections & Data Handling**  
  - Manages SDK entities such as devices, calls, and events.
- **Event Handling System**  
  - Implements an event-driven programming model.
- **Enums & Constants**  
  - Provides standardized enumerations to ensure consistency.
- **Cryptographic Utilities**  
  - Handles secure encryption and decryption.
- **Process Management**  
  - Controls system-level processes efficiently.

📌 **File:** [`shared-components.md`](core-components/shared-components.md)

---

## **3. WampClient**
The **WampClient** component is responsible for real-time messaging and communication.

### **Key Features:**
- **Implements WAMP Protocol**  
  - Uses `WampSharp` for Web Application Messaging Protocol (WAMP).
- **Real-Time Data Exchange**  
  - Enables communication between system components.
- **Integration with Shared Components**  
  - Manages devices, call events, and audio analytics.

📌 **File:** [`wamp-client.md`](core-components/wamp-client.md)

---

## **4. Integration Modules**
The **Integration Modules** provide connectivity with external systems.

### **Key Features:**
1. **Access Control**  
   - Manages door access and security permissions.
2. **Audio Analytics**  
   - Processes audio events such as gunshot or glass break detection.
3. **Device Management**  
   - Registers and monitors intercom and PA devices.
4. **Call Handling**  
   - Manages call operations and event synchronization.
5. **Broadcasting**  
   - Enables PA system announcements and group messages.

📌 **File:** [`integration-modules.md`](core-components/integration-modules.md)

---

## **5. Summary**
The **Zenitel Connect Pro SDK** follows a modular architecture with three core components:
- **SharedComponents** – The foundation of the SDK.
- **WampClient** – The real-time messaging system.
- **Integration Modules** – The extension layer for third-party integrations.

Each component is designed to be **extensible**, **high-performance**, and **scalable** for various communication and security applications.

🔗 **Next:**
- [Shared Components](core-components/shared-components.md)
- [WampClient](core-components/wamp-client.md)
- [Integration Modules](core-components/integration-modules.md)
