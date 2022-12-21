
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "RedditVideoGenerator"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ClickPhase"
#define MyAppURL "https://clickphase.vercel.app/RedditVideoGenerator"
#define MyAppExeName "RedditVideoGenerator.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{4D7DBDA9-B36B-4F5D-8899-D34ABE4DC6E2}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
DisableWelcomePage=no
DisableDirPage=no
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=C:\Program Files (x86)\RedditVideoGenerator
DisableProgramGroupPage=yes
LicenseFile=C:\Users\fligh\source\repos\RedditVideoGenerator\LICENSE.txt
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=C:\Users\fligh\source\repos\RedditVideoGenerator\Installer\InstallerExecutables
OutputBaseFilename=RedditVideoGenerator_1.0.0_Setup
SetupIconFile=C:\Users\fligh\source\repos\RedditVideoGenerator\RedditVideoGenerator\bin\Release\Resources\icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
VersionInfoVersion = 1.0.0
UninstallDisplayIcon={app}\RedditVideoGenerator.exe
UninstallDisplayName=RedditVideoGenerator

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "C:\Users\fligh\source\repos\RedditVideoGenerator\NOTICE.txt"; Flags: dontcopy
Source: "C:\Users\fligh\source\repos\RedditVideoGenerator\Installer\PRIVACYPOLICY.rtf"; Flags: dontcopy
Source: "C:\Users\fligh\source\repos\RedditVideoGenerator\RedditVideoGenerator\bin\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\fligh\source\repos\RedditVideoGenerator\RedditVideoGenerator\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs


[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[UninstallRun]
Filename: "{cmd}"; Parameters: "/C ""taskkill /im {#MyAppExeName} /f /t"; RunOnceId: "Uninstall"

[Code]

var
  LicenseAcceptedRadioButtons: array of TRadioButton;

procedure CheckLicenseAccepted(Sender: TObject);
begin
  // Update Next button when user (un)accepts the license
  WizardForm.NextButton.Enabled :=
    LicenseAcceptedRadioButtons[TComponent(Sender).Tag].Checked;
end;

procedure LicensePageActivate(Sender: TWizardPage);
begin
  // Update Next button when user gets to second license page
  CheckLicenseAccepted(LicenseAcceptedRadioButtons[Sender.Tag]);
end;

function CloneLicenseRadioButton(
  Page: TWizardPage; Source: TRadioButton): TRadioButton;
begin
  Result := TRadioButton.Create(WizardForm);
  Result.Parent := Page.Surface;
  Result.Caption := Source.Caption;
  Result.Left := Source.Left;
  Result.Top := Source.Top;
  Result.Width := Source.Width;
  Result.Height := Source.Height;
  // Needed for WizardStyle=modern / WizardResizable=yes
  Result.Anchors := Source.Anchors;
  Result.OnClick := @CheckLicenseAccepted;
  Result.Tag := Page.Tag;
end;

var
  LicenseAfterPage: Integer;

procedure AddLicensePage(LicenseFileName, WindowTitle, WindowCaption, WindowDescription: string);
var
  Idx: Integer;
  Page: TOutputMsgMemoWizardPage;
  LicenseFilePath: string;
  RadioButton: TRadioButton;
  var
  #ifndef UNICODE
    rtfstr: string;
  #else
    rtfstr: AnsiString;
  #endif
begin
  Idx := GetArrayLength(LicenseAcceptedRadioButtons);
  SetArrayLength(LicenseAcceptedRadioButtons, Idx + 1);

  Page :=
    CreateOutputMsgMemoPage(
      LicenseAfterPage, WindowTitle,
      WindowCaption, WindowDescription, '');
  Page.Tag := Idx;

  // Shrink license box to make space for radio buttons
  Page.RichEditViewer.Height := WizardForm.LicenseMemo.Height;
  Page.RichEditViewer.UseRichEdit := True;
  Page.OnActivate := @LicensePageActivate;

  // Load license
  // Loading ex-post, as Lines.LoadFromFile supports UTF-8,
  // contrary to LoadStringFromFile.
  ExtractTemporaryFile(LicenseFileName);
  LicenseFilePath := ExpandConstant('{tmp}\' + LicenseFileName);
  LoadStringFromFile(LicenseFilePath, rtfstr);
  Page.RichEditViewer.RTFText := rtfstr;
  DeleteFile(LicenseFilePath);

  // Clone accept/do not accept radio buttons
  RadioButton :=
    CloneLicenseRadioButton(Page, WizardForm.LicenseAcceptedRadio);
  LicenseAcceptedRadioButtons[Idx] := RadioButton;

  RadioButton :=
    CloneLicenseRadioButton(Page, WizardForm.LicenseNotAcceptedRadio);
  // Initially not accepted
  RadioButton.Checked := True;

  LicenseAfterPage := Page.ID;
end;

procedure InitializeWizard();
begin
  LicenseAfterPage := wpLicense;

  AddLicensePage('NOTICE.txt', 'Third Party Notices and Licenses', 'Please read the following important information before continuing.', 
  'Please read the following third party notices and licenses. You must accept the terms of these licenses and notices before continuing with the installation.');
  
  AddLicensePage('PRIVACYPOLICY.rtf', 'Privacy Policy', 'Please read the following important information before continuing.',
  'Please read the following Privacy Policy. You must accept the terms of this privacy policy before continuing with the installation.');
end;
