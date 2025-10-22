# BMW Coding
**BMW coding** is online service and similar to the coding function of ISTA+ (coding by vehicle order modification) and requires a fast and stable internet connection.  
It's only available for F series vehicles or higher.  
Because the database is too large for the app, this is a web application that only could be accessed from [Deep OBD](Deep_OBD_for_BMW_and_VAG.md).  
Vehicle telegrams are transferred via the integrated browser over the internet.  
DoIP SSL communcation with the vehicle is also possible, in this case the encrypted part of the communication is only between app and vehicle.  
After selecting the vehicle order modification (options) in the web gui, a TAL is generated that could be executed in the next step.  
While TAL execution make sure, that the ignition is switched on, the motor is stopped and a charger with at least 60A is connected to the vehicle.  
Don't close or minimize the app page, because this will interrupt vehicle communication!  
In the [beta testing phase](https://play.google.com/apps/testing/de.holeschak.bmw_deep_obd) it will be free, later a license will be required to pay the server costs.
When buying an original [Replacement firmware for ELM327](Replacement_firmware_for_ELM327.md#buy-a-preprogrammed-adapter) adapter, a license is included for one vehicle
