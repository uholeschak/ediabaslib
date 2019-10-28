# Global settings
This page allows to change some global app settings.  
* _Theme_ options:
  * _Dark_: Dark theme is selected.
  * _Light_: Light theme is selected.
* _For OBD network adapter internet connection via_ (Wi-Fi or Ethernet OBD II adapters) options:
  * _Mobile network_: Internet connection via mobile network is selected.
  * _Wi-Fi_: Internet connection via Wi-Fi is selected.
  * _Ethernet_: Internet connection via Ethernet is selected.
* _Bluetooth enable_ options:
  * _Ask for Bluetooth enable_: Ask every time if Bluetooth should be enabled.
  * _Always enable Bluetooth without request_: Always enable Bluetooth, never ask.
  * _No automatic Bluetooth handling_: Bluetooth must be enabled manually.
* _Bluetooth disable_ options:
  * _Disable Bluetooth at exit, if unused and enabled by app_: At app exit disable Bluetooth if the app has enabled it.  
  If the Bluetooth connection is in use for other services it will not be disabled.
* _Lock during communication_ and _Lock during data logging_ options:
  * _Device enters energy saving mode and the communication will be stopped_: No lock is active, the device enters energy saving mode as usual and the communication will be stopped.
  * _Keep CPU active_: The CPU keeps running but the display will be switched off.
  * _Screen dimmed_: The CPU keeps running but the display will be dimmed.
  * _Screen bright_: The CPU keeps running and the display will stay bright.
* _Data logging_ options:
  * _Store log data settings_: Store log data settings (not trace seetings) from the main menu and restore them at app restart.
* _App start_ options:
  * _No connection establishment to vehicle_: No automatic connection establishment to vehicle.
  * _Auto connection establishment to vehicle_: If possible auto connect to vehicle. This is only possible if no dialog opens at startup.
  * _Auto connection establishment to vehicle and close App (only if broadcast is active)_: If possible auto connect to vehicle. After the service has been started the App will be closed.  
  This option is only active if _Send data broadcasts_ has been selected and the service is active.
* _App exit_ options:
  * _Double click required for app exit_: A double click on the back button is required to exit the app.
* _Update check_ options:
  * _Never_: Never check for updates.
  * _Every day_: Check for updates once every day.
  * _Every week_: Check for updates once every week.
* _Broadcast_ options:
  * _Send data broadcasts_: Data is broadcasted to other apps (see [Broadcasts](Page_specification.md#broadcasts)).
* _CPU usage_ options:
  * _Check cpu usage at start_: Check CPU usage at program startup. This requires some time. I high CPU use could cause communication problems.
* _Check ECU files_ options:
  * _Check ECU files at startup_: Check if all ECU files are present and have the correct size. This is time consuming. If you are are sure the ECU files will not get damaged you could disable this option.
* _Battery voltage warning_ options:
  * _Show over voltage warning_: Show adapter overvoltage warning for adapter protection.
* _VAG mode_ options:
  * _Use old VAG mode_: Use the old VAG mode instead of the new implementation. This is only recommended only of you want to use existing configuration files.
* _ECU detection_ options:
  * _For BMW vehicles with DS2 protocol always scan all ECUs (slow)_: If the vehicle has been retrofitted the car database may be incorrect and not all ECUs may be detected.  
  This option allows to ignore the car database and all ECUs are scanned, which is very time consuming.
* _Storage media_ options:
  * _Default or storage location_: If the default storage media for the ECU files is not appropriate, a different media could be selected here.  
  The application storage directory on the media will be always fixed to _de.holeschak.bmw_deep_obd_.
* _Debug_ options:
  * _Collect debug information for trace files_: More data is collected while reading vehicle information to improve trace files.
  * _Bluetooth HCI snoop log file_: Current Bluetooth HCI snoop log file name. You could enable HCI snoop logging with the button _Configure snoop log_.

![Global settings](GlobalSettings_AppGlobalSettingsSmall.png)
