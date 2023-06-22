# CanWifiTerminal
CanWifiTerminal allows send and receive [CAN bus](https://en.wikipedia.org/wiki/CAN_bus) messages using the [ECAN-W01](https://www.cdebyte.com/products/ECAN-W01) / [ECAN-W01S](https://www.cdebyte.com/products/ECAN-W01S) device by [EBYTE](https://www.cdebyte.com/).

Features:  
1. Connect to the ECAN-W01 device. The device needs to be set to TCP Server.
2. Send and receive can messages
3. Support standard and extended frame identification
4. Can dump messages to JSON file. The JSON can be used later to resend the message
5. Can be used as a sample for how to automate CAN Bus interactions using the ECAN-W01 device
6. Developed using C# and .NET 7.0 Compatible with Windows and Linux.

Note: This tool is not replacing the "EBYTE Network Configuration Tool" which is used for initial setup.