For NET9 up to date build tools are required.
Visual Studio is updating the workload very late, so a manual update is required before the build:

donet workload update

If there are unistall errors of old workloads, uninstall has to be executed manually:
https://gist.github.com/jonathanpeppers/63beb491a9185ac06710261536cc35c9

reg query HKLM\SOFTWARE\Microsoft\Windows\currentversion\uninstall\ -s -f <failure package name>
reg query HKLM\SOFTWARE\Microsoft\Windows\currentversion\uninstall\ -s -f <failure package version>

Result output:
HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\currentversion\uninstall\<GUID>

Uninstall MSI with GUID manually:
msiexec /x <GUID> IGNOREDEPENDENCIES=ALL

In the Assets folder the encrypted ECU package Ecu.bin has to be copied before the build.
The ECU package could be extracted from an existing .aab file (which is a zip) at the location assets1\assets\Ecu.bin.

When modifying Xml\Sample\* files execute Scripts\CreateAssetZips.bat to update the Assets\Sample.zip.
