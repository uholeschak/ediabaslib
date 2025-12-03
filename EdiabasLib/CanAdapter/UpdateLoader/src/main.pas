(*#######################################################################################

UpdateLoader - Hauptprogramm

Standardeinstellungen können in protocol.pas angepasst werden

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

unit main;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, FileUtil, Forms, Controls, Graphics, Dialogs, ComCtrls, FileInfo,
  Grids, StdCtrls, ExtCtrls, EditBtn, Buttons, LCLType, IniFiles,
  LCLIntf, Tools, Protocol, SynaSer, LazFileUtils;

type

  { TMainWindow }

  TMainWindow = class(TForm)
    btnUpdateControl: TButton;
    btnSelectFirmwareFilename: TButton;
    btnClearSettings: TButton;
    btnSaveSettings: TButton;
    ckbEnableDevMode: TCheckBox;
    ckbDevMiniGUI: TCheckBox;
    ckbDevOnTop: TCheckBox;
    ckbDevLockFile: TCheckBox;
    ckbDevLockPass: TCheckBox;
    ckbDevHidePass: TCheckBox;
    ckbDevHideAdv: TCheckBox;
    ckbSendResetCmd: TCheckBox;
    ckbDetectOneWire: TCheckBox;
    ckbProgramStart: TCheckBox;
    ckbShowMemory: TCheckBox;
    ckbReloadFile: TCheckBox;
    ckbProgramWrite: TCheckBox;
    ckbProgramVerify: TCheckBox;
    ckbHideMessages: TCheckBox;
    ckbAdvancedMode: TCheckBox;
    cmbComPort: TComboBox;
    cmbBaudRate: TComboBox;
    edtVersionInfo: TEdit;
    gpbMemoryView: TGroupBox;
    gpbStatus: TGroupBox;
    gpbAdvancedSettings: TGroupBox;
    gpbConnection: TGroupBox;
    gpbUpdate: TGroupBox;
    gpbInfo: TGroupBox;
    gpbAbout: TGroupBox;
    gpbSettingsFile: TGroupBox;
    lblInfo_1: TLabel;
    lblInfo_2: TLabel;
    lblBootloaderVersion: TLabel;
    lblLicense: TLabel;
    lblCopyright: TLabel;
    lblProgramConnect: TLabel;
    lblProgramWrite: TLabel;
    lblProgramVerify: TLabel;
    lblUpdateProgress: TLabel;
    lblFlashNotice: TLabel;
    lblBaudRate: TLabel;
    lblComPort: TLabel;
    edtBootloaderPassword: TLabeledEdit;
    edtFirmwareFilename: TLabeledEdit;
    edtConnectTries: TLabeledEdit;
    memUpdateStatus: TMemo;
    memUpdateLog: TMemo;
    OpenDialog: TOpenDialog;
    PageControl: TPageControl;
    pgbProgramConnect: TProgressBar;
    pgbProgramWrite: TProgressBar;
    pgbUpdateProgress: TProgressBar;
    pgbProgramVerify: TProgressBar;
    StatusBar: TStatusBar;
    stgMemoryView: TStringGrid;
    TabSheet1: TTabSheet;
    TabSheet2: TTabSheet;
    TabSheet3: TTabSheet;
    tmrFlashNotice: TTimer;
    procedure btnSaveSettingsClick(Sender: TObject);
    procedure btnSelectFirmwareFilenameClick(Sender: TObject);
    procedure btnClearSettingsClick(Sender: TObject);
    procedure btnUpdateControlClick(Sender: TObject);
    procedure ckbAdvancedModeChange(Sender: TObject);
    procedure ckbDevHidePassChange(Sender: TObject);
    procedure ckbDevMiniGUIChange(Sender: TObject);
    procedure ckbEnableDevModeChange(Sender: TObject);
    procedure ckbShowMemoryChange(Sender: TObject);
    procedure cmbComPortChange(Sender: TObject);
    procedure edtConnectTriesChange(Sender: TObject);
    procedure edtFirmwareFilenameChange(Sender: TObject);
    procedure FormClose(Sender: TObject; var CloseAction: TCloseAction);
    procedure FormCloseQuery(Sender: TObject; var CanClose: boolean);
    procedure FormCreate(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure lblCopyrightClick(Sender: TObject);
    procedure PageControlChanging(Sender: TObject; var AllowChange: boolean);
    procedure stgMemoryViewDrawCell(Sender: TObject; aCol, aRow: integer;
      aRect: TRect; aState: TGridDrawState);
    procedure TabSheet2Show(Sender: TObject);
    procedure tmrFlashNoticeTimer(Sender: TObject);
  private
    { private declarations }
  var
    updateBuffer: TMemoryBuffer;
    updateBufferValid: boolean;
    updateBufferUsed: longint;

    memoryViewUpdate: boolean;
    programmingActive: boolean;

    comportsCount: integer;
    settingsFile: string;
    settingsLoaded: boolean;
    deliveryMode: boolean;
    miniGUI: boolean;

    procedure logMessage(message: string; openMessageBox: boolean = False;
      addToLog: boolean = True; windowIcon: integer = MB_ICONEXCLAMATION);
    procedure infoMessage(message: string);
    procedure flashNotice(showNotice: boolean; message: string = '';
      flashSpeed: integer = 500);

    function doFirmwareUpdate(serialConnection: TBlockSerial;
      bootloaderRoutines: TBootloaderRoutines;
      programWrite, programVerify, programStart, showMessages: boolean;
      sendReset: boolean = True; supportOneWire: boolean = True): boolean;

    procedure sleepActive(duration: integer);

    //Einstellungen laden & sichern: Vollständiger Pfad!
    procedure loadSettings(fileName: string);
    procedure saveSettings(fileName: string; saveDeliveryMode: boolean = False);
  public
    { public declarations }
  end;

var
  MainWindow: TMainWindow;

implementation

uses Translations, GetText, Math;

{$R *.lfm}

{ TMainWindow }
{$REGION 'Programmstart/Ende'}
procedure TMainWindow.loadSettings(fileName: string);
var
  iniFile: TMemIniFile;
  selectedPage: integer;
  ItemIndex: integer;
begin
  iniFile := TMemIniFile.Create(fileName);

  try
    if not Assigned(iniFile) then
      raise Exception.Create('Datei konnte nicht geladen werden!');

    deliveryMode := iniFile.ReadBool('DeliveryMode', 'Enabled', False);

    //cmbComPort.Items speichert die Portnummer als Integer-Objekt zusätzlich zum Text
    ItemIndex := cmbComPort.Items.IndexOfObject(TObject(iniFile.ReadInteger('UpdateLoader', 'ComPort', -1)));
    if (ItemIndex <= comportsCount) and (ItemIndex >= 0) {and (not deliveryMode)} then
      cmbComPort.ItemIndex := ItemIndex;

    //ItemIndex := iniFile.ReadInteger('UpdateLoader', 'BaudRate', defaultBaudrate);
    ItemIndex := defaultBaudrate;
    cmbBaudRate.Enabled := False;
    if (ItemIndex <= cmbBaudRate.Items.Count) and (ItemIndex >= 0) then
      cmbBaudRate.ItemIndex := ItemIndex;

    ckbAdvancedMode.Checked := iniFile.ReadBool('UpdateLoader', 'AdvancedMode', False);
    ckbProgramWrite.Checked := iniFile.ReadBool('UpdateLoader', 'ProgramWrite', True);
    ckbProgramVerify.Checked := iniFile.ReadBool('UpdateLoader', 'ProgramVerify', True);
    ckbProgramStart.Checked := iniFile.ReadBool('UpdateLoader', 'ProgramStart', True);
    ckbReloadFile.Checked := iniFile.ReadBool('UpdateLoader', 'ReloadFile', True);
    ckbHideMessages.Checked := iniFile.ReadBool('UpdateLoader', 'HideMessages', False);
    ckbShowMemory.Checked := iniFile.ReadBool('UpdateLoader', 'ShowMemory', False);
    //ckbDetectOneWire.Checked := iniFile.ReadBool('UpdateLoader', 'DetectOneWire', False);
    ckbDetectOneWire.Checked := False;
    ckbDetectOneWire.Enabled := False;
    ckbSendResetCmd.Checked := iniFile.ReadBool('UpdateLoader', 'SendReset', True);

    edtBootloaderPassword.Text :=
      iniFile.ReadString('UpdateLoader', 'BootloaderPassword',
      defaultBootloaderPassword);

    //Im Paketmodus immer den Dateinamen aus der Datei laden
    if (edtFirmwareFilename.Text = '') or deliveryMode then
      edtFirmwareFilename.Text := TrimFilename(iniFile.ReadString('UpdateLoader', 'FirmwareFilename', ''));

    if iniFile.ReadInteger('UpdateLoader', 'ConnectTries',
      defaultConnectTries) > 0 then
      edtConnectTries.Text :=
        IntToStr(iniFile.ReadInteger('UpdateLoader', 'ConnectTries',
        defaultConnectTries))
    else
      edtConnectTries.Text := IntToStr(defaultConnectTries);

    selectedPage := iniFile.ReadInteger('UpdateLoader', 'Page', 0);

    //Spezialmodus zur Auslieferung (Programmpaket)
    if deliveryMode then
    begin
      //miniGUI: Verkleinerte Oberfläche ohne Status- und Loganzeige
      miniGUI := iniFile.ReadBool('DeliveryMode', 'MiniGUI', False);

      if iniFile.ReadBool('DeliveryMode', 'ModalGUI', False) then
      begin
        MainWindow.FormStyle := fsSystemStayOnTop;
      end;

      if (iniFile.ReadBool('DeliveryMode', 'LockFilename', False)) or (miniGUI) then
      begin
        edtFirmwareFilename.Width := 300;

        btnSelectFirmwareFilename.Enabled := False;
        btnSelectFirmwareFilename.Visible := False;
      end;

      if (iniFile.ReadBool('DeliveryMode', 'LockPassword', False)) or (miniGUI) then
      begin
        edtBootloaderPassword.ReadOnly := True;
      end;

      if (iniFile.ReadBool('DeliveryMode', 'HidePassword', False)) or (miniGUI) then
      begin
        edtBootloaderPassword.PasswordChar := '*';
        edtBootloaderPassword.ReadOnly := True;
      end;

      if (iniFile.ReadBool('DeliveryMode', 'HideAdvancedMode', False)) or (miniGUI) then
      begin
        ckbAdvancedMode.Enabled := False;
        ckbAdvancedMode.Visible := False;

        edtBootloaderPassword.Width := 300;

        ckbAdvancedMode.Checked := False;
      end;

      //Fehlende FW-Datei abfangen, Auswahlmöglichkeit ist nicht vorhanden
      if (miniGUI = True) or (iniFile.ReadBool('DeliveryMode',
        'LockFilename', False)) then
      begin
        if (not FileExistsUTF8(edtFirmwareFilename.Text)) or
          (length(edtFirmwareFilename.Text) <= 0) then
        begin
          logMessage('Die mitgelieferte Firmware-Datei wurde nicht gefunden.'
            + sLineBreak +
            'Wahrscheinlich wurde das Update-Paket unvollständig heruntergeladen.' +
            sLineBreak + sLineBreak + 'Das Programm wird jetzt beendet.',
            True, False, MB_ICONSTOP);

          MainWindow.Hide;
          Application.Terminate;
        end;
      end;

      //Fenstergröße anpassen bei ausgeblendetem Passwort und Modus-Checkbox
      if (miniGUI = False) and
        (iniFile.ReadBool('DeliveryMode', 'HidePassword', False)) and
        (iniFile.ReadBool('DeliveryMode', 'HideAdvancedMode', False)) then
      begin
        gpbConnection.Height := gpbConnection.Height - 45;

        edtBootloaderPassword.Enabled := False;
        edtBootloaderPassword.Visible := False;

        gpbUpdate.Top := gpbUpdate.Top - 45;
        gpbInfo.Height := gpbInfo.Height - 45;
        memUpdateLog.Height := memUpdateLog.Height - 45;
        PageControl.Height := PageControl.Height - 45;
        gpbStatus.Top := gpbStatus.Top - 45;
        MainWindow.Height := MainWindow.Height - 45;
      end;

      //Verkleinerte GUI
      if miniGUI = True then
      begin
        edtFirmwareFilename.Enabled := False;
        edtFirmwareFilename.Visible := False;
        edtBootloaderPassword.Enabled := False;
        edtBootloaderPassword.Visible := False;

        gpbConnection.Height := 75;

        gpbUpdate.Height := 75;
        gpbUpdate.Top := 85;

        btnUpdateControl.Top := 8;

        gpbInfo.Visible := False;

        PageControl.Width := 340;
        PageControl.Height := 195;

        gpbStatus.Visible := False;

        gpbAbout.Width := 320;

        MainWindow.Width := 350;
        MainWindow.Height := 230;
      end;
    end;

  finally
    iniFile.Free;
  end;

  ckbAdvancedModeChange(Self);
  ckbEnableDevModeChange(Self);
  cmbComPortChange(Self);

  if (TabSheet2.TabVisible = True) and (selectedPage = 1) and (not deliveryMode) then
    PageControl.TabIndex := 1
  else
    PageControl.TabIndex := 0;

  gpbSettingsFile.Visible := not deliveryMode;
end;

procedure TMainWindow.saveSettings(fileName: string; saveDeliveryMode: boolean = False);
var
  iniFile: TMemIniFile;
begin
  iniFile := TMemIniFile.Create(fileName);

  try
    if not Assigned(iniFile) then
      raise Exception.Create('Datei konnte nicht geladen werden!');

    iniFile.Clear;

    if cmbComPort.ItemIndex >= 0 then
      iniFile.WriteInteger('UpdateLoader', 'ComPort', integer(cmbComPort.Items.Objects[cmbComPort.ItemIndex]))
    else
      iniFile.WriteInteger('UpdateLoader', 'ComPort', -1);

    iniFile.WriteInteger('UpdateLoader', 'BaudRate', cmbBaudRate.ItemIndex);

    iniFile.WriteBool('UpdateLoader', 'AdvancedMode', ckbAdvancedMode.Checked);
    iniFile.WriteBool('UpdateLoader', 'ProgramWrite', ckbProgramWrite.Checked);
    iniFile.WriteBool('UpdateLoader', 'ProgramVerify', ckbProgramVerify.Checked);
    iniFile.WriteBool('UpdateLoader', 'ProgramStart', ckbProgramStart.Checked);
    iniFile.WriteBool('UpdateLoader', 'ReloadFile', ckbReloadFile.Checked);
    iniFile.WriteBool('UpdateLoader', 'HideMessages', ckbHideMessages.Checked);
    iniFile.WriteBool('UpdateLoader', 'ShowMemory', ckbShowMemory.Checked);
    iniFile.WriteBool('UpdateLoader', 'DetectOneWire', ckbDetectOneWire.Checked);
    iniFile.WriteBool('UpdateLoader', 'SendReset', ckbSendResetCmd.Checked);

    iniFile.WriteString('UpdateLoader', 'BootloaderPassword',
      edtBootloaderPassword.Text);
    iniFile.WriteString('UpdateLoader', 'FirmwareFilename',
      edtFirmwareFilename.Text);

    if StrToInt(edtConnectTries.Text) > 0 then
      iniFile.WriteInteger('UpdateLoader', 'ConnectTries',
        StrToInt(edtConnectTries.Text))
    else
      iniFile.WriteInteger('UpdateLoader', 'ConnectTries', defaultConnectTries);

    iniFile.WriteInteger('UpdateLoader', 'Page', PageControl.TabIndex);

    if saveDeliveryMode then
    begin
      iniFile.WriteString('UpdateLoader', 'FirmwareFilename',
        ExtractFileName(edtFirmwareFilename.Text));

      iniFile.WriteBool('DeliveryMode', 'Enabled', True);

      iniFile.WriteBool('DeliveryMode', 'MiniGUI', ckbDevMiniGUI.Checked);
      iniFile.WriteBool('DeliveryMode', 'ModalGUI', ckbDevOnTop.Checked);
      iniFile.WriteBool('DeliveryMode', 'HideAdvancedMode', ckbDevHideAdv.Checked);
      iniFile.WriteBool('DeliveryMode', 'LockFilename', ckbDevLockFile.Checked);
      iniFile.WriteBool('DeliveryMode', 'LockPassword', ckbDevLockPass.Checked);
      iniFile.WriteBool('DeliveryMode', 'HidePassword', ckbDevHidePass.Checked);
    end;

    iniFile.UpdateFile;

  finally
    iniFile.Free;
  end;
end;

procedure TMainWindow.FormShow(Sender: TObject);
var
  fileVersion: TFileVersionInfo;
  idx, dropDownWidth: integer;
begin
  try
    comportsCount := getComportCount;
  except
  end;

  if comportsCount <= 0 then
  begin
    logMessage('Dieser Computer verfügt über keine serielle Schnittstelle (COM-Port).'
      + sLineBreak +
      'Mindestens eine Schnittstelle ist notwendig um dieses Programm zu verwenden.' +
      sLineBreak + sLineBreak + 'Das Programm wird jetzt beendet.',
      True, False, MB_ICONSTOP);

    MainWindow.Hide;
    Application.Terminate;
  end;

  //Versioninformationen für Fenstertitel & Info-Seite
  try
    try
      fileVersion := TFileVersionInfo.Create(Application);
      fileVersion.fileName := Application.ExeName;
      fileVersion.ReadFileInfo;

      edtVersionInfo.Text := 'Version: ' + fileversion.VersionStrings.Values['ProductName'] +
        ' ' + fileversion.VersionStrings.Values['ProductVersion'] +
        ' (Build ' + fileversion.VersionStrings.Values['FileVersion'] + ', ' +
{$I %DATE%}
        +')';

      Application.Title := fileversion.VersionStrings.Values['ProductName'] +
        ' ' + fileversion.VersionStrings.Values['ProductVersion'];

      MainWindow.Caption := fileversion.VersionStrings.Values['ProductName'] +
        ' ' + fileversion.VersionStrings.Values['ProductVersion'] +
        ' (Build ' + fileversion.VersionStrings.Values['FileVersion'] + ')';

      lblBootloaderVersion.Caption := 'Bootloader-Version: ' + settingValidBootloader;
    finally
      fileVersion.Free;
    end;
  except
  end;

  try
    getComportNames(cmbComPort.Items);
  except
  end;

  //Größe des Dropdown-Felds erweitern, ggf. um Breite der Scrollbar erweitert
  dropDownWidth := cmbComPort.Width;
  for idx := 0 to pred(cmbComPort.Items.Count) do
    dropDownWidth := Max(dropDownWidth, cmbComPort.Canvas.TextWidth(cmbComPort.Items[idx]) + 8);

  if cmbComPort.DropDownCount < cmbComPort.Items.Count then
    Inc(dropDownWidth, GetSystemMetrics(SM_CXVSCROLL));
  SendMessage(cmbComPort.Handle, 352, EnsureRange(dropDownWidth, cmbComPort.Width, MainWindow.Width), 0); //352/xH160 = CB_SETDROPPEDWIDTH

  cmbComPort.ItemIndex := 0;
  cmbBaudRate.ItemIndex := defaultBaudrate;
  StatusBar.Panels[0].Text := 'Bereit, keine Firmware-Datei geladen';
  stgMemoryView.Clear;

  settingsFile := ExtractFilePath(Application.ExeName) + 'UpdateLoader.ini';

  //Startparameter: .ini oder .hex-Datei
  if ParamCount > 0 then
  begin
    if fileExists(ParamStr(1)) then
    begin
      if lowerCase(ExtractFileExt(ParamStr(1))) = '.ini' then
        settingsFile := ParamStr(1)
      else if lowerCase(ExtractFileExt(ParamStr(1))) = '.hex' then
        edtFirmwareFilename.Text := ParamStr(1);
    end;
  end;

  //Einstellungen laden
  try
    loadSettings(settingsFile);
    settingsLoaded := True;
  except
    ShowMessage('Fehler beim Laden der Programmeinstellungen');
  end;

  memUpdateStatus.Clear;
  memUpdateLog.Clear;
end;

procedure TMainWindow.lblCopyrightClick(Sender: TObject);
begin
  OpenURL('http://www.leo-andres.de/');
end;

procedure TMainWindow.FormClose(Sender: TObject; var CloseAction: TCloseAction);
begin
  //Einstellungen speichern
  if (settingsLoaded) and (not deliveryMode) then
  begin
    try
      saveSettings(settingsFile);
    except
      ShowMessage('Fehler beim Speichern der Programmeinstellungen');
    end;
  end;

  CloseAction := caFree;
end;

procedure TMainWindow.FormCloseQuery(Sender: TObject; var CanClose: boolean);
begin
  CanClose := not programmingActive;
end;

procedure TMainWindow.FormCreate(Sender: TObject);
var
  dialogTranslation: TPOFile;
  Item: TPOFileItem;
begin
  inherited;

  SetLength(updateBuffer, settingMaximumBufferSize);
  clearBuffer(updateBuffer);

  updateBufferValid := False;
  updateBufferUsed := 0;

  memoryViewUpdate := False;
  programmingActive := False;

  settingsFile := '';
  settingsLoaded := False;

  deliveryMode := False;
  miniGUI := False;

  //Schaltflächen übersetzen
  dialogTranslation := TPOFile.Create;

  Item := Nil;
  dialogTranslation.FillItem(Item, 'rsmbyes', '&Yes', '&Ja', '-', 'lclstrconsts', '', '');
  dialogTranslation.FillItem(Item,'rsmbno', '&No', '&Nein', '-', 'lclstrconsts', '', '');
  dialogTranslation.FillItem(Item,'rsmbok', '&OK', '&OK', '-', 'lclstrconsts', '', '');
  dialogTranslation.FillItem(Item,'rsmbcancel', 'Cancel', 'Abbrechen', '-', 'lclstrconsts', '', '');

  Translations.TranslateUnitResourceStrings('LCLStrConsts', dialogTranslation);
end;

{$ENDREGION}

{$REGION 'Events (Button-Betätigung)'}
procedure TMainWindow.PageControlChanging(Sender: TObject; var AllowChange: boolean);
begin
  if (programmingActive = True) and (PageControl.PageIndex = 0) then
    PageControl.PageIndex := 0;

  AllowChange := not programmingActive;
end;

procedure TMainWindow.btnClearSettingsClick(Sender: TObject);
var
  reply: integer;
begin
  reply := Application.MessageBox('Einstellungen wirklich zurücksetzen?',
    'UpdateLoader', MB_ICONQUESTION or MB_YESNO or MB_DEFBUTTON2);

  if reply = idYes then
  begin
    cmbComPort.ItemIndex := 0;
    cmbBaudRate.ItemIndex := defaultBaudrate;
    PageControl.TabIndex := 0;

    ckbAdvancedMode.Checked := False;
    ckbProgramWrite.Checked := True;
    ckbProgramVerify.Checked := True;
    ckbProgramStart.Checked := True;
    ckbReloadFile.Checked := True;
    ckbHideMessages.Checked := False;
    ckbShowMemory.Checked := False;
    ckbDetectOneWire.Checked := False;
    ckbSendResetCmd.Checked := True;

    ckbEnableDevMode.Checked := False;

    ckbDevMiniGUI.Checked := False;
    ckbDevOnTop.Checked := False;
    ckbDevHideAdv.Checked := False;
    ckbDevLockFile.Checked := False;
    ckbDevLockPass.Checked := False;
    ckbDevHidePass.Checked := False;

    edtBootloaderPassword.Text := defaultBootloaderPassword;
    edtFirmwareFilename.Text := '';

    edtConnectTries.Text := IntToStr(defaultConnectTries);

    ckbAdvancedModeChange(Self);
    ckbEnableDevModeChange(Self);
    edtFirmwareFilenameChange(Self);

    try
      saveSettings(settingsFile);
    except
      ShowMessage('Fehler beim Speichern der Programmeinstellungen');
    end;
  end;
end;

procedure TMainWindow.btnSaveSettingsClick(Sender: TObject);
var
  reply: integer;
begin
  reply := Application.MessageBox('Konfiguration wirklich speichern?' +
    sLineBreak + sLineBreak +
    'Hinweis: Der absolute Pfad zur Update-Datei wird entfernt',
    'UpdateLoader', MB_ICONQUESTION or MB_YESNO or MB_DEFBUTTON2);

  if reply = idYes then
  begin

    //Einstellungen speichern
    try
      if (settingsLoaded) and (not deliveryMode) then
        saveSettings(settingsFile, True)
      else
        raise Exception.Create('Speichern unzulässig');
    except
      ShowMessage('Fehler beim Speichern der Programmeinstellungen!');
      Exit;
    end;

    reply := Application.MessageBox('Konfiguration gespeichert!' +
      sLineBreak + 'UpdateLoader kann nun ausgeliefert werden' +
      sLineBreak + sLineBreak + 'Programm jetzt beenden?', 'UpdateLoader',
      MB_ICONINFORMATION or MB_YESNO or MB_DEFBUTTON1);

    if reply = idYes then
      Application.Terminate;
  end;
end;

procedure TMainWindow.cmbComPortChange(Sender: TObject);
begin
  if cmbComPort.ItemIndex >= 0 then
  begin
    cmbComPort.Hint := cmbComPort.Items[cmbComPort.ItemIndex];
    cmbComPort.ShowHint := true;
  end
  else
    cmbComPort.ShowHint := false;
end;

procedure TMainWindow.btnSelectFirmwareFilenameClick(Sender: TObject);
begin
  if edtFirmwareFilename.Text <> '' then
    OpenDialog.Filename := edtFirmwareFilename.Text
  else
    OpenDialog.Filename := ExtractFilePath(Application.ExeName);

  if OpenDialog.Execute then
  begin
    if edtFirmwareFilename.Text = OpenDialog.FileName then
      edtFirmwareFilenameChange(Self)
    else
      edtFirmwareFilename.Text := OpenDialog.FileName;
  end;
end;

procedure TMainWindow.ckbAdvancedModeChange(Sender: TObject);
begin
  TabSheet2.TabVisible := ckbAdvancedMode.Checked;
  if (TabSheet2.TabVisible = False) and (PageControl.TabIndex = 1) then
    PageControl.TabIndex := 0;
end;

procedure TMainWindow.ckbDevHidePassChange(Sender: TObject);
begin
  if ckbDevHidePass.Checked then
  begin
    ckbDevLockPass.Tag := integer(ckbDevLockPass.Checked);
    ckbDevLockPass.Checked := True;
    ckbDevLockPass.Enabled := False;
  end
  else
  begin
    ckbDevLockPass.Checked := boolean(ckbDevLockPass.Tag);
    ckbDevLockPass.Enabled := True;
  end;
end;

procedure TMainWindow.ckbDevMiniGUIChange(Sender: TObject);
begin
  if ckbDevMiniGUI.Checked then
  begin
    ckbDevHideAdv.Tag := integer(ckbDevHideAdv.Checked);
    ckbDevLockFile.Tag := integer(ckbDevLockFile.Checked);
    ckbDevHidePass.Tag := integer(ckbDevHidePass.Checked);

    ckbDevHideAdv.Checked := True;
    ckbDevLockFile.Checked := True;
    ckbDevHidePass.Checked := True;

    ckbDevHideAdv.Enabled := False;
    ckbDevLockFile.Enabled := False;
    ckbDevHidePass.Enabled := False;
  end
  else
  begin
    ckbDevHideAdv.Checked := boolean(ckbDevHideAdv.Tag);
    ckbDevLockFile.Checked := boolean(ckbDevLockFile.Tag);
    ckbDevHidePass.Checked := boolean(ckbDevHidePass.Tag);

    ckbDevHideAdv.Enabled := True;
    ckbDevLockFile.Enabled := True;
    ckbDevHidePass.Enabled := True;
  end;
end;

procedure TMainWindow.ckbEnableDevModeChange(Sender: TObject);
begin
  ckbDevMiniGUI.Enabled := ckbEnableDevMode.Checked;
  ckbDevOnTop.Enabled := ckbEnableDevMode.Checked;
  ckbDevHideAdv.Enabled := ckbEnableDevMode.Checked;
  ckbDevLockFile.Enabled := ckbEnableDevMode.Checked;
  ckbDevLockPass.Enabled := ckbEnableDevMode.Checked;
  ckbDevHidePass.Enabled := ckbEnableDevMode.Checked;

  btnSaveSettings.Enabled := ckbEnableDevMode.Checked;
end;

procedure TMainWindow.ckbShowMemoryChange(Sender: TObject);
begin
  if (ckbShowMemory.Checked = True) and (updateBufferValid = True) then
  begin
    memoryViewUpdate := True;

    if PageControl.TabIndex = 1 then
      TabSheet2Show(Self);
  end
  else
  begin
    memoryViewUpdate := False;

    stgMemoryView.Clear;
    gpbMemoryView.Caption := ' Programm ';
  end;
end;

procedure TMainWindow.edtConnectTriesChange(Sender: TObject);
begin
  try
    StrToInt(edtConnectTries.Text);
  except
    edtConnectTries.Text := IntToStr(defaultConnectTries);
    exit;
  end;
  if StrToInt(edtConnectTries.Text) <= 0 then
    edtConnectTries.Text := IntToStr(defaultConnectTries);
end;

procedure TMainWindow.edtFirmwareFilenameChange(Sender: TObject);
var
  success: boolean;
  oldCursor: TCursor;
begin
  success := False;

  if (FileExistsUTF8(edtFirmwareFilename.Text)) and
    (length(edtFirmwareFilename.Text) > 0) then
  begin
    oldCursor := Screen.Cursor;
    Screen.Cursor := crHourGlass;
    pgbUpdateProgress.Style := pbstMarquee;

    updateBufferUsed := 0;
    try
      success := loadProgramFile(edtFirmwareFilename.Text, updateBuffer,
        settingMaximumBufferSize, updateBufferUsed);
    except
      on E: Exception do
        logMessage(E.Message + ' (' + ExtractFileName(edtFirmwareFilename.Text) +
          ')', True, True);
    end;

    if success = True then
    begin
      updateBufferValid := True;

      ckbShowMemoryChange(Self);

      logMessage('Firmware-Datei geladen (' +
        ExtractFileName(edtFirmwareFilename.Text) + ')');
      StatusBar.Panels[0].Text :=
        'Bereit, Firmware-Datei geladen (' + ExtractFileName(
        edtFirmwareFilename.Text) + ')';
      btnUpdateControl.Enabled := True;
    end
    else
    begin
      updateBufferValid := False;
      btnUpdateControl.Enabled := False;
      StatusBar.Panels[0].Text := 'Bereit, keine Firmware-Datei geladen';

      ckbShowMemoryChange(Self);
    end;

    Screen.Cursor := oldCursor;
    pgbUpdateProgress.Style := pbstNormal;
  end
  else
  begin
    updateBufferValid := False;
    btnUpdateControl.Enabled := False;
    StatusBar.Panels[0].Text := 'Bereit, keine Firmware-Datei geladen';

    edtFirmwareFilename.Text := '';

    ckbShowMemoryChange(Self);
  end;
end;

procedure TMainWindow.stgMemoryViewDrawCell(Sender: TObject;
  aCol, aRow: integer; aRect: TRect; aState: TGridDrawState);
begin
  if gdFixed in aState then
    Exit;

  case aCol of
    0:
    begin
      stgMemoryView.Canvas.Font.Color := RGB(128, 0, 0);
    end;
    4:
    begin
      stgMemoryView.Canvas.Font.Color := RGB(128, 0, 128);
    end;
    else
    begin
      stgMemoryView.Canvas.Font.Color := RGB(0, 0, 128);
    end;
  end;

  stgMemoryView.Canvas.TextOut(aRect.Left + 2, aRect.Top + 2,
    stgMemoryView.Cells[aCol, aRow]);
  stgMemoryView.Canvas.TextRect(aRect, aRect.Left + 2, aRect.Top +
    2, stgMemoryView.Cells[aCol, aRow]);
end;

procedure TMainWindow.TabSheet2Show(Sender: TObject);
var
  oldCursor: TCursor;
begin
  if memoryViewUpdate = True then
  begin
    memoryViewUpdate := False;

    oldCursor := Screen.Cursor;
    Screen.Cursor := crHourGlass;

    showProgramMemory(stgMemoryView, updateBuffer, updateBufferUsed, pgbUpdateProgress);
    gpbMemoryView.Caption := ' Programm (' + convertKB2String(updateBufferUsed) + ') ';

    Screen.Cursor := oldCursor;
  end;
end;

{$ENDREGION}

{$REGION 'Hilfsfunktionen für die Benutzerführung'}
procedure TMainWindow.logMessage(message: string; openMessageBox: boolean = False;
  addToLog: boolean = True; windowIcon: integer = MB_ICONEXCLAMATION);
begin
  if addToLog = True then
    memUpdateLog.Lines.Add(message);

  if openMessageBox = True then
    Application.MessageBox(PChar(message), 'UpdateLoader - Nachricht',
      MB_OK + windowIcon);
end;

procedure TMainWindow.infoMessage(message: string);
begin
  memUpdateStatus.Lines.Add(message);
end;

procedure TMainWindow.flashNotice(showNotice: boolean; message: string = '';
  flashSpeed: integer = 500);
begin
  lblFlashNotice.Caption := message;
  lblFlashNotice.Visible := showNotice;
  lblFlashNotice.Left := round((gpbInfo.Width - lblFlashNotice.Width) / 2);

  tmrFlashNotice.Interval := flashSpeed;
  tmrFlashNotice.Enabled := showNotice;
end;

procedure TMainWindow.tmrFlashNoticeTimer(Sender: TObject);
begin
  lblFlashNotice.Visible := not lblFlashNotice.Visible;
end;

procedure TMainWindow.sleepActive(duration: integer);
var
  i: integer;
begin
  if duration <= 0 then
    Exit;

  for i := 0 to Round(duration / 100) do
  begin
    Sleep(100);
    Application.ProcessMessages;
  end;
end;

{$ENDREGION}

{$REGION 'Update-Routine'}
procedure TMainWindow.btnUpdateControlClick(Sender: TObject);
var
  programWrite, programVerify, programStart, showMessages: boolean;
  sendReset, detectOneWire: boolean;
  reloadFile: boolean;
  reply: integer;
  startTime: TDateTime;
  oldCursor: TCursor;
  updateDuration: string;
  oldStatus: string;
  success: boolean;
  serialConnection: TBlockSerial;
  bootloaderRoutines: TBootloaderRoutines;
begin
  showMessages := not (ckbHideMessages.Checked and
    (ckbAdvancedMode.Checked or deliveryMode)); //Nachrichten im Liefermodus ausschaltbar
  programWrite := ckbProgramWrite.Checked or ((not ckbAdvancedMode.Checked) and (not deliveryMode));
  programVerify := ckbProgramVerify.Checked or ((not ckbAdvancedMode.Checked) and (not deliveryMode));
  programStart := ckbProgramStart.Checked or ((not ckbAdvancedMode.Checked) and (not deliveryMode));
  reloadFile := ckbReloadFile.Checked or ((not ckbAdvancedMode.Checked) and (not deliveryMode));
  sendReset := ckbSendResetCmd.Checked or ((not ckbAdvancedMode.Checked) and (not deliveryMode));
  detectOneWire := ckbDetectOneWire.Checked and (ckbAdvancedMode.Checked or deliveryMode);

  if programmingActive = True then
    Exit;

  if cmbComPort.ItemIndex <= -1 then
  begin
    logMessage('Kein COM-Port ausgewählt.' +
      sLineBreak + sLineBreak + 'Nichts zu tun!', True, False, MB_ICONINFORMATION);
    Exit;
  end;

  if (programWrite = False) and (programVerify = False) then
  begin
    logMessage('Mindestens einen Schritt (Programmieren/Überprüfen) auswählen.' +
      sLineBreak + sLineBreak + 'Nichts zu tun!', True, False, MB_ICONINFORMATION);
    Exit;
  end;

  if reloadFile = True then
    edtFirmwareFilenameChange(Self);

  if updateBufferValid = False then
    Exit;

  if showMessages = True then
  begin
    reply := Application.MessageBox(
      'Warnung: Die Aktualisierung der Firmware kann nicht mehr rückgängig gemacht werden.'
      + ' Im Fehlerfall kann das Gerät beschädigt werden.' +
      sLineBreak + sLineBreak + sLineBreak + 'Update wirklich durchführen?',
      'UpdateLoader', MB_ICONWARNING or MB_YESNO or MB_DEFBUTTON2);
    if reply = idNo then
      Exit;
  end;

  //Initialisierung
  programmingActive := True;
  startTime := TDateTime(now);

  serialConnection := TBlockSerial.Create;
  bootloaderRoutines := TBootloaderRoutines.Create;

  //GUI sperren
  btnSelectFirmwareFilename.Enabled := False;
  btnUpdateControl.Enabled := False;
  cmbComPort.Enabled := False;
  edtBootloaderPassword.Enabled := False;
  edtFirmwareFilename.Enabled := False;
  ckbAdvancedMode.Enabled := False;
  PageControl.PageIndex := 0;

  //Statusanzeigen setzen
  oldCursor := Screen.Cursor;
  Screen.Cursor := crHourGlass;
  oldStatus := StatusBar.Panels[0].Text;
  memUpdateStatus.Clear;

  if showMessages = True then
    MainWindow.FormStyle := fsStayOnTop;

  btnUpdateControl.Caption := 'Bitte warten...';
  StatusBar.Panels[0].Text :=
    'Firmware-Update wird ausgeführt, Benutzeroberfläche ist deaktiviert!';
  logMessage('Firmware-Update gestartet (' + convertKB2String(updateBufferUsed) + ')');

  //Update ausführen
  try
    success := doFirmwareUpdate(serialConnection, bootloaderRoutines,
      programWrite, programVerify, programStart, showMessages, sendReset, detectOneWire);
  except
    on E: Exception do
    begin
      logMessage('Fehler: ' + E.Message + ' (COM: ' +
        serialConnection.GetErrorDesc(serialConnection.LastError) + ')', true, true, MB_ICONSTOP);
      success := false;
    end;
  end;

  //Verbindung freigeben
  if serialConnection.InstanceActive = True then
  begin
    serialConnection.Purge;
    serialConnection.Flush;
    serialConnection.CloseSocket;
  end;

  //Objekte löschen
  serialConnection.Free;
  bootloaderRoutines.Free;

  //GUI freigeben
  btnSelectFirmwareFilename.Enabled := True;
  btnUpdateControl.Enabled := True;
  cmbComPort.Enabled := True;
  edtBootloaderPassword.Enabled := True;
  edtFirmwareFilename.Enabled := True;
  ckbAdvancedMode.Enabled := True;

  //Statusanzeige zurücksetzen
  flashNotice(False);

  Screen.Cursor := oldCursor;
  StatusBar.Panels[0].Text := oldStatus;

  if showMessages = True then
    MainWindow.FormStyle := fsNormal;

  btnUpdateControl.Caption := 'Update starten!';
  pgbProgramConnect.Position := 0;
  pgbProgramWrite.Position := 0;
  pgbProgramVerify.Position := 0;
  pgbUpdateProgress.Position := 0;

  //Meldung
  if success = True then
  begin
    logMessage('Firmware-Update erfolgreich beendet!');

    DateTimeToString(updateDuration, 'hh:mm:ss', TDateTime(now) - startTime);
    logMessage('Gesamtdauer: ' + updateDuration);

    if (miniGUI = True) or (showMessages = True) then
    begin
      logMessage('Firmware-Update erfolgreich beendet!' + sLineBreak +
        '(Gesamtdauer: ' + updateDuration + ')', True, False, MB_ICONINFORMATION);
    end;
  end
  else
  begin
    infoMessage('Fehler aufgetreten, Update abgebrochen!');
    logMessage('Fehler beim Firmware-Update, Ausführung abgebrochen');
  end;

  //Status zurücksetzen, Freigabe für nächsten Durchlauf
  programmingActive := False;
end;

function TMainWindow.doFirmwareUpdate(serialConnection: TBlockSerial;
  bootloaderRoutines: TBootloaderRoutines;
  programWrite, programVerify, programStart, showMessages: boolean;
  sendReset: boolean = True; supportOneWire: boolean = True): boolean;
var
  comPort: string;
  wireMode: integer;
  oneWire: boolean;
  connectTries: integer;
  reply: integer;
  supportsCRC: boolean;
  supportsVerify: boolean;
  deviceName, deviceSignature: string;
  deviceFlashSize: longint;
  deviceWriteBuffer: integer;
  deviceRevision: string;
  baudRate: integer;
begin
  oneWire := False;
  supportsCRC := False;
  supportsVerify := False;
  Result := False;

  pgbUpdateProgress.Position := 0;
  pgbUpdateProgress.Max := 60;
  pgbUpdateProgress.Step := 10;

  infoMessage('COM-Port öffnen');

  //Port und Einstellungen ermitteln
  comPort := Format('COM%d', [integer(cmbComPort.Items.Objects[cmbComPort.ItemIndex])]);

  baudRate := StrToInt(cmbBaudRate.Text);
  if (baudRate <= 0) or ((not ckbAdvancedMode.Checked) and (not deliveryMode)) then
    baudRate := StrToInt(cmbBaudRate.Items[defaultBaudrate]);

  //Schnittstelle konfigurieren
  serialConnection.RaiseExcept := True;
  serialConnection.LinuxLock := False;
  serialConnection.InterPacketTimeout := False;

  //Com-Port öffnen
  try
    serialConnection.Connect(comPort);
    sleepActive(settingPortSetupDelay);
    serialConnection.Config(baudRate, 8, 'N', SB1, False, False);
    sleepActive(settingPortSetupDelay);
    serialConnection.Purge;
    serialConnection.Flush;
  except
    on E: Exception do
    begin
      logMessage(serialConnection.GetErrorDesc(serialConnection.LastError) +
        sLineBreak + sLineBreak + 'COM-Port konnte nicht geöffnet werden.' +
        sLineBreak + 'Port wird eventuell von anderer Anwendung blockiert',
        True, False, MB_ICONSTOP);
      Exit;
    end;
  end;

  //Nach Verbindungsaufbau Exceptions deaktivieren -> Fehlerprüfung passiert auf Protokollebene
  serialConnection.RaiseExcept := False;

  //Geöffnete Verbindung an Bootloader-Klasse übergeben
  bootloaderRoutines.assignSerialConnection(serialConnection);

  pgbUpdateProgress.StepIt;
  Application.ProcessMessages;
  infoMessage('Verbindungsaufbau...');

  //Benutzerführung
  if sendReset = False then
  begin
    if showMessages = True then
    begin
      logMessage('Gerät jetzt ausschalten und weitere Anweisungen abwarten.' +
        sLineBreak + sLineBreak + 'Verbindung nicht trennen!', True,
        False, MB_ICONINFORMATION);

      sleepActive(settingConnectMessageDelay);
      flashNotice(True, 'Gerät wieder einschalten!', 500);
      sleepActive(settingConnectMessageDelay);
    end
    else
    begin
      flashNotice(True, 'Bootloader aktivieren!', 500);
      sleepActive(settingConnectMessageDelay);
    end;
  end;

  //Verbindung herstellen
  pgbUpdateProgress.StepIt;
  Application.ProcessMessages;

  try
    connectTries := StrToInt(edtConnectTries.Text);
    if (connectTries <= 0) or ((not ckbAdvancedMode.Checked) and (not deliveryMode)) then
      connectTries := defaultConnectTries;

    wireMode := bootloaderRoutines.connect(edtBootloaderPassword.Text,
      connectTries, sendReset, supportOneWire, pgbProgramConnect);
  except
    on E: Exception do
    begin
      logMessage('Verbindungsaufbau:' + sLineBreak + E.Message +
        sLineBreak + sLineBreak + 'Update-Vorgang wird abgebrochen',
        True, False, MB_ICONSTOP);
      Exit;
    end;
  end;

  //Exceptions aktivieren: Timeout muss Exception auslösen
  serialConnection.RaiseExcept := True;

  if (wireMode = 1) and supportOneWire then
  begin
    oneWire := True;

    if connectTries <> 1 then
      infoMessage('Verbunden: 1-Draht-Modus, ' + IntToStr(connectTries) + ' Versuche')
    else
      infoMessage('Verbunden: 1-Draht-Modus, 1 Versuch');

    logMessage('Achtung: Der 1-Draht-Modus wurde noch nicht getestet!',
      True, False, MB_ICONWARNING);
  end
  else if (wireMode = 1) and  not supportOneWire then
  begin
    logMessage('Verbindungsaufbau:' + sLineBreak + '1-Draht-Verbindung erkannt, Modus jedoch nicht unterstützt!' +
      sLineBreak + sLineBreak + 'Update-Vorgang wird abgebrochen',
      True, False, MB_ICONSTOP);
    Exit;
  end
  else
  begin
    if connectTries <> 1 then
      infoMessage('Verbunden: 2-Draht-Modus, ' + IntToStr(connectTries) + ' Versuche')
    else
      infoMessage('Verbunden: 2-Draht-Modus, 1 Versuch');
  end;

  bootloaderRoutines.setWireMode(oneWire);

  //CRC-Unterstützung ermitteln
  supportsCRC := bootloaderRoutines.detectCRCSupport;
  bootloaderRoutines.resetCRC;

  if supportsCRC = True then
    infoMessage('Übertragung CRC-gesichert!')
  else
    infoMessage('CRC-Überprüfung nicht unterstützt');

  //Verify-Unterstützung ermitteln
  supportsVerify := bootloaderRoutines.detectVerifySupport;
  if supportsVerify = True then
    infoMessage('Überprüfung (Verify) unterstützt!')
  else
    infoMessage('Überprüfung (Verify) nicht unterstützt');

  //Im Paketmodus muss ein aktives "Verify" immer ausgeführt werden
  if (programVerify) and (not supportsVerify) and (deliveryMode) then
  begin
    logMessage('Die Überprüfung (Verify) der Übertragung wird nicht unterstützt, ist jedoch für das aktuelle Programmpekt erforderlich!' +
      sLineBreak + sLineBreak + 'Update-Vorgang wird abgebrochen',
      True, False, MB_ICONSTOP);
    Exit;
  end;

  Application.ProcessMessages;

  //Informationen auslesen
  try
    //Bootloader-Version
    deviceRevision := bootloaderRoutines.readRevision;
    if deviceRevision <> settingValidBootloader then
    begin
      raise Exception.Create('Ungültige Bootloader-Version erkannt: ' +
        deviceRevision + sLineBreak + sLineBreak + 'Ausschließlich Version ' +
        settingValidBootloader + ' wird unterstützt!');
    end;

    //Signatur
    deviceSignature := bootloaderRoutines.readSignature;
    deviceName := bootloaderRoutines.getDeviceName(deviceSignature);

    if deviceName = defaultDeviceName then
    begin
      if showMessages = True then
      begin
        reply := Application.MessageBox(
          'Es wurde ein unbekannter Mikrocontroller gefunden (unbekannte Signatur).' +
          sLineBreak + 'Das Firmware-Update könnte fehlschlagen und Daten verloren gehen.'
          + sLineBreak + sLineBreak + 'Update trotzdem durchführen?',
          'UpdateLoader', MB_ICONWARNING or MB_YESNO or MB_DEFBUTTON2);

        if reply = idNo then
          Exit;
      end
      else
        logMessage('Achtung: Unbekannter Mikrocontroller!');
    end;

    //Notiz Bootloader-Version und Chip-Signatur
    infoMessage('Chip-Signatur: ' + deviceSignature);
    infoMessage('Bootloader V' + deviceRevision + ' auf ' + deviceName + ' gefunden');

    //Flash-Größe
    deviceFlashSize := bootloaderRoutines.readFlashSize;

    if deviceFlashSize > settingMaximumBufferSize then
    begin
      raise Exception.Create(
        'Flash-Speicher größer als maximal mit diesem Programm ansprechbarer Bereich' +
        sLineBreak + sLineBreak + '(' + convertKB2String(deviceFlashSize) +
        ' Flash >>> ' + convertKB2String(settingMaximumBufferSize) + ' Maximum)');
    end;

    if deviceFlashSize < updateBufferUsed then
    begin
      raise Exception.Create('Flash-Speicher kleiner als Firmware-Datei' +
        sLineBreak + sLineBreak + '(' + IntToStr(deviceFlashSize) +
        ' Byte Flash <<< ' + IntToStr(updateBufferUsed) + ' Byte Firmware)');
    end;

    //Puffer-Größe
    deviceWriteBuffer := bootloaderRoutines.readBufferSize;

    if deviceWriteBuffer < settingMinimumWriteBuffer then
    begin
      raise Exception.CreateFmt('Sendepuffer zu klein' + sLineBreak +
        sLineBreak + '(%d Byte Puffer <<< %d Byte Minimum)', [deviceWriteBuffer, settingMinimumWriteBuffer]);
    end
    else if deviceWriteBuffer < settingSmallWriteBuffer then
      logMessage('Achtung: Kleiner Sendepuffer!');

    //Notiz Flash-Größe und Sendepuffer
    infoMessage('Flash-Größe: ' + convertKB2String(deviceFlashSize) +
      ' (Puffer: ' + IntToStr(deviceWriteBuffer) + ' Byte)');

  except
    on E: Exception do
    begin
      logMessage('Einstellungen auslesen:' + sLineBreak + E.Message +
        sLineBreak + sLineBreak + 'Update-Vorgang wird abgebrochen',
        True, False, MB_ICONSTOP);
      Exit;
    end;
  end;

  Application.ProcessMessages;

  //CRC zwischenprüfen
  if supportsCRC = True then
  begin
    try
      if bootloaderRoutines.checkCRC = False then
        raise Exception.Create('Prüfsumme falsch, Übertragungsfehler!')
      else
        infoMessage('CRC-Prüfsumme OK');
    except
      on E: Exception do
      begin
        logMessage('CRC-Prüfsumme:' + sLineBreak + E.Message +
          sLineBreak + sLineBreak + 'Update-Vorgang wird abgebrochen',
          True, False, MB_ICONSTOP);
        Exit;
      end;
    end;
  end;

  pgbUpdateProgress.StepIt;
  Application.ProcessMessages;

  flashNotice(True, 'Update läuft', 1000);

  //Programm aufspielen
  if programWrite = True then
  begin

    infoMessage('Programmiere ' + convertKB2String(updateBufferUsed) +
      ' von ' + convertKB2String(deviceFlashSize) + ' Flash...');

    try
      if bootloaderRoutines.writeFirmware(updateBuffer, updateBufferUsed, deviceWriteBuffer, pgbProgramWrite) = False then
        raise Exception.Create('Programmieren fehlgeschlagen')
      else
        infoMessage('Programmieren erfolgreich');
    except
      on E: Exception do
      begin
        logMessage('Programmieren: (Adresse 0x' + bootloaderRoutines.getFailureAddress +
          ')' + sLineBreak + E.Message + sLineBreak + sLineBreak +
          'Update-Vorgang wird abgebrochen' + sLineBreak +
          'Achtung: Original-Firmware wurde möglicherweise überschrieben oder beschädigt!',
          True, False, MB_ICONSTOP);
        Exit;
      end;
    end;

    pgbUpdateProgress.StepIt;
  end
  else
    pgbUpdateProgress.Max := pgbUpdateProgress.Max - 10;

  Application.ProcessMessages;

  //Abfrage auf optionales Überprüfen
  if (showMessages = True) and (programVerify = False) and (supportsVerify = True) and (not deliveryMode) then
  begin
    reply := Application.MessageBox(
      'Die geschriebenen Daten sollte zur Erhöhung der Funktionssicherheit überprüft werden.'
      + sLineBreak +
      'In den erweiterten Einstellungen wurde die Überprüfung jedoch deaktiviert.' +
      sLineBreak + sLineBreak + 'Überprüfung jetzt trotzdem durchführen?',
      'UpdateLoader', MB_ICONQUESTION or MB_YESNO or MB_DEFBUTTON1);

    if reply = idYes then
      programVerify := True;
  end;

  //Programm prüfen
  if (programVerify = True) and (supportsVerify = True) then
  begin

    infoMessage('Überprüfe gespeicherte Firmware...');

    try
      if bootloaderRoutines.verifyFirmware(updateBuffer, updateBufferUsed, pgbProgramVerify) = False then
        raise Exception.Create('Überprüfung fehlgeschlagen')
      else
        infoMessage('Überprüfung erfolgreich');
    except
      on E: Exception do
      begin
        logMessage('Programm überprüfen: (Adresse 0x' +
          bootloaderRoutines.getFailureAddress + ')' + sLineBreak +
          E.Message + sLineBreak + sLineBreak + 'Update-Vorgang wird abgebrochen',
          True, False, MB_ICONSTOP);
        Exit;
      end;
    end;

    pgbUpdateProgress.StepIt;
  end
  else
    pgbUpdateProgress.Max := pgbUpdateProgress.Max - 10;

  Application.ProcessMessages;

  //CRC prüfen
  if supportsCRC = True then
  begin
    try
      if bootloaderRoutines.checkCRC = False then
        raise Exception.Create('Prüfsumme falsch, Übertragungsfehler!')
      else
        infoMessage('CRC-Prüfsumme OK');
    except
      on E: Exception do
      begin
        logMessage('CRC-Prüfsumme:' + sLineBreak + E.Message +
          sLineBreak + sLineBreak + 'Update-Vorgang wird abgebrochen',
          True, False, MB_ICONSTOP);
        Exit;
      end;
    end;
  end;

  Application.ProcessMessages;

  //Programm starten
  if programStart = True then
  begin
    if bootloaderRoutines.startFirmware = False then
    begin
      if showMessages = True then
      begin
        logMessage('Die neue Firmware konnte nicht automatisch gestartet werden. (Kommando wird nicht unterstützt)'
          + sLineBreak + sLineBreak +
          'Gerät bitte manuell neu starten um die neue Firmware zu aktivieren!',
          True, False, MB_ICONINFORMATION);
      end
      else
        infoMessage('Firmware nicht gestartet (Kommando unbekannt)');
    end
    else
      infoMessage('Neue Firmware gestartet');

    pgbUpdateProgress.StepIt;
  end
  else
    pgbUpdateProgress.Max := pgbUpdateProgress.Max - 10;

  Application.ProcessMessages;

  infoMessage('Firmware-Update erfolgreich beendet!');
  pgbUpdateProgress.Position := pgbUpdateProgress.Max;
  sleepActive(500);

  Result := True;
end;

{$ENDREGION}

end.
