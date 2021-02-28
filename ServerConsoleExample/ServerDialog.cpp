#include "pch.h"

#include "ServerConsoleApp.h"
#include "ServerDialog.h"
#include "Utils.h"

IMPLEMENT_DYNAMIC(ServerDialog, CDialogEx)

ServerDialog::ServerDialog(IGameServiceProvider* serviceProvider, CWnd* parent)
	: CDialogEx(IDD_SERVER, parent),
	m_serviceProvider(serviceProvider)
{
	const auto& pGameServer = m_serviceProvider->Get<IGameServer>();

	using namespace std::placeholders;
	m_playerConnectedEventHandle = pGameServer->Listen(GameServerEventType::PlayerConnected, std::bind(&ServerDialog::HandlePlayerConnected, this, _1));
	m_playerDisconnectedEventHandle = pGameServer->Listen(GameServerEventType::PlayerDisconnected, std::bind(&ServerDialog::HandlePlayerDisconnected, this, _1));
	m_playerChatMessageHandle = pGameServer->Listen(GameServerEventType::ChatMessage, std::bind(&ServerDialog::HandleChatMessage, this, _1));
	m_worldLoadedHandle = pGameServer->Listen(GameServerEventType::WorldLoaded, std::bind(&ServerDialog::HandleWorldLoaded, this, _1));
}

ServerDialog::~ServerDialog()
{
	try
	{
		const auto& pGameServer = m_serviceProvider->Get<IGameServer>();

		pGameServer->Unlisten(GameServerEventType::PlayerConnected, m_playerConnectedEventHandle);
		pGameServer->Unlisten(GameServerEventType::PlayerDisconnected, m_playerDisconnectedEventHandle);
		pGameServer->Unlisten(GameServerEventType::ChatMessage, m_playerChatMessageHandle);
		pGameServer->Unlisten(GameServerEventType::WorldLoaded, m_worldLoadedHandle);
	}
	catch (...)
	{

	}
}

void ServerDialog::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CONSOLE, ConsoleEditBox);
	DDX_Control(pDX, IDC_PLAYERS, PlayersListCtrl);
	DDX_Control(pDX, IDC_BAN, ButtonBan);
	DDX_Control(pDX, IDC_KICK, ButtonKick);
}

void __stdcall ServerDialog::TimerCallback(HWND hwnd, UINT uMsg, UINT idEvent, DWORD dwTime)
{
	((ServerDialog*)idEvent)->RefreshPlayers();
}

void ServerDialog::CloseDialog()
{
	OnCancel();
}

void ServerDialog::ShowDialog()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	Create(IDD_SERVER);
	ShowWindow(SW_SHOW);
}

BOOL ServerDialog::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CDialog::OnInitDialog();

	CreateColumns();

	using namespace std::placeholders;
	SetTimer((UINT_PTR)this, 1000, TimerCallback);

	return TRUE;
}

void ServerDialog::CreateColumns()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	PlayersListCtrl.InsertColumn(0, _T("Name"), LVCFMT_LEFT, 40);
	PlayersListCtrl.InsertColumn(1, _T("Status"), LVCFMT_LEFT, 80);
	PlayersListCtrl.InsertColumn(2, _T("Ping"), LVCFMT_LEFT, 40);
	PlayersListCtrl.InsertColumn(3, _T("Score"), LVCFMT_LEFT, 80);
	PlayersListCtrl.InsertColumn(4, _T("Team"), LVCFMT_LEFT, 80);

	for (int i = 0; i < NumColumns; i++)
	{
		PlayersListCtrl.SetColumnWidth(i, LVSCW_AUTOSIZE_USEHEADER);
	}
}

void ServerDialog::PostNcDestroy()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CDialog::PostNcDestroy();
}

void ServerDialog::OnOK()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (!UpdateData(TRUE))
		return;
	DestroyWindow();
}

void ServerDialog::OnCancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	KillTimer((UINT_PTR)this);
	ShowWindow(SW_HIDE);
	DestroyWindow();
}

