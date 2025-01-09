For NET9 an up to date framework is required.
Visual studio is updating the workload very late, to a manual update is required:

donet workload update

If there are unstall errors of old workload, uninstall has to be executed manually:
https://gist.github.com/jonathanpeppers/63beb491a9185ac06710261536cc35c9

reg query HKLM\SOFTWARE\Microsoft\Windows\currentversion\uninstall\ -s -f <package name>
reg query HKLM\SOFTWARE\Microsoft\Windows\currentversion\uninstall\ -s -f <package version>

Result:
HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\currentversion\uninstall\<GUID>

Uninstall GUID manually:
msiexec /x <GUID> /q IGNOREDEPENDENCIES=ALL
