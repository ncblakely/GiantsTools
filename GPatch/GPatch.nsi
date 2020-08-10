SetCompressor /SOLID lzma

!define PRODUCT_NAME "Giants: Citizen Kabuto"
!define PRODUCT_VERSION "1.498"

; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "GPatch.ico"

; Welcome page
;!insertmacro MUI_PAGE_WELCOME
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
;!define MUI_FINISHPAGE_SHOWREADME $INSTDIR\readme.txt

!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!insertmacro MUI_PAGE_FINISH

!define MUI_LANGDLL_REGISTRY_ROOT "HKCU"
!define MUI_LANGDLL_REGISTRY_KEY "Software\PlanetMoon\Giants"
!define MUI_LANGDLL_REGISTRY_VALUENAME "SetupLanguage"

; Language files
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "German"
!insertmacro MUI_LANGUAGE "Italian"
!insertmacro MUI_LANGUAGE "Spanish"

; Language selection settings
!define MUI_LANGDLL_WINDOWTITLE "Setup Language"

; Language strings
LangString AskInstallGSA ${LANG_ENGLISH} "The GameSpy Arcade gaming service supports multiplayer matchmaking for Giants: Citizen Kabuto.  Find buddies, download patches, and chat with new opponents online.  Install GameSpy Arcade?"
LangString AskInstallGSA ${LANG_FRENCH} "Le service de jeu GameSpy Arcade gère désormais les parties multijoueurs de Giants: Citizen Kabuto.  Trouvez des amis, téléchargez des patchs et discustez avec de nouveaux adversaires en ligne.  Installer GameSpy Arcade ?"
LangString AskInstallGSA ${LANG_GERMAN} "Der 'GameSpy Arcade Gaming Service' unterstützt Multiplayer-Matchmaking für Giants: Citizen Kabuto. Finde Freunde, lade Patches herunter und chatte online mit neuen Gegnern. GameSpy Arcade installieren?"
LangString AskInstallGSA ${LANG_SPANISH} "Ahora, el servicio de juegos de GameSpy Arcade acepta las partidas multijugador de Giants: Citizen Kabuto. Busca amigos, descarga parches y charla con nuevos rivales conectados. ¿Deseas instalar GameSpy Arcade?"
LangString AskInstallGSA ${LANG_ITALIAN} "Il servizio giochi di GameSpy Arcade supporta l'abbinamento di più giocatori per Giants: Cittadino Kabuto.  Trova amici, scarica le patch e chatta coi nuovi avversari online.  Installare GameSpy Arcade?"

!include LogicLib.nsh

; MUI end ------

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "GPatch1_498_177_0.exe"
InstallDir "$PROGRAMFILES\Giants\"
InstallDirRegKey HKCU "SOFTWARE\PlanetMoon\Giants" "DestDir"
ShowInstDetails hide

;Request application privileges for Windows Vista+
RequestExecutionLevel admin

Section
  SetDetailsView hide
  SectionIn RO
  SetOverwrite on
  
  ; Install DX redist for DX9 renderer
  SetOutPath "$INSTDIR\Redist"
  File /r "Files\Redist\*.*"
  ExecWait "$INSTDIR\Redist\dxsetup.exe /silent" $0
  
  ${If} $0 != 0
        MessageBox MB_OK "Setup failed to update DirectX ($0). Please visit www.microsoft.com and download the latest version of the DirectX end user redistributable."
        RMDir /r "$INSTDIR\Redist" ; Delete temporary files
        ;return
  ${Else}
         RMDir /r "$INSTDIR\Redist" ; Delete temporary files
  ${EndIf}
  
  ; Delete old files
  Delete $INSTDIR\bin\Shaders\*.*
  Delete $INSTDIR\gg_dx7r.dll
  Delete $INSTDIR\gg_dx8r.dll
  Delete $INSTDIR\Giants.exe
  Delete $INSTDIR\*.vso
  Delete $INSTDIR\*.pso
  
  SetOutPath "$INSTDIR"
  File /r "Files\*.*"

 ; remove old mods (may have compatibility issues)
  Delete $INSTDIR\bin\worldlist2.bin
  Delete $INSTDIR\bin\worldlist3.bin
  Delete $INSTDIR\bin\worldlist4.bin
  Delete $INSTDIR\bin\worldlist5.bin
  Delete $INSTDIR\bin\mappack1.gzp
  Delete $INSTDIR\bin\A-GRM1.gzp
  
  
SectionEnd

!define NETVersion "3.5"
!define NETInstaller "dotnetfx35setup.exe"
Section "MS .NET Framework v${NETVersion}" SecFramework
  IfFileExists "$WINDIR\Microsoft.NET\Framework\v${NETVersion}" NETFrameworkInstalled 0
  File /oname=$TEMP\${NETInstaller} "Files\Redist\${NETInstaller}"

  DetailPrint "Starting Microsoft .NET Framework v${NETVersion} Setup..."
  ExecWait "$TEMP\${NETInstaller}"
  Return

  NETFrameworkInstalled:
  DetailPrint "Microsoft .NET Framework is already installed!"
SectionEnd

;--------------------------------
;Installer Functions

Function .onInit

  !insertmacro MUI_LANGDLL_DISPLAY

FunctionEnd

;--------------------------------