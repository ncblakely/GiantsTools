SetCompressor /SOLID lzma

!define PRODUCT_NAME "Giants Launcher"
!define PRODUCT_VERSION "1.0.0.2"

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

!define MUI_LANGDLL_REGISTRY_ROOT "HKCU"
!define MUI_LANGDLL_REGISTRY_KEY "Software\PlanetMoon\Giants"
!define MUI_LANGDLL_REGISTRY_VALUENAME "SetupLanguage"

; Language files
!insertmacro MUI_LANGUAGE "English"

; MUI end ------

Name "Giants Launcher Update"
OutFile "LauncherUpdate_1002.exe"
InstallDir "C:\Program Files\Giants"
InstallDirRegKey HKCU "SOFTWARE\PlanetMoon\Giants" "DestDir"
ShowInstDetails hide

;Request application privileges for Windows Vista
RequestExecutionLevel admin

Section
  SetDetailsView hide
  SectionIn RO
  SetOverwrite on
  
  
  SetOutPath "$INSTDIR"
  File /r "Giants.exe"

  
SectionEnd

Function .onInit
        Processes::KillProcess "Giants.exe"
        Processes::FindProcess "Giants.exe"
        ${If} $R0 == 1
              MessageBox MB_OK "Please close the Giants launcher before installing this update."
              Abort
        ${EndIf}
        
        ClearErrors
        FileOpen $R0 "$INSTDIR\Giants.exe" w
        ${If} ${Errors}
              MessageBox MB_OK "Could not write to Giants.exe. Please ensure the Giants launcher is closed."
              Abort
        ${Else}
               FileClose $R0
        ${EndIf}
FunctionEnd

Function .onInstFailed
         MessageBox MB_OK "Update failed. Please visit www.giantswd.org and download the latest version manually."
FunctionEnd

Function .onInstSuccess
         MessageBox MB_OK "Update complete!"
         Exec "$INSTDIR\Giants.exe"
FunctionEnd