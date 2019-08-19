#define MyAppName "Fast Image Viewer"
#define MyAppVersion "1.0.0.3"
;#define MyAppName "Fast Image Viewer Stable"
#define MyAppPublisher "Nevaran"
#define MyAppExeName "Fast Image Viewer.exe"

[Setup]
ChangesAssociations=yes
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{8C9E9ECE-F29A-4A71-B728-B21C0F85366E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; The [Icons] "quicklaunchicon" entry uses {userappdata} but its [Tasks] entry has a proper IsAdminInstallMode Check.
UsedUserAreasWarning=no
; Uncomment the following line to run in non administrative install mode (install for current user only.)
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=D:\CS\Fast Image Viewer Standalone\_Setup
OutputBaseFilename=FIV Setup
SetupIconFile=D:\CS\Fast Image Viewer Standalone\Fast Image Viewer Standalone\bin\Release\def.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
;Source: "C:\Program Files (x86)\Inno Setup 6\Examples\MyProg.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\CS\Fast Image Viewer Standalone\Fast Image Viewer Standalone\bin\Release\Fast Image Viewer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\CS\Fast Image Viewer Standalone\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\CS\Fast Image Viewer Standalone\Fast Image Viewer Standalone\bin\Release\bg-BG\*"; DestDir: "{app}\bg-BG"; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "D:\CS\Fast Image Viewer Standalone\Fast Image Viewer Standalone\bin\Release\nl-BE\*"; DestDir: "{app}\nl-BE"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\CS\Fast Image Viewer Standalone\Fast Image Viewer Standalone\bin\Release\nl-NL\*"; DestDir: "{app}\nl-NL"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\CS\Fast Image Viewer Standalone\Fast Image Viewer Standalone\bin\Release\pt-BR\*"; DestDir: "{app}\pt-BR"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\CS\Fast Image Viewer Standalone\Fast Image Viewer Standalone\bin\Release\se-SE\*"; DestDir: "{app}\se-SE"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCU; Subkey: "Software\Classes\.jpg"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

Root: HKCU; Subkey: "Software\Classes\.jpeg"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

Root: HKCU; Subkey: "Software\Classes\.png"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

Root: HKCU; Subkey: "Software\Classes\.gif"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

Root: HKCU; Subkey: "Software\Classes\.tiff"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

Root: HKCU; Subkey: "Software\Classes\.bmp"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

Root: HKCU; Subkey: "Software\Classes\.svg"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
;Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

Root: HKCU; Subkey: "Software\Classes\.ico"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

