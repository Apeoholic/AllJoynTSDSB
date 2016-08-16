# AllJoyn TS DSB
A device system bridge that works with 433mhz devices via Telldus Live.

Currently connects to Telldus Live, and converts Telldus devices to AllJoyn devices.

Using offline protocols is in the works (and part of the source)

## Current version

### Temperature sensor

org.alljoyn.SmartSpaces.Environment.CurrentTemperature

### Humidity sensor

org.alljoyn.SmartSpaces.Environment.CurrentHumidity

### Dimmer, outlets

Lighting Service Framework


## Next version

Implementing https://github.com/dotMorten/AlljoynDSB

Adding support for switch

## Set up

1. Deploy the Device System Bridge to a computer (I would recommend a Raspberry PI 3 running Windows 10 IoT Core).
2. Start Device Explorer for Alljoun (Download from Windows 10 store)
3. Find Telldus Live 
4. Navigate to Telldus Live / Telldus_Live / com.apeoholic.TSDSB.TelldusLive.MainInterface
5. Choose Step1GetToken and click invoke
6. Copy the url and paste it into a web browser, follow the instructions.
7. Go back to Device Explorer and navigate to the MainInterface again and click Step2Confirm.
8. ???
9. Profit
