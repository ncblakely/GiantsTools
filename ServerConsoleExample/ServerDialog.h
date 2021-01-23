#pragma once

#include "Network/Public/IGameServer.h"
#include "Network/Public/IGameServerConsole.h"
#include "Core/Public/IGameServiceProvider.h"
#include "Core/Public/ITextLookupService.h"

// ServerDialog dialog

class ServerDialog : public CDialogEx, public IGameServerConsole
{
	DECLARE_DYNAMIC(ServerDialog)

public:
	~ServerDialog();
	ServerDialog(IGameServiceProvider* container, CWnd* parent = nullptr);

	void CreateColumns();
	void RefreshPlayers();

	static void STDMETHODCALLTYPE TimerCallback(HWND hwnd, UINT uMsg, UINT idEvent, DWORD dwTime);
	void CloseDialog() override;
	void ShowDialog() override;

	void HandlePlayerConnected(const GameServerEvent& event);
	void HandlePlayerDisconnected(const GameServerEvent& event);
	void HandleChatMessage(const GameServerEvent& event);
	void HandleWorldLoaded(const GameServerEvent& event);

	// MFC methods
	BOOL OnInitDialog() override;
	void PostNcDestroy() override;
	void OnOK() override;
	void OnCancel() override;

// Dialog Data
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_SERVER };
#endif

private:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()

	CEdit ConsoleEditBox;
	CListCtrl PlayersListCtrl;
	CButton ButtonBan;
	CButton ButtonKick;

	afx_msg void OnBnClickedKick();
	afx_msg void OnBnClickedBan();
	afx_msg void OnItemChanged(NMHDR* pNMHDR, LRESULT* pResult);

	UUID m_playerConnectedEventHandle{};
	UUID m_playerDisconnectedEventHandle{};
	UUID m_playerChatMessageHandle{};
	UUID m_worldLoadedHandle{};
	IGameServiceProvider* m_serviceProvider{};

	const int NumColumns = 5;
};
