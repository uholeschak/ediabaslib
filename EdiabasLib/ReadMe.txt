EdiabsLib Test SSL:
-------------------
CarSimulator: e61.txt
Parameter: --ifh="ENET" EnetRemoteHost=127.0.0.1;EnetNetworkProtocol=SSL;SslSecurityPath=C:\EDIABAS\Security\SSL_Truststore;S29CertPath=C:\EDIABAS\Security\S29\Certs;EnetSslPort=13496;EnetVehicleProtocol=DoIP;EnetDoipGatewayAddress=FFFF;IfhTrace=2;ApiTrace=1 -s "C:\EDIABAS\Ecu\d_motor.grp" -j "_VERSIONINFO" -j "FS_LESEN" -j "FS_LESEN_DETAIL#0x4232#F_ART_ANZ;F_UW_ANZ" -j "FS_LESEN_DETAIL#0x4232#F_art_anz;F_uw_anz" -j "FS_LOESCHEN"
