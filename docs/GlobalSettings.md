# Global settings
This page allows to change some global app settings.  
* _Language_ options:
  * _Default_: Use device system language.
  * _Other_: Use specified language.
* _Theme_ options:
  * _Dark_: Dark theme is selected.
  * _Light_: Light theme is selected.
* _Translator_ options:
  * _Yandex.Translate_: Use Yandex.Translate for translation (Api Key required).
  * _IBM Watson_: Use IBM Watson for translation (Api Key required).
* _Translator keep alive_ options:
  * _Keep IBM Watson Api Key alive once a day_: The IBM Watson Api Key expires after one month if unused. The auto login keeps the key alive.
* _Title bar_ options:
  * _Auto hide title bar on main page_: Auto hide title bar 3 seconds after connecting. You could hide / show the title bar with a swipe gesture at the top edge of the app or display the bar with a long click and release on the main page.
  * _Suppress unused title bars_: Title bars without special functions are hidden by default. You could hide / show the title bar with a swipe gesture at the top edge of the app.
* _Multi window_ options:
  * _Graphical display: Swap display orientation in multi window mode_: In multi window mode some devices change the orientation and some not. With this option you could invert the orientation for graphical display.
* _For OBD network adapter internet connection via_ (Wi-Fi or Ethernet OBD II adapters, only Android 21 or higher) options:
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
  This feature is only available for Android 12 and lower.
* _Lock during communication_ and _Lock during data logging_ options:
  * _Device enters energy saving mode and the communication will be stopped_: No lock is active, the device enters energy saving mode as usual and the communication will be stopped.
  * _Keep CPU active_: The CPU keeps running but the display will be switched off.
  * _Screen dimmed_: The CPU keeps running but the display will be dimmed.
  * _Screen bright_: The CPU keeps running and the display will stay bright.
* _Data logging_ options:
  * _Store log data settings_: Store log data settings (not trace settings) from the main menu and restore them at app restart.
* _App start_ options:
  * _No connection establishment to vehicle_: No automatic connection establishment to vehicle.
  * _Auto connection establishment to vehicle_: If possible auto connect to vehicle with the last selected page.  
    This is only possible if no dialog opens at startup (All requests have to be acknowledged before).
  * _Auto connection establishment to vehicle and close App_: If possible auto connect to vehicle with the last selected page.  
    This is only possible if no dialog opens at startup (All requests have to be acknowledged before).
    After the service has been started the app will be closed.
  * _Auto start the service at boot time to restore the last connection state_: When the device is rebooted while communication to the vehicle is active, the app will start as service in the background and use the last main page for communication.  
    The boot option is very often not working on Android radios.  
    If an Android app manager is available, it's recommended to configure Deep OBD in the app manager to stay active in standby.  
    Otherwise choose the second start option and keep Deep OBD always in foreground to make the app remain active after standby. With other tools (e.g. Automation, Macrodroid) the app has to be launched after booting.
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
  * _Check ECU files at startup_: Check if all ECU files are present and have the correct size. This is time consuming. If you are sure the ECU files will not get damaged you could disable this option.
* _Battery voltage warning_ options:
  * _Show over voltage warning_: Show adapter overvoltage warning for adapter protection.
* _VAG mode_ options:
  * _Use old VAG mode_: Use the old VAG mode instead of the new implementation. This is only recommended only of you want to use existing configuration files.
* _BMW database_ options:
  * _Use BMW database_: Use BMW database for job and fault interpretation and translation. When this option is disabled communication startup is faster.
  * _Show only relevant errors_: Show only errors that are marked as relevant in the BMW database.  
  Most info memory entries are hidden and the shadow memory is never read in this mode.  
  Some entries depend on integration level and may change after vehicle update.  
* _ECU detection_ options:
  * _For BMW vehicles with DS2 protocol always scan all ECUs (slow)_: If the vehicle has been retrofitted the car database may be incorrect and not all ECUs may be detected.  
  This option allows to ignore the car database and all ECUs are scanned, which is very time consuming.
* _File management_ options:  
(These functions have to be used, if android prevents direct external access of the app folder.)
  * _Copy directory to app_: Copy a directory from an external folder to the app folder.
  * _Copy file or directory from app_: Copy a file or directory from the app folder to an external folder.
  * _Delete file or directory from app_: Delete a file or directory from the app folder.
* _Storage media_ options:
  * _Default or storage location_: If the default storage media for the ECU files is not appropriate, a different media could be selected here.  
  The application storage directory on the media will be always fixed to `de.holeschak.bmw_deep_obd`.
* _Debug_ options:
  * _Collect debug information for trace files_: More data is collected while reading vehicle information to improve trace files.
  * _Bluetooth HCI snoop log file_: Current Bluetooth HCI snoop log file name. You could enable HCI snoop logging with the button _Configure snoop log_.
* _Settings_ options:
  * _Default settings_: Restore the default settings for the global settings page.
  * _Export settings_: Export settings to `DeepObbSettings.xml` in the selected storage media folder subdirectory `Exports`. You could select if you also want to export private data.  
  You should not pass on private data files to other people.
  * _Import settings_: Import settings from `DeepObbSettings.xml` in the selected storage media folder subdirectory `Exports`.

![Global settings](GlobalSettings_AppGlobalSettingsSmall.png)