void ServerDialog::RefreshPlayers()
{
	if (!m_hWnd)
	{
		return;
	}

	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	const int savedSelection = PlayersListCtrl.GetSelectionMark();
	PlayersListCtrl.DeleteAllItems();


	const auto& pTextLookupService = m_serviceProvider->Get<ITextLookupService>();
	const auto& pGameServer = m_serviceProvider->Get<IGameServer>();
	for (const auto& player : pGameServer->GetPlayers())
	{
		if (player->host)
		{
			continue; // Skip host player
		}

		LVITEM item{};
		item.cColumns = NumColumns;
		item.mask = LVIF_COLUMNS | LVIF_PARAM;
		item.lParam = player->index;

		const int index = PlayersListCtrl.InsertItem(&item);
		PlayersListCtrl.SetItemText(index, 0, player->name.c_str());
		PlayersListCtrl.SetItemText(index, 1, pTextLookupService->GetNetPlayerStateName(player->state).c_str());
		PlayersListCtrl.SetItemText(index, 2, fmt::format(_T("{0}"), player->ping).c_str());
		PlayersListCtrl.SetItemText(index, 3, fmt::format(_T("{0}"), player->score).c_str());
		PlayersListCtrl.SetItemText(index, 4, pTextLookupService->GetPlayerTeamName(player->team).c_str());
	}

	PlayersListCtrl.SetSelectionMark(savedSelection);
}

void ServerDialog::HandlePlayerConnected(const GameServerEvent& event)
{
	RefreshPlayers();
}

void ServerDialog::HandlePlayerDisconnected(const GameServerEvent& event)
{
	RefreshPlayers();
}

void ServerDialog::HandleChatMessage(const GameServerEvent& event)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	const auto& chatMessageEvent = (const ChatMessageEvent&)event;

	ConsoleEditBox.SetSel(-1, 0);
	ConsoleEditBox.ReplaceSel(chatMessageEvent.message.data());
	ConsoleEditBox.SetSel(-1, 0);
	ConsoleEditBox.ReplaceSel("\n");
}

void ServerDialog::HandleWorldLoaded(const GameServerEvent& event)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	const auto& pGameServer = m_serviceProvider->Get<IGameServer>();

	auto details = pGameServer->GetGameDetails();

	// TODO: Connect to world state controls
}

BEGIN_MESSAGE_MAP(ServerDialog, CDialogEx)
	ON_BN_CLICKED(IDC_BAN, &ServerDialog::OnBnClickedBan)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_PLAYERS, &ServerDialog::OnItemChanged)
	ON_BN_CLICKED(IDC_KICK, &ServerDialog::OnBnClickedKick)
END_MESSAGE_MAP()


// ServerDialog message handlers

void ServerDialog::OnBnClickedBan()
{
	const int selection = PlayersListCtrl.GetSelectionMark();
	if (selection == -1)
	{
		return;
	}

	const PlayerIndex playerIndex = (PlayerIndex)PlayersListCtrl.GetItemData(selection);
	if (playerIndex > 0)
	{
		const auto& pGameServer = m_serviceProvider->Get<IGameServer>();
		pGameServer->BanPlayer(playerIndex);
	}
}

void ServerDialog::OnBnClickedKick()
{
	const int selection = PlayersListCtrl.GetSelectionMark();
	if (selection == -1)
	{
		return;
	}

	const PlayerIndex playerIndex = (PlayerIndex)PlayersListCtrl.GetItemData(selection);
	if (playerIndex > 0)
	{
		const auto& pGameServer = m_serviceProvider->Get<IGameServer>();
		pGameServer->KickPlayer(playerIndex, KickReason::Removed);
	}
}

void ServerDialog::OnItemChanged(NMHDR* pNMHDR, LRESULT* pResult)
{
	const LPNMLISTVIEW pNMListView = (const LPNMLISTVIEW)(pNMHDR);

	if (!(pNMListView->uChanged & LVIF_STATE))
	{
		return;
	}

	if ((pNMListView->uOldState & LVIS_SELECTED) == (pNMListView->uNewState & LVIS_SELECTED))
	{
		return;
	}

	ButtonKick.EnableWindow(TRUE);
	ButtonBan.EnableWindow(TRUE);

	*pResult = 0;
}
