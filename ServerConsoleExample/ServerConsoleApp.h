#pragma once

#ifndef __AFXWIN_H__
	#error "include 'pch.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols
#include "ServerDialog.h"
#include <IGameServerConsole.h>
#include <ComponentBase.h>

class ServerConsoleApp : public CWinApp
{
public:
	~ServerConsoleApp();

// MFC methods
	BOOL InitInstance() override;
	BOOL ExitInstance() override;

	IGameServerConsole* InitializeDialog(IComponentContainer* container);

	DECLARE_MESSAGE_MAP()
};
