IONOS Validation plugin for use with https://www.win-acme.com/ client.
Authentication settings are used from User.config to simplify command line.
Don't copy to Acme folder because the DLL is automatically parsed at this location!

Command line arguments (which is the Acme default):
create {Identifier} {RecordName} {Token}
delete {Identifier} {RecordName}

User.config example:
<?xml version="1.0" encoding="utf-8"?>
<appSettings>
    <add key="Prefix" value="Auth prefix"/>
    <add key="Key" value="Auth key"/>
</appSettings>
