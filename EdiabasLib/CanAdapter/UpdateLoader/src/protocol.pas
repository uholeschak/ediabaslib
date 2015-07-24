(*#######################################################################################

UpdateLoader - Protokollinformationen und Standardeinstellungen

http://www.leo-andres.de/

Copyright (C) 2012 Leo-Andres Hofmann


This file is part of UpdateLoader.

UpdateLoader is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

UpdateLoader is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with UpdateLoader. If not, see <http://www.gnu.org/licenses/>.

#######################################################################################*)
unit protocol;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, FileUtil, Forms, Controls, Graphics, Dialogs, ComCtrls,
  ExtCtrls, EditBtn, Buttons, LCLType, LCLIntf, Tools, SynaSer;

const
  //Standardeinstellungen
  defaultConnectTries: integer = 250;
  defaultBootloaderPassword: string = 'Peda';
  defaultDeviceName: string = 'Unbekannt';
  defaultBaudrate: integer = 7; //Eintrags-Nr. in der ComboBox

  //Einstellungen für die Übertragung
  settingValidBootloader: string = '2.1';
  settingConnectTryTimeout: integer = 25; //Wartezeit auf Antwort beim Verbinden (ms)
  settingPortSetupDelay: integer = 500; //Wartezeit beim Öffnen des COM-Ports (ms)
  settingResetPostDelay: integer = 150; //Wartezeit nach Reset-Auslösung über Kommando (ms)
  settingConnectMessageDelay: integer = 150; //Wartezeit zwischen GUI-Anzeigen (ms)
  settingReadTimeout: integer = 250; //Allgemeine Wartezeit auf serielle Daten (ms)
  settingReadLongTimeout: integer = 750; //Wartezeit auf "Continue" beim Flashen, "Success" beim Verbinden (ms)
  settingConnectLoopTimeout: integer = 100; //Maximale Laufzeit der Empfangsschleife während Verbindungsaufbau (ms)
  settingSyncDelay: integer = 100; //Wartezeit vor Synchronisations-Byte nach Passwort
  settingSyncLoopTimeout: integer = 3000; //Maximale Laufzeit Synchronisation nach Passwort (ms)
  settingMaximumBufferSize: longint = 262144;
  settingVerifyChunkLength: integer = 512;
  settingMinimumWriteBuffer: integer = 8; //Minimaler Sendepuffer
  settingSmallWriteBuffer: integer = 128; //Warnmeldung "langsame Übertragung"

type
  TBootloaderRoutines = class
    constructor Create;
    destructor Destroy; override;
    private
      const
         deviceNames: array[0..22] of string = (
           '1E 90 07:ATtiny13',
           '1E 91 0A:ATtiny2313',
           '1E 92 05:ATmega48',
           '1E 92 06:ATtiny45',
           '1E 92 07:ATtiny44',
           '1E 92 08:ATtiny461',
           '1E 93 06:ATmega8515',
           '1E 93 07:ATmega8',
           '1E 93 08:ATmega8535',
           '1E 93 0A:ATmega88',
           '1E 93 0B:ATtiny85',
           '1E 93 0C:ATtiny84',
           '1E 93 0D:ATtiny861',
           '1E 93 0F:ATmega88P',
           '1E 94 03:ATmega16',
           '1E 94 04:ATmega162',
           '1E 94 06:ATmega168',
           '1E 94 0B:ATmega168P',
           '1E 95 01:ATmega323',
           '1E 95 02:ATmega32',
           '1E 95 0F:ATmega328P',
           '1E 96 09:ATmega644',
           '1E 98 02:ATmega2561');

         //Bootloader-Kommunikation
         blcIdentifierCommand: byte = $A5;
         blcIdentifierEscape: byte = $A5;
         blcIdentifierAnswer: byte = $A8;
         blcIdentifierContinue: byte = $A9;

         blcReadRevision: byte = $0;
         blcReadBufferSize: byte = $1;
         blcReadSignature: byte = $2;
         blcReadFlashSize: byte = $3;

         blcCommandProgramWrite: byte = $4;
         blcCommandProgramStart: byte = $5;
         blcCommandProgramCheckCRC: byte = $6;
         blcCommandProgramVerify: byte = $7;
         blcCommandProgramFinish: byte = $80;

         blcStatusConnected: byte = $A6;
         blcStatusBadCommand: byte = $A7;
         blcStatusSuccess: byte = $AA;
         blcStatusFail: byte = $AB;

         blcDataIllegalChar: byte = $13;
         blcDataIllegalCharShift: byte = $80;
         blcDataAutobaudLeader: byte = $0D; //Steuerzeichen Baudraten-Erkennung, einmalig vor Passwort
         blcDataPasswordTrailer: byte = $FF;

      var
        serialConnection: TBlockSerial;
        connectionCRC: word;
        oneWire: boolean;
        failureAddress: longint; //Zuletzt bearbeiter Speicherbereich

      procedure serialSendByte(data: byte);
      procedure serialSendString(data: string);

      procedure calculateCRC(data: byte);
      procedure skipOneWireBytes(count: integer);

      function detectSupport(command: byte): boolean;
      function readInfo(readCommand: integer): TByteList;
      function uploadData(targetCommand: byte; waitForContinue: boolean; buffer: TMemoryBuffer; bufferLength: longint; chunkLength: longint; progressBar: TProgressBar): boolean;

      function getBufferChunk(buffer: TMemoryBuffer; var startPosition: longint; chunkLength: longint; bufferMaxLength: longint): AnsiString;

    public
      procedure assignSerialConnection(connection: TBlockSerial);
      procedure setWireMode(mode: boolean);
      procedure resetCRC;

      //Rückgabe: 1/2 für 1/2-Draht-Verbindung, verbrauchte Anzahl Verbindungsversuche wird in connectTries gesetzt
      function connect(bootloaderPassword: string; var connectTries: integer; sendReset: boolean = true; detectOneWire: boolean = true; progressBar: TProgressBar = nil): integer;

      function writeFirmware(buffer: TMemoryBuffer; bufferLength: longint; chunkLength: longint; progressBar: TProgressBar = nil): boolean;
      function verifyFirmware(buffer: TMemoryBuffer; bufferLength: longint; progressBar: TProgressBar = nil): boolean;
      function startFirmware: boolean;

      function checkCRC: boolean;

      function detectVerifySupport: boolean;
      function detectCRCSupport: boolean;

      function readRevision: string;
      function readSignature: string;
      function readBufferSize: integer;
      function readFlashSize: longint;

      function getFailureAddress: string;
      function getDeviceName(signature: string): string;
  end;

implementation

uses DateUtils;

{$REGION 'Initialisierung'}
constructor TBootloaderRoutines.Create;
begin
  inherited;

  connectionCRC := 0;
  oneWire := false;
  failureAddress := 0;

  serialConnection := nil;
end;

destructor TBootloaderRoutines.Destroy;
begin
  inherited;
end;

procedure TBootloaderRoutines.assignSerialConnection(connection: TBlockSerial);
begin
  self.serialConnection := connection;
end;

procedure TBootloaderRoutines.setWireMode(mode: boolean);
begin
  self.oneWire := mode;
end;

procedure TBootloaderRoutines.resetCRC;
begin
  self.connectionCRC := 0;
end;
{$ENDREGION}

{$REGION 'Hauptfunktionen (Übertragen, Verbinden)'}
function TBootloaderRoutines.connect(bootloaderPassword: string; var connectTries: integer; sendReset: boolean = true; detectOneWire: boolean = true; progressBar: TProgressBar = nil): integer;
const
   ResetCmd: array[1..6] of Byte = ($82, $F1, $F1, $FF, $FF, $62);
var
  connectTry: integer;
  wireMode: byte;
  wireDetectChar: byte;
  reply: byte;
  connectionEstablished, loopTimeout: boolean;
  startTime: TDateTime;
begin
  wireMode := 0;
  wireDetectChar := ord(bootloaderPassword[1]);    //Erstes Zeichen im Passwort wird im 1-Wire Betrieb zurückgesendet
  connectionEstablished := false;
  Result := 0;

  //Auf gültiges Passwort prüfen:
  //Steuerzeichen "Connected" und "Success" dürfen nicht im Passwort vorkommen
  if (pos(AnsiChar(blcStatusConnected), bootloaderPassword) <> 0) or (pos(AnsiChar(blcStatusSuccess), bootloaderPassword) <> 0) then
    raise Exception.Create('Ungültiges Passwort, es dürfen keine Protokollzeichen im Passwort vorkommen!');

  if Assigned(progressBar) then
  begin
    progressBar.Position := 0;
    progressBar.Max := connectTries;
    progressBar.Step := 1;
  end;

  //Send reset to the application
  if sendReset then
  begin
    serialConnection.SendBuffer(@ResetCmd, Length(ResetCmd));
    Sleep(settingResetPostDelay);
  end;

  //Puffer leeren
  serialConnection.Purge;
  serialConnection.Flush;

  reply := 0;
  loopTimeout := false;

  for connectTry := 0 to connectTries do
  begin
{$IF Declared(blcDataAutobaudLeader)}
    //0x0D zur Baudraten-Erkennung senden
    serialSendByte(blcDataAutobaudLeader);
{$ENDIF}

    //Passwort + 0xFF senden
    serialSendString(bootloaderPassword);
    serialSendByte(blcDataPasswordTrailer);

    try
      //Gesamten empfangenen Puffer durchlaufen
      //Schleife mit Timeout, permante Übertragungen blockieren die Empfangsschleife
      startTime := TDateTime(now);
      repeat
        reply := serialConnection.RecvByte(settingConnectTryTimeout);

        if (reply = wireDetectChar) and (detectOneWire) then
           wireMode := 1;
        if reply = blcStatusConnected then
           connectionEstablished := true;

        if MilliSecondsBetween(now, startTime) > settingConnectLoopTimeout then
        begin
          loopTimeout := true;
          Break;
        end;
      until not serialConnection.CanReadEx(settingConnectTryTimeout);
    except
    end;

    if connectionEstablished = true then
      Break;

    if Assigned(progressBar) then
      progressBar.StepIt;
    Application.ProcessMessages;
  end;

  //Genutzte Anzahl Verbindungsversuche zurückmelden
  connectTries := connectTry + 1;

  //keine 1-Draht Verbindung oder Erkennung deaktiviert -> 2-Draht
  if (wireMode <> 1) or (not detectOneWire) then
    wireMode := 2;

  //Puffer leeren
  serialConnection.Purge;
  serialConnection.Flush;

  //Passwort wurde akzeptiert, Umschaltung in Kommandomodus:
  //Nach dem Empfang von blcStatusSuccess ist die Verbindung synchron
  if connectionEstablished = true then
  begin
    //0xA5 senden, Bootloader antwortet mit Status "Success"
    //Nach dem Verbindungsaufbau wird das Sychronisationsbyte verzögert
    //gesendet um es eindeutig vom Passwort zu trennen
    Sleep(settingSyncDelay);
    serialSendByte(blcIdentifierCommand);

    reply := 0;
    loopTimeout := false;

    try
      //Gesamten empfangenen Puffer durchlaufen, 1-Wire Daten werden ignoriert
      //Schleife mit Timeout, permante Übertragungen blockieren die Empfangsschleife
      startTime := TDateTime(now);
      repeat
        reply := serialConnection.RecvByte(settingReadTimeout);

        if MilliSecondsBetween(now, startTime) > settingSyncLoopTimeout then
        begin
          loopTimeout := true;
          Break;
        end;
      until reply = blcStatusSuccess;
    except
    end;

    //Puffer leeren
    serialConnection.Purge;
    serialConnection.Flush;

    if loopTimeout then
      raise Exception.Create('Zeitüberschreitung nach Verbindungsaufbau, Synchronisation fehlgeschlagen')
    else if reply <> blcStatusSuccess then
      raise Exception.CreateFmt('Ungültige Antwort des Bootloaders (0x%.2x) ' + sLineBreak + 'Verbindung aufgebaut, Synchronisation fehlgeschlagen', [reply]);
  end
  else
  begin
    if loopTimeout then
      raise Exception.Create('Zeitüberschreitung' + sLineBreak + 'Verbindungsaufbau durch laufende Kommunikation blockiert')
    else
      raise Exception.Create('Zeitüberschreitung');
  end;

  Result := wireMode;

  if Assigned(progressBar) then
    progressBar.Position := progressBar.Max;
  Application.ProcessMessages;
end;

function TBootloaderRoutines.writeFirmware(buffer: TMemoryBuffer; bufferLength: longint; chunkLength: longint; progressBar: TProgressBar = nil): boolean;
begin
  Result := uploadData(blcCommandProgramWrite, true, buffer, bufferLength, chunkLength, progressBar);
end;

function TBootloaderRoutines.verifyFirmware(buffer: TMemoryBuffer; bufferLength: longint; progressBar: TProgressBar = nil): boolean;
begin
  Result := uploadData(blcCommandProgramVerify, false, buffer, bufferLength, settingVerifyChunkLength, progressBar);
end;

function TBootloaderRoutines.startFirmware: boolean;
begin
  Result := true;

  try
    serialSendByte(blcIdentifierCommand);
    serialSendByte(blcCommandProgramStart);

    skipOneWireBytes(2);

    try
      if serialConnection.RecvByte(settingReadTimeout) = blcStatusBadCommand then
        Result := false;
    except
    end;

  finally
    serialConnection.Purge;
    serialConnection.Flush;
  end;
end;
{$ENDREGION}

{$REGION 'CRC-Prüfsumme'}
function TBootloaderRoutines.checkCRC: boolean;
var
  reply: byte;
begin
  Result := false;

  try
    serialSendByte(blcIdentifierCommand);
    serialSendByte(blcCommandProgramCheckCRC);
    serialSendString(AnsiChar(lo(connectionCRC)) + AnsiChar(hi(connectionCRC)));

    skipOneWireBytes(4);

    try
      reply := serialConnection.RecvByte(settingReadTimeout);
    except
      raise Exception.Create('Zeitüberschreitung');
    end;

  finally
    serialConnection.Purge;
    serialConnection.Flush;
  end;

  if (reply <> blcStatusSuccess) and (reply <> blcStatusFail) then
    raise Exception.Create('Abschluss der Übertragung nicht bestätigt');

  if reply = blcStatusSuccess then
    Result := true;
end;
{$ENDREGION}

{$REGION 'Informationen auslesen'}
function TBootloaderRoutines.detectVerifySupport: boolean;
begin
  Result := detectSupport(blcCommandProgramVerify);
end;

function TBootloaderRoutines.detectCRCSupport: boolean;
begin
  Result := detectSupport(blcCommandProgramCheckCRC);
end;

function TBootloaderRoutines.readRevision: string;
var
  readBuffer: string;
  readBytes: TByteList;
begin
  Result := '';

  readBytes := readInfo(blcReadRevision);
  readBuffer := convertByteList2String(readBytes);

  if length(readBuffer) = 2 then
    readBuffer := readBuffer[1] + '.' + readBuffer[2]
  else
    raise Exception.Create('Ungültiger Datensatz empfangen (Revision)');

  Result := readBuffer;
end;

function TBootloaderRoutines.readSignature: string;
var
  readBuffer: string;
  readBytes: TByteList;
begin
  Result := '';

  readBytes := readInfo(blcReadSignature);
  readBuffer := convertByteList2HexString(readBytes, true);

  Result := readBuffer;
end;

function TBootloaderRoutines.readBufferSize: integer;
var
  readBytes: TByteList;
begin
  Result := 0;

  readBytes := readInfo(blcReadBufferSize);

  if high(readBytes) = 1 then
    Result := readBytes[1] + (readBytes[0] * 256)
  else
    raise Exception.Create('Ungültiger Datensatz empfangen (Sendpuffer-Größe)');
end;

function TBootloaderRoutines.readFlashSize: longint;
var
  readBytes: TByteList;
begin
  Result := 0;

  readBytes := readInfo(blcReadFlashSize);

  if high(readBytes) = 2 then
    Result := readBytes[2] + (readBytes[1] * 256) + (readBytes[0] * 65536)
  else
    raise Exception.Create('Ungültiger Datensatz empfangen (Flash-Größe)');
end;

function TBootloaderRoutines.getFailureAddress: string;
begin
  Result := IntToHex(failureAddress, 6);
end;

function TBootloaderRoutines.getDeviceName(signature: string): string;
var
  position: integer;
  device: string;
begin
  device := '';
  Result := defaultDeviceName;

  for position := low(deviceNames) to high(deviceNames) do
  begin
    if signature = copy(deviceNames[position], 1, 8) then
    begin
      device := copy(deviceNames[position], 10, length(deviceNames[position]));
      Break;
    end;
  end;

  if device <> '' then
     Result := device;
end;
{$ENDREGION}

{$REGION 'Private Funktionen'}
{$REGION 'Bitübertragung'}
procedure TBootloaderRoutines.serialSendByte(data: byte);
begin
  serialConnection.SendByte(data);

  calculateCRC(data);
end;

procedure TBootloaderRoutines.serialSendString(data: AnsiString);
var
  position: integer;
begin
  serialConnection.SendString(data);

  for position := 1 to length(data) do
      calculateCRC(ord(data[position]));
end;

procedure TBootloaderRoutines.calculateCRC(data: byte);
var
  count: integer;
begin
  for count := 0 to 7 do
  begin
    if ((data and $01) xor (connectionCRC and $0001) <> 0) then
    begin
      connectionCRC := connectionCRC shr 1;
      connectionCRC := connectionCRC xor $A001;
    end
    else
    begin
      connectionCRC := connectionCRC shr 1;
    end;

    data := data shr 1;
  end;
end;

procedure TBootloaderRoutines.skipOneWireBytes(count: integer);
begin
  if oneWire = true then
  begin
    try
      serialConnection.RecvBufferStr(count, settingReadTimeout);
    except
    end;
  end;
end;
{$ENDREGION}

function TBootloaderRoutines.detectSupport(command: byte): boolean;
var
  reply: byte;
begin
  Result := false;

  try
    serialSendByte(blcIdentifierCommand);
    serialSendByte(command);

    skipOneWireBytes(2);

    //Unbekannte Kommandos lehnt der Bootloader sofort ab
    //Bei einem gültigen Kommando läuft die Empfangsfunktion ins Timeout, der Bootloader
    //erwartet jetzt weitere Daten zum aktivierten Kommando
    try
      if serialConnection.RecvByte(settingReadTimeout) = blcStatusBadCommand then
      begin
        Result := false;
        exit;
      end;
    except
    end;

    //Kommando sofort wieder beenden, hier nur prüfen
    serialSendByte(blcIdentifierCommand);
    serialSendByte(blcCommandProgramFinish);

    skipOneWireBytes(2);

    try
      reply := serialConnection.RecvByte(settingReadTimeout);
      if (reply = blcStatusSuccess) or (reply = blcStatusFail) then
         Result := true;
    except
    end;

  finally
    serialConnection.Purge;
    serialConnection.Flush;
  end;
end;

function TBootloaderRoutines.readInfo(readCommand: integer): TByteList;
var
  answerLength: byte;
  readAnswer: TByteList;
  readLeader: byte;
  readTrailer: byte;
  answerPosition: integer;
begin
  //Daten anfordern
  serialSendByte(blcIdentifierCommand);
  serialSendByte(readCommand);

  skipOneWireBytes(2);

  try
    try
      readLeader := serialConnection.RecvByte(settingReadTimeout);

      answerLength := serialConnection.RecvByte(settingReadTimeout);

      if answerLength > 0 then
      begin
        //Länge abzüglich Bestätigungscode
        dec(answerLength);
        SetLength(readAnswer, answerLength);

        for answerPosition := 0 to pred(answerLength) do
          readAnswer[answerPosition] := serialConnection.RecvByte(settingReadTimeout);
      end;

      readTrailer := serialConnection.RecvByte(settingReadTimeout);
    except
      raise Exception.Create('Zeitüberschreitung');
    end;

    if (readLeader <> blcIdentifierAnswer) or (readTrailer <> blcStatusSuccess) then
      raise Exception.Create('Ungültige Daten empfangen');
  finally
    serialConnection.Purge;
    serialConnection.Flush;
  end;

  Result := readAnswer;
end;

function TBootloaderRoutines.uploadData(targetCommand: byte; waitForContinue: boolean; buffer: TMemoryBuffer; bufferLength: longint; chunkLength: longint; progressBar: TProgressBar): boolean;
var
  chunkCount: longint;
  chunkLeftover: longint;
  chunkData: AnsiString;
  chunkPosition: longint;
  currentChunk: longint;
  reply: byte;
begin
  chunkPosition := low(buffer);
  currentChunk := 0;
  Result := false;

  chunkCount := bufferLength div chunkLength;
  chunkLeftover := bufferLength - ((chunkCount) * chunkLength);
  if chunkLeftover <> 0 then
    inc(chunkCount);

  if Assigned(progressBar) then
  begin
    progressBar.Position := 0;
    progressBar.Max := chunkCount;
    progressBar.Step := 1;
  end;

  //Programmieren starten
  serialSendByte(blcIdentifierCommand);
  serialSendByte(targetCommand);

  skipOneWireBytes(2);

  try
    while currentChunk <= chunkCount do
    begin
      //Datenblock laden, Zählerstände setzen
      chunkData := getBufferChunk(buffer, chunkPosition, chunkLength, bufferLength);
      inc(currentChunk);
      failureAddress := chunkPosition;

      //Datenblock senden
      serialSendString(chunkData);

      skipOneWireBytes(Length(ChunkData));

      //Nach der Übertragung aller Chunks prüfen, ob alle Daten gesendet wurden
      //Ja: Schleife beenden, Antwort wird nicht abgewartet
      //Nein: Durch Füllzeichen stimmte die Berechnung nicht, noch einen Chunk senden
      if currentChunk = chunkCount then
      begin
        if chunkPosition < bufferLength then
          inc(chunkCount)
        else
          Break;
      end;

      //Auf "Fortsetzen"-Kommando warten
      if waitForContinue = true then
      begin
        try
          reply := serialConnection.RecvByte(settingReadLongTimeout);
        except
          raise Exception.Create('Zeitüberschreitung');
        end;

        if reply <> blcIdentifierContinue then
          raise Exception.Create('Ungültige Antwort empfangen');
      end;

      if Assigned(progressBar) then
        progressBar.StepIt;
      Application.ProcessMessages;
    end;

    //Programmieren beenden
    serialSendByte(blcIdentifierCommand);
    serialSendByte(blcCommandProgramFinish);

    skipOneWireBytes(2);

    try
      reply := serialConnection.RecvByte(settingReadLongTimeout);
    except
      raise Exception.Create('Zeitüberschreitung');
    end;

    if (reply <> blcStatusSuccess) and (reply <> blcStatusFail) then
      raise Exception.Create('Abschluss der Übertragung nicht bestätigt');
  finally
    serialConnection.Purge;
    serialConnection.Flush;
  end;

  if reply = blcStatusSuccess then
    Result := true;

  if Assigned(progressBar) then
    progressBar.Position := progressBar.Max;
  Application.ProcessMessages;
end;

function TBootloaderRoutines.getBufferChunk(buffer: TMemoryBuffer; var startPosition: longint; chunkLength: longint; bufferMaxLength: longint): AnsiString;
var
  convert: AnsiString;
  convertCount: longint;
begin
  convert := '';
  convertCount := 0;
  Result := '';

  if (startPosition < 0) or (startPosition < low(buffer)) then
    startPosition := low(buffer);
  if startPosition > bufferMaxLength then
    startPosition := bufferMaxLength;
  if (chunkLength + startPosition) > bufferMaxLength then
    chunkLength := bufferMaxLength - startPosition;
  if (chunkLength <= 0) then
    Exit;

  while convertCount < chunkLength do
  begin
    if (buffer[startPosition] = blcDataIllegalChar) or (buffer[startPosition] = blcIdentifierEscape) then
    begin
      convert := convert + AnsiChar(blcIdentifierEscape);
      convert := convert + AnsiChar(buffer[startPosition] + blcDataIllegalCharShift);
    end
    else
      convert := convert + AnsiChar(buffer[startPosition]);

    inc(convertCount);
    inc(startPosition);

    if startPosition > bufferMaxLength then
      Break;
  end;

  Result := convert;
end;
{$ENDREGION}

end.

