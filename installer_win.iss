[Setup]
AppName=FixMaster
AppVersion=1.0.0
DefaultDirName={autopf}\FixMaster
DefaultGroupName=FixMaster
OutputDir=releases\installers
OutputBaseFilename=FixMaster-Setup-Win64
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de Trabalho"; GroupDescription: "Atalhos adicionais:"

[Files]
Source: "releases\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\FixMaster"; Filename: "{app}\FixMaster.exe"
Name: "{autodesktop}\FixMaster"; Filename: "{app}\FixMaster.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\FixMaster.exe"; Description: "Iniciar o FixMaster agora"; Flags: nowait postinstall skipifsilent