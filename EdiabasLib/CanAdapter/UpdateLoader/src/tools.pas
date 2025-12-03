(*#######################################################################################

UpdateLoader - Hilfsfunktionen

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

unit tools;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, FileUtil, Forms, Controls, Graphics, Dialogs, ComCtrls,
  Grids, ExtCtrls, EditBtn, Buttons, LCLType, LCLIntf, Registry, LazUtf8;

type
  TMemoryBuffer = array of byte;
  TByteList = array of byte;

function getComportCount: integer;
procedure getComportNames(portNames: TStrings);
function getComportFriendlyName(regKey: string; port: string): string;

procedure showProgramMemory(memoryView: TStringGrid; buffer: TMemoryBuffer; bufferLength: longint; progressBar: TProgressBar = nil);
function loadProgramFile(fileName: string; buffer: TMemoryBuffer; bufferLength: longint; var usedBuffer: longint): boolean;
procedure clearBuffer(buffer: TMemoryBuffer);

function convertByteList2String(data: TByteList; separateBytes: boolean = false): string;
function convertByteList2HexString(data: TByteList; separateBytes: boolean = false): string;
function convertKB2String(kb: longint; appendUnit: boolean = true): string;

implementation

uses
  Protocol, RegExpr;

{$REGION 'Com-Ports auflisten'}
function portIndexCompare(List: TStringList; Index1, Index2: Integer): Integer;
begin
  if integer(List.Objects[Index1]) < integer(List.Objects[Index2]) then
    Result := -1
  else if integer(List.Objects[Index1]) > integer(List.Objects[Index2]) then
    Result := 1
  else
    Result := 0;
end;

function getComportCount: integer;
var
  registry: TRegistry;
  keyInfo: TRegKeyInfo;
begin
  Result := -1;
  registry := TRegistry.Create;

  try
    registry.RootKey := HKEY_LOCAL_MACHINE;
    if registry.OpenKeyReadOnly('HARDWARE\DEVICEMAP\SERIALCOMM') then
    begin
      registry.GetKeyInfo(keyInfo);
      Result := keyInfo.NumValues;
    end;
  finally
    registry.CloseKey;
    registry.Free;
  end;
end;

procedure getComportNames(portNames: TStrings);
var
  registry: TRegistry;
  regKeyList, portNameList: TStringList;
  portKey: string;
  portName, nameString:string;
  nameLength: integer;
  friendlyName: string;
  portIndex: integer;
  indexRegex: TRegExpr;
begin
  registry := TRegistry.Create(KEY_READ or $0100); ;
  regKeyList := TStringList.Create;
  portNameList := TStringList.Create;
  indexRegex := TRegExpr.Create;

  indexRegex.Expression := 'COM\s*?(0*[1-9][0-9]*)'; //Portnummer "1" aus "COM1" lesen

  regKeyList.Clear;
  portNameList.Clear;

  portNames.BeginUpdate;
  try
    portNames.Clear;

    registry.Access := KEY_READ;
    registry.RootKey := HKEY_LOCAL_MACHINE;
    if registry.OpenKeyReadOnly('HARDWARE\DEVICEMAP\SERIALCOMM') then
    begin
      registry.GetValueNames(regKeyList);

      for portKey in regKeyList do
      begin
        nameLength := registry.GetDataSize(portKey);
        nameString := registry.ReadString(portKey);

        //Nicht terminierte Strings abfangen
        setlength(portName, nameLength + 1);
        fillChar(portName[1], nameLength + 1, #0);
        move(nameString[1], portName[1], nameLength);
        portName := trim(portName);

        friendlyName := getComportFriendlyName('\SYSTEM\CurrentControlSet\Enum\', portName);
        indexRegex.Exec(portName);

        if not TryStrToInt(indexRegex.Match[1], portIndex) then
           Continue;

        if friendlyName <> '' then
          portNameList.AddObject(Format('COM %d: %s', [portIndex, friendlyName]), TObject(portIndex))
        else
          portNameList.AddObject(Format('COM %d', [portIndex]), TObject(portIndex));
      end;

      portNameList.CustomSort(@portIndexCompare);
      portNames.Assign(portNameList);
    end;
  finally
    registry.CloseKey;

    registry.Free;
    regKeyList.Free;
    portNameList.Free;
    indexRegex.Free;

    portNames.EndUpdate;
  end;
end;

function getComportFriendlyName(regKey: string; port: string): string;
var
  registry: TRegistry;
  keyNameList: TStringList;
  keyName, currentKey: string;
  friendlyName: string;
  foundKeyIn: string;
begin
  Result := '';
  registry := TRegistry.Create;
  keyNameList := TStringList.Create;

  keyNameList.Clear;

  registry.RootKey := HKEY_LOCAL_MACHINE;
  registry.OpenKeyReadOnly(regKey);
  registry.GetKeyNames(keyNameList);
  registry.CloseKey;

  try
    for keyName in keyNameList do
    begin
      currentKey := regKey + keyName + '\';

      if registry.OpenKeyReadOnly(currentKey + 'Device Parameters') then
      begin
        if registry.ReadString('PortName') = port then
        begin
          registry.CloseKey;
          registry.OpenKeyReadOnly(currentKey);
          friendlyName := SysToUTF8(registry.ReadString('FriendlyName'));
          registry.CloseKey;

          foundKeyIn := copy(currentKey, 32, length(currentKey) - 32);
          foundKeyIn := copy(foundKeyIn, 0, pos('\', foundKeyIn) - 1);

          if foundKeyIn <> '' then
            friendlyName := friendlyName + ' - (' + foundKeyIn + ')';

          Break;
        end;
      end
      else
      begin
        if registry.OpenKeyReadOnly(currentKey) and registry.HasSubKeys then
        begin
          friendlyName := getComportFriendlyName(currentKey, port);
          registry.CloseKey;
          if friendlyName <> '' then
            Break;
        end;
      end;
    end;

    Result := friendlyName;
  finally
    registry.CloseKey;

    registry.Free;
    keyNameList.Free;
  end;
end;
{$ENDREGION}

{$REGION 'Hex-Datei anzeigen und auslesen'}
procedure showProgramMemory(memoryView: TStringGrid; buffer: TMemoryBuffer; bufferLength: longint; progressBar: TProgressBar = nil);
var
  bufferPosition: longint;
  byteCount: integer;
  dataHex, dataChr, adressChr: string;
begin
  byteCount := 1;
  dataHex := '';
  dataChr := '';

  if Assigned(progressBar) then
  begin
    progressBar.Position := 0;
    progressBar.Max := bufferLength div 100;
    progressBar.Step := 1;
  end;

  memoryView.Hide;
  memoryView.RowCount := 2;

  memoryView.ColWidths[0] := 55;
  memoryView.ColWidths[1] := 170;
  memoryView.ColWidths[2] := 15;
  memoryView.ColWidths[3] := 175;
  memoryView.ColWidths[4] := 120;

  memoryView.Cells[0, 0] := 'Adresse';
  memoryView.Cells[1, 0] := 'Low-Bytes';
  memoryView.Cells[3, 0] := 'High-Bytes';
  memoryView.Cells[4, 0] := 'Daten';

  for bufferPosition := low(buffer) to pred(bufferLength) do
  begin
    dataHex := dataHex + IntToHex(buffer[bufferPosition], 2) + ' ';

    if buffer[bufferPosition] <> 0 then
      dataChr := dataChr + AnsiChar(buffer[bufferPosition])
    else
      dataChr := dataChr + ' ';

    //Komplette Zeile oder Pufferende erreicht (siehe Schleifenkopf)
    if (byteCount = 16) or (bufferPosition = pred(bufferLength)) then
    begin
      adressChr := IntToHex(bufferPosition - 15, 6);

      memoryView.Cells[0, memoryView.RowCount - 1] := adressChr;
      memoryView.Cells[4, memoryView.RowCount - 1] := dataChr;

      if Length(dataHex) >= 24 then
      begin
        memoryView.Cells[1, memoryView.RowCount - 1] := copy(dataHex, 1, 24);
        memoryView.Cells[3, memoryView.RowCount - 1] := copy(dataHex, 25, Length(dataHex) - 1);
        memoryView.Cells[2, memoryView.RowCount - 1] := '-';
      end
      else
        memoryView.Cells[1, memoryView.RowCount - 1] := copy(dataHex, 1, Length(dataHex) - 1);

      memoryView.RowHeights[memoryView.RowCount - 1] := 18;
      memoryView.RowCount := memoryView.RowCount + 1;

      dataHex := '';
      dataChr := '';
      byteCount := 0;
    end;

    inc(byteCount);

    if Assigned(progressBar) and ((bufferPosition mod 100) = 0) then
       progressBar.StepIt;
  end;

  memoryView.RowCount := memoryView.RowCount - 1;
  memoryView.Show;

  if Assigned(progressBar) then
     progressBar.Position := 0;
end;

{$R-}
function loadProgramFile(fileName: string; buffer: TMemoryBuffer; bufferLength: longint; var usedBuffer: longint): boolean;
var
  programFile: TStringList;
  programFileEnum: TStringsEnumerator;
  readLine, temp: string;
  data, checksum, byteCount, dataLength: byte;
  address: word;
  memoryAddress, segmentAddress: longint;
  segmentedAdress: boolean;
  recordType, lineChecksum: byte;
begin
  Result := false;
  checksum := 0;
  lineChecksum := 0;
  segmentAddress := 0;
  segmentedAdress := false;

  clearBuffer(buffer);
  usedBuffer := 0;

  programFile := TStringList.Create;

  try
    programFile.LoadFromFile(UTF8toAnsi(fileName));
    programFileEnum := programFile.GetEnumerator;

    while programFileEnum.MoveNext do
    begin
      readLine := programFileEnum.GetCurrent;

      if readLine[1] = ':' then
      begin
        checksum := 0;

        //Datensatz-Länge
        temp := '$' + copy(readLine, 2, 2);
        dataLength := StrToInt(temp);
        checksum := checksum + dataLength;

        //Adresse
        temp := '$' + copy(readLine, 4, 4);
        address := StrToInt(temp);
        checksum := checksum + lo(address) + hi(address);

        if segmentedAdress = true then
           memoryAddress := address + segmentAddress
        else
           memoryAddress := address;

        //Record-Typ
        temp := '$' + copy(readLine, 8, 2);
        recordType := StrToInt(temp);
        checksum := checksum + recordType;

        //Record-Typen verarbeiten
        case recordType of
          0: //Datenblock
            begin
              if (memoryAddress + dataLength - 1) <= bufferLength then
              begin
                byteCount := 0;

                while byteCount < dataLength do
                begin
                  temp := '$' + copy(readLine, 10 + 2 * byteCount, 2);
                  data := StrToInt(temp);
                  checksum := checksum + data;

                  //Doppelte Einträge abfangen
                  if buffer[memoryAddress + byteCount] <> StrToInt('$FF') then
                  begin
                    if buffer[memoryAddress + byteCount] = data then
                      raise Exception.Create('Doppelter Eintrag in Datei, Speicherstelle überschrieben');
                  end;

                  buffer[memoryAddress + byteCount] := data;
                  inc(byteCount);
                end;

                //temp := '$' + copy(readLine, 10 + 2 * byteCount, 2);
                temp := '$' + RightStr(readLine, 2);
                lineChecksum := StrToInt(temp);

                //Belegten Speicher zählen
                if usedBuffer <= memoryAddress + byteCount then
                   usedBuffer := memoryAddress + byteCount;
              end
              else
                raise Exception.Create('Firmware größer als Pufferspeicher (' + convertKB2String(bufferLength) + ')');
            end;
          1: //Dateiende
            begin
              temp := '$' + copy(readLine, 10, 2);
              lineChecksum := StrToInt(temp);
            end;
          2: //Segment-Adresse
            begin
              segmentedAdress := true;

              temp := '$' + copy(readLine, 10, 4);
              segmentAddress := StrToInt(temp) * 16;

              temp := '$' + copy(readLine, 14, 2);
              lineChecksum := StrToInt(temp);
            end;
          else
            begin
              raise Exception.Create('Ungültiger Record (unbekannte Funktion)');
              break;
            end;
        end;

        //Prüfsumme
        checksum := lineChecksum + checksum;
        if ((checksum and $00FF) <> 0) then
          raise Exception.Create('Prüfsumme der Firmware-Datei ungültig');
      end
      else
        raise Exception.Create('Ungültiges Dateiformat (kein Intel-Hex)');
    end; //while ! EOF

    if usedBuffer <= 0 then
      raise Exception.Create('Firmware-Datei enthält keine Einträge');
  finally
    programFile.Free;
    programFileEnum.Free;
  end;

  Result := true;
end;
{$R+}

procedure clearBuffer(buffer: TMemoryBuffer);
var
  count: longint;
begin
  for count := low(buffer) to high(buffer) do
    buffer[count] := $FF;
end;
{$ENDREGION}

{$REGION 'Formatumwandlungen'}

function convertByteList2String(data: TByteList; separateBytes: boolean = false): string;
var
  count: integer;
  convert: string;
begin
  convert := '';
  Result := '';

  for count := low(data) to high(data) do
  begin
      convert := convert + IntToStr(data[count]);
      if separateBytes then
         convert := convert + ' ';
  end;

  //Überschüssiges Leerzeichen am Ende entfernen
  if separateBytes then
     convert := leftStr(convert, length(convert) - 1);

  Result := convert;
end;

function convertByteList2HexString(data: TByteList; separateBytes: boolean = false): string;
var
  count: integer;
  convert: string;
begin
  convert := '';
  Result := '';

  for count := low(data) to high(data) do
  begin
      convert := convert + IntToHex(data[count], 2);
      if separateBytes then
         convert := convert + ' ';
  end;

  //Überschüssiges Leerzeichen am Ende entfernen
  if separateBytes then
     convert := leftStr(convert, length(convert) - 1);

  Result := convert;
end;

function convertKB2String(kb: longint; appendUnit: boolean = true): string;
var
  convert: string;
begin
  convert := '';
  Result := '';

  if kb >= 1024 then
  begin
    convert := IntToStr(Round(kb / 1024));
    if appendUnit = true then
      convert := convert + 'kB';
  end
  else
  begin
    convert := IntToStr(kb);
    if appendUnit = true then
      convert := convert + ' Byte';
  end;

  Result := convert;
end;
{$ENDREGION}

end.

