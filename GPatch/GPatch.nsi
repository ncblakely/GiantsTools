Unicode True
SetCompressor /SOLID lzma

!define PRODUCT_NAME "Giants: Citizen Kabuto"
!define PRODUCT_VERSION "1.498"

; MUI 1.67 compatible ------
!include "MUI2.nsh"
!include "DotNetChecker.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "GPatch.ico"

; Welcome page
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

!include LogicLib.nsh

; MUI end ------

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "Output\GPatch1_498_206_0.exe"
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
  ${EndIf}
  
  ExecWait "$INSTDIR\Redist\VC_redist.x86.exe /install /passive /norestart" $0
  ${If} $0 != 0
	MessageBox MB_OK "Setup failed to install the Visual C++ Runtime. Please visit www.microsoft.com and download the latest version of the Visual C++ redistributable."
  ${EndIf}

  RMDir /r "$INSTDIR\Redist" ; Delete temporary files
  
  ; Delete old files
  Delete $INSTDIR\bin\Shaders\*.*
  Delete $INSTDIR\gg_dx7r.dll
  Delete $INSTDIR\gg_dx8r.dll
  Delete $INSTDIR\gg_dx9r.dll  
  Delete $INSTDIR\Giants.exe
  Delete $INSTDIR\BugTrap.dll
  Delete $INSTDIR\GiantsMain.exe
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
  Delete $INSTDIR\bin\GData.gbt
  Delete $INSTDIR\bin\GData.h
  
SectionEnd

!define NETVersion "4.7.2"
!define NETInstallerFileName "NDP472-KB4054531-Web.exe"
!define NETInstallerPath "Files\Redist\NDP472-KB4054531-Web.exe"

Section "MS .NET Framework v${NETVersion}" SecFramework
  IfFileExists "$WINDIR\Microsoft.NET\Framework\v${NETVersion}" NETFrameworkInstalled 0
  File /oname=$TEMP\${NETInstallerFileName} "${NETInstallerPath}"
  
  !insertmacro CheckNetFramework 472
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