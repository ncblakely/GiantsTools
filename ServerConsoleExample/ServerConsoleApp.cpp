#include "pch.h"
#include "ServerConsoleApp.h"
#include "ServerDialog.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

BEGIN_MESSAGE_MAP(ServerConsoleApp, CWinApp)
END_MESSAGE_MAP()

ServerConsoleApp ConsoleApp;
BOOL ServerConsoleApp::InitInstance()
{
	INITCOMMONCONTROLSEX InitCtrls;
	InitCtrls.dwSize = sizeof(InitCtrls);
	InitCtrls.dwICC = ICC_WIN95_CLASSES;
	InitCommonControlsEx(&InitCtrls);

	CWinApp::InitInstance();
	
	return TRUE;
}

ServerConsoleApp::~ServerConsoleApp()
{
}

BOOL ServerConsoleApp::ExitInstance()
{
	return CWinApp::ExitInstance();
}

void ServerConsoleApp::InitializeDialog(IGameServiceProvider* serviceProvider)
{
	// Create the server console window.
	// As this is also a Component, Giants will clean up this object automatically once
	// it is no longer needed (i.e, there is no need to call delete).
	auto dialog = std::make_shared<ServerDialog>(serviceProvider);
	m_pMainWnd = dialog.get();

	serviceProvider->Add<IGameServerConsole>(dialog);
}

__declspec(dllexport) void CreateServerConsole(
	int apiVersion, 
	HWND hWnd,
	IGameServiceProvider* serviceProvider)
{
	if (apiVersion > 1)
	{
		throw std::invalid_argument(fmt::format("Unsupported API version {0}", apiVersion).c_str());
	}

	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	ConsoleApp.InitializeDialog(serviceProvider);
}