# AllJoyn TS DSB
A device system bridge that works with 433mhz devices via Telldus Live.

Currently connects to Telldus Live, and converts Telldus devices to AllJoyn devices.
Using offline protocols is in the works (and part of the source)

Current version supports:

### Temperature sensor

org.alljoyn.SmartSpaces.Environment.CurrentTemperature

### Humidity sensor

org.alljoyn.SmartSpaces.Environment.CurrentHumidity

### Dimmer, outlets

Lighting Service Framework


Next version:
Implementing https://github.com/dotMorten/AlljoynDSB
Adding support for switch
