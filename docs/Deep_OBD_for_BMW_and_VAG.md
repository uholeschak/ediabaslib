# Deep OBD app
This page describes how to use _Deep OBD app_.  
Download app from Google Play: [https://play.google.com/store/apps/details?id=de.holeschak.bmw_deep_obd](https://play.google.com/store/apps/details?id=de.holeschak.bmw_deep_obd)  
Table of contents:
* [Manufacturers](#manufacturers)
* [Supported adapters](#supported-adapters)
* [First start](#first-start)
* [Your first configuration](#your-first-configuration)
* [The main menu](#the-main-menu)
	* [Configuration generator](Configuration_Generator.md)
	* [HowTo create Deep OBD for BMW and VAG pages](Page_specification.md)
	* [Ediabas tool](EdiabasTool.md)
	* [BMW coding](BMW_Coding.md)
	* [Global settings](GlobalSettings.md)
* [Google translation APIs](#google-translation-apis)
* [Log and trace files location](#log-and-trace-files-location)
* [Android Auto](#android-auto)
* [Background image](#background-image)
* [Request to delete data send by the app](Deep_OBD_Data_Delete_Request.md)

## Manufacturers
Basically the _Deep OBD app_ can operate in two modes, either BMW or VAG. You have to select the car manufacturer first. The VAG group mode (VW, Audi, Seat, Skoda) is still experimental and only supports the protocols KPW2000, KWP1281 and TP2.0. A [Replacement firmware for ELM327](Replacement_firmware_for_ELM327.md) adapter is required for this mode.

## Supported adapters
The _Deep OBD app_ supports several OBD II adapters:
* Standard FTDI based USB "INPA compatible" D-CAN/K-Line adapters (all protocols)
* ELM327 based Bluetooth and WiFi adapters. Recommended ELM327 versions are 1.4b, 1.5 and origin 2.1, which are based on PIC18F25K80 processor (no MCP2515 chip) (D-CAN protocol only) 
* Custom [Bluetooth D-CAN/K-Line adapter](Build_Bluetooth_D-CAN_adapter.md) (BMW-FAST protocol over D-CAN and K-Line)
* ELM327 based adapters with [Replacement firmware for ELM327](Replacement_firmware_for_ELM327.md) D-CAN and K-Line (all protocols!). When VAG has been selected as manufacturer, only this adapter could be used.
* [ENET WiFi adapters](ENET_WiFi_Adapter.md) (for BMW F-models)

More details could be found here: [Supported adapter types](AdapterTypes.md)

## First start
At the first start of the _Deep OBD app_ you will be asked to extract the ECU files. The extracted files are very large and requires approximately 2.5GB on the external SDCard. An internet connection is required for this to check for update information.  
In the next step a configuration (`*.cccfg` file) must be created. The easiest way to do so is to use the [configuration generator](Configuration_Generator.md). For complex scenarios you could manually create configuration files (see [HowTo create Deep OBD pages](Page_specification.md)). After loading and compiling the configuration file, all tabs included in the file will be visible on the main page.  
Before connecting to the vehicle via Bluetooth a [Bluetooth adapter](Build_Bluetooth_D-CAN_adapter.md) has to be selected (or you will be asked when connecting). It's recommended to pair the adapter in the Android Bluetooth menu before using it in the _Deep OBD app_, because this way a connection password could be assigned.

### Your first configuration
Follow the next steps to generate your first configuration (BMW):
* Open the [Configuration generator](Configuration_Generator.md) from the main menu
* In the generator menu select the correct interface type and [Bluetooth adapter](Build_Bluetooth_D-CAN_adapter.md) if needed.
* Press the _Read_ button and wait until the ECU list is populated.
* Select an ECU entry you are interested in.
* On the next page select a job and corresponding job result for data you want to see on the main page.  
Make sure you have set the checkmarks for these results!  
You could test reading the value with _Test_ button below.
* Leave the page and possibly select another ECU.
* Exit the [Configuration generator](Configuration_Generator.md) and store the configuration.
* On the main page you will see the selected ECU tabs and the _Error_ tab.
* Press the _Connect_ button.
* Select the desired tab.

![Select Bluetooth device](Deep_OBD_for_BMW_and_VAG_AppSelectBluetoothSmall.png) ![ECU list](Configuration_Generator_AppGeneratorEcusSmall.png)
![Job selection](Configuration_Generator_AppGeneratorJobSmall.png) ![Motor page](Deep_OBD_for_BMW_and_VAG_AppMotorSmall.png)

## The main menu
The application has a configuration menu with the following options:
* _Manufacturer_: Select the car manufacturer with this menu point first. The default is BMW, the other manufacturers are from the VAG group (VW, Audi, Skoda). The VAG mode is still experimental, only for vehicles until 2017-08 and requires a [Bluetooth D-CAN/K-Line adapter](Build_Bluetooth_D-CAN_adapter.md).
* _Adapter_: With this menu the [Bluetooth adapter](Build_Bluetooth_D-CAN_adapter.md) could be selected.  If the device is not paired already, searching for new devices is possible. This menu is only enabled if a configuration with _interface_ type _BLUETOOTH_ has been selected.
* _Adapter configuration_: When using a FTDI USB or Bluetooth (non ELM327) adapter, this menu item opens the adapter configuration page. The following settings are available (depending from adapter type):
	* _CAN baud rate_: (500kbit/100kbit) or K-Line (CAN off)
	* _Separation time_: Separation time between CAN telegrams. The default is 0, only change this value if there are communication problems.
	* _Block size_: Size of CAN telegram blocks. The default is 0, only change this value if there are communication problems.
	* _Firmware update_: If a new firmware is available for the adapter, the update could be initiated with this button.
* _ENET IP_: This menu displays the currently manually assigned ENET IP address and allows to search for vehicles and edit the IP address manually.  
This could be also used in hotspot mode (Hotspot active and WiFi off) if the ENET network is connected to the Android hotspot.
* _Adapter IP_: This menu displays the currently manually assigned WiFi adapter IP address and allows to edit the IP address and port.  
If the Android device is in hotspot mode (Hotspot active and WiFi off), communication with the adapter is only possible if the IP address is assigned manually.  
This is especially useful for adapters with ESP8266 chip, because they could be configured to connect to the Android hotspot automatically.  
If the port is omitted in the IP settings, the default port is 23 in hotspot mode (Hotspot active and WiFi off) and if the IP address is 192.168.4.1 (ESP8266). In all other cases the default port is 35000.
* _Configuration_: This submenu contains configuration selection and editing functions.
	* _Select_: This menu allows the selection of the [configuration file](Page_specification.md) (`*.cccfg` file). When using the [configuration generator](Configuration_Generator.md) the configuration is selected automatically. After selection the file will be compiled.
	* _Recent configurations_: In this submenu the last 10 recently used configurations are accessible.
	* _Edit_: Edit the current configuration main (`*.cccfg` file). A suitable XML editor has to be be installed.
	* _Edit pages list_: Edit the current pages list (`*.cccpages` file).
	* _Edit current page_: Submenu to edit the currently active page (`*.cccpage` file).
		* _Edit page_: Edit the currently active page in the XML editor.
		* _Font size_: Change the page font size directly without XML editor. This is only possible if the _fontsize_ tag is present in the XML file.
		* _Gauges landscape_: Change the number of gauges in landscape mode directly without XML editor. This is only possible if the _gauges-landscape_ tag is present in the XML file.
		* _Gauges portrait_: Change the number of gauges in portrait mode directly without XML editor. This is only possible if the _gauges-portrait_ tag is present in the XML file.
		* _Result display order_: Change order of the displayed results directly without XML editor. This is only possible if the _display-order_ tag is present for all _display_ nodes the XML file.
	* _Edit other file_: Allows to select and edit any configuration (`*.cccpage` file).
	* _Reset XML editor_: Resets the last selected XML editor. A new editor has be to be selected again when editing files.
	* _Close_: Close the current configuration.
* _Configuration generator_: Simple [XML configuration files](Page_specification.md) could be generated automatically using the informations obtained from the vehicle. This menu opens the [configuration generator](Configuration_Generator.md) which allows to create new or modify existing XML files by simply selecting the ECU and job informations.
* _Ediabas tool_: This is a port of the tool32.exe windows application. Selecting the menu will open the [Ediabas tool](EdiabasTool.md) page.
* _Coding_: This submenu allow to access the [BMW coding](BMW_Coding.md) online service.
* _Extract ECU files_: The ECU files are very large, so they have to be extracted at first app start. With this menu item files could be extracted again. An internet connection is required to check for update information.
* _Data logging_: Selecting this menu entry will open a sub menu with multiple data logging options:
	* _Create trace file_: If the checkbox of this menu is active, a `ifh.trc` file will be created when the application is connected. The trace file will be created in the `Log` subdirectory.
	* _Append trace file_: If this checkbox is enabled the trace file is always appended. Otherwise the trace file will be overridden after selection of a new configuration or restart of the application.
	* _Log data_: This checkbox enables logging of the display data to a log file. Only those lines are logged, that have a _log_tag_ property in the [configuration file](Page_specification.md). The _logfile_ property in the _page_ node has to be specified as well to activate logging. When using the [configuration generator](Configuration_Generator.md) _log_tag_ is set by default to the job name and _logfile_ to the ECU name. Data will be logged in the `Log` subdirectory.
* _Trace file_: Selecting this menu entry will open a sub menu with options for trace file handling.
	* _Send trace file_: Send the trace file from the last vehicle communication to the author of Ediabaslib/Deep OBD for further study and bugfixing/enhacements. Important: Data sent is compressed and anonymous.
	* _Open trace file_: Open the trace file from the last vehicle communication with an external app that supports zip files.
	* _Resend trace file_: Retry sending of the last trace file. Only visible if sending has failed.
	* _Open last trace file_: Open the last trace file with an external app that supports zip files, if sending has failed previously.
* _Translations_: (Only for non German languages) This menu opens a submenu that allows configuration of automatic ECU text translation with various translations engines. Most engines require an API key, but some have also special access tokens. Free translation if very limited in most cases and you have to pay for larger text amount.
	* _Translate ECU text_: If this menu item is checked, automatic ECU text translation is active.
	* _Translation configuration_: For automatic translation with various translation providers. For translation an API Key may be required. This menu assists to select and configure a translation provider. For using [Google translation APIs](#google-translation-apis) the URLs have to be copied first.
	* _Clear translation cache_: To enforce a new translation this menu resets the translation cache.
* _Global settings_: Opens the [global app settings](GlobalSettings.md) page.
* _Online help_: Displays this help page.
* _App info_: Displays the app version and unique app id.

![Menu](Deep_OBD_for_BMW_and_VAG_AppMenuSmall.png)

Below are some screenshots from the example E61 configuration:

![Motor page](Deep_OBD_for_BMW_and_VAG_AppMotorSmall.png) ![Motor page](Deep_OBD_for_BMW_and_VAG_AppMotorGraphSmall.png) ![Motor page](Deep_OBD_for_BMW_and_VAG_AppClimateSmall.png) ![Motor page](Deep_OBD_for_BMW_and_VAG_AppAxisSmall.png) ![Motor page](Deep_OBD_for_BMW_and_VAG_AppReadAllErrorsSmall.png)

## Google translation APIs
There are public undocumented Google translation APIs, which could also be found in the project [translatepy](https://github.com/Animenosekai/translate).  
You could copy the basic URLs from here and paste them in the _Translation configuration_ Google APIs page, but they could change without any notice.  
If one API fails the next one is tried automatically.  
```
https://clients5.google.com/translate_a/t?client=dict-chrome-ex
https://translate.googleapis.com/translate_a/single?client=gtx&dt=t
```

## Log and trace files location
The location of the log and trace files depends from the Android version.  
Beginning with Android KitKat (4.4) writing to the external SdCard is not possible any more. For older Android versions log and trace files are stored in a subdirectory relative to `de.holeschak.bmw_deep_obd` on the external SDCard. For KitKat and above the data could be found in the directory `Android\data\de.holeschak.bmw_deep_obd\files` of the external SDCard.  
The standard log files are stored in the subdirectory `Log`, whereas the [Ediabas tool](EdiabasTool.md) uses the subdirectory `LogEdiabasTool` and the [configuration generator](Configuration_Generator.md) the subdirectory `LogConfigTool`.  
If sending of the trace files fails, the backup trace files are stored in the subdirectory `TraceBackup`.

## Background image
It's possible to replace the background image. Simply store a custom `Background.jpg` file in the directory `de.holeschak.bmw_deep_obd\files\Images` (The `Images` subdirectory has to be created first).

## Android Auto
Android Auto is now available in the release version of the _Deep OBD app_.

## Visual Studio Settings for Compilation
Visual Studio Android settings.  
It's recommended to install and configure Android Studio before and then use a common configuration.  
Update the components in Android Studio only.
* Java SDK location: Microsoft: `C:\Program Files\Microsoft\jdk-17.0.8.101-hotspot` or custom: `C:\Program Files\Java\jdk-11.0.12`
* Android SDK location: `C:\Users\<user>\AppData\Local\Android\android-sdk`
* Archive location: `C:\Users\<user>\AppData\Local\Xamarin\Mono for Android\Archives`
* Activate option: _Keep application cache_
* Activate option: _Install Android SDK automatically_
* Deactivate option: _Use AndroidX migrator_
* Extra -> Android -> Android SDK Manager: Enable option _Repository complete list_ (if Android Studio is installed)
