# EdiabasLib supported adapter types

The following table shows which adapter and vehicle combination is supported:

| Adapter | BMW-DS2 | BMW-FAST | BMW-FAST-ENET |VAG |
| ------- | ------- | -------- | ----------- | --- |
| _FTDI USB_ | Yes | Yes | D-CAN | No |
| _ELM327_ | No | D-CAN only | D-CAN | No |
| _Custom_ | Yes | Yes | D-CAN | No |
| _Deep OBD_ | Yes | Yes | D-CAN | Yes |
| _Deep OBD ENET_ | No | No | Yes | No |

## Vehicle legend:
* _BMW-DS2_: BMW models (with DS2 protocol): E36, E38, E39, E46, E52, E53, E83, E85 and E86.  
An OBD II Pin 7+8 connection in the adapter is required!
* _BMW-FAST_: BMW E series with BMW-FAST protocol (DS2 successor protocol).
* _BMW-FAST-ENET_: BMW models with ENET (F series or higher with HSFZ or DoIP protocol).
* _VAG_: All VAG models. This mode is still experimental!

## Adapter legend:
* _FTDI USB_: Standard FTDI based USB "INPA compatible" D-CAN/K-Line adapters.  
Do not use adapters with fake FT232R chip, there are communication problems!
* _ELM327_: ELM327 based Bluetooth and WiFi adapters. Recommended ELM327 versions are 1.4b, 1.5 and origin 2.1, which are based on PIC18F25K80 processor (no MCP2515 chip).  
Only D-CAN is supported (BMW vehicles starting from 3/2007).  
There are fake PIC18F25K80 processors with version 1.5, these will not work!.  
ARM based adapters are in most cases not 100% ELM327 compatible. The adapter manufacturer must explicitly state, that the adapter is Deep OBD compatible.
* _Custom_: Custom [Bluetooth D-CAN/K-Line adapter](Build_Bluetooth_D-CAN_adapter.md).
* _Deep OBD_: Bluetooth and WiFi adapters with [Replacement firmware for ELM327](Replacement_firmware_for_ELM327.md).  
For _BMW-DS2_ vehicles an OBD II Pin 7+8 connection in the adapter is required!
* _Deep OBD ENET_: [ENET WiFi adapters](ENET_WiFi_Adapter.md) for BMW F-models.
