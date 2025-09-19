using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Localization;

public sealed class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_START_GAME = "0";
    public const string KEY_GAME_VERSION = "0.0.0.0";

    #region Events
    public event EventHandler OnLeftLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<EventArgs> OnLobbyGameStarted;
    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }
    #endregion

    [SerializeField] GameObject lobbyList;
    [SerializeField] LocalizedStringTable localTable;
    [SerializeField] float refreshLobbyListTimer = 5f;

    Lobby joinedLobby;

    float heartbeatTimer;
    float lobbyPollTimer;
    string playerName;
    string currentGameVersion;

    void Awake()
    {
        Instance = this;
        playerName = EditPlayerName.Instance.GetPlayerName();
        currentGameVersion = Application.version;
    }

    void Update()
    {
        HandleRefreshLobbyList(); // Disable if you test multiple builds! Invoke "more request"
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    #region Handle
    private void HandleRefreshLobbyList()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn && lobbyList.activeSelf)
        {
            refreshLobbyListTimer -= Time.deltaTime;
            if (refreshLobbyListTimer < 0f)
            {
                float refreshLobbyListTimerMax = 5f;
                refreshLobbyListTimer = refreshLobbyListTimerMax;

                RefreshLobbyList();
            }
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (joinedLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                float lobbyPollTimerMax = 1.1f;
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                if (!IsPlayerInLobby())
                {
                    //Kicked from Lobby!
                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    joinedLobby = null;
                }
                else if (joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    if (!IsLobbyHost())
                        Relay.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);

                    joinedLobby = null;
                    OnLobbyGameStarted?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
    #endregion

    #region return anything
    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Unity.Services.Lobbies.Models.Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                    return true;
            }
        }
        return false;
    }

    private Unity.Services.Lobbies.Models.Player GetPlayer()
    {
        return new Unity.Services.Lobbies.Models.Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
        });
    }
    #endregion

    public void SetActiveLobbyList(bool active) => lobbyList.SetActive(active);

    #region HostCanDo
    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
    {
        Unity.Services.Lobbies.Models.Player player = GetPlayer();

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject>
            {
                {KEY_START_GAME, new DataObject(visibility: DataObject.VisibilityOptions.Member, value: "0") },
                {KEY_GAME_VERSION, new DataObject(DataObject.VisibilityOptions.Public, currentGameVersion, index: DataObject.IndexOptions.S1) }
            }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                string relayCode = await Relay.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                joinedLobby = lobby;
            }
            catch (LobbyServiceException) { }
        }
    }
    #endregion

    #region EveryoneCanDo
    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new()
            {
                Count = 25,

                // Filter for open lobbies only
                Filters = new List<QueryFilter>
                {
                    new(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0"),
                    new(QueryFilter.FieldOptions.S1, currentGameVersion, QueryFilter.OpOptions.EQ)
                },

                // Order by newest lobbies first
                Order = new List<QueryOrder> {
                    new(
                        asc: false,
                        field: QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        }
        catch (LobbyServiceException)
        {
            //Debug.Log(e);
        }
    }

    public async void UpdatePlayerName(string playerName)
    {
        this.playerName = playerName;

        if (joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_NAME, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: playerName)
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
            catch (LobbyServiceException)
            {
                //Debug.Log(e);
            }
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;

                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
    #endregion

    #region ClientCanDo
    public async void JoinLobbyByCode(string lobbyCode)
    {
        Unity.Services.Lobbies.Models.Player player = GetPlayer();

        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions
        {
            Player = player
        });

        if(!lobby.Data.TryGetValue(KEY_GAME_VERSION, out DataObject versionData) || versionData.Value != currentGameVersion)
        {
            ModalViewWindowController.Instance.ShowHorizontal(false, "Version does not match",
                $"Your version:{currentGameVersion}\n" +
                $"Lobby Version:{versionData.Value}", true, altTxt: "Ok", altAction: () => { });
            return;
        }

        joinedLobby = lobby;
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    public async void JoinLobby(Lobby lobby)
    {
        Unity.Services.Lobbies.Models.Player player = GetPlayer();

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
        {
            Player = player
        });

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    public async void QuickJoinLobby()
    {
        try
        {
            Unity.Services.Lobbies.Models.Player player = GetPlayer();
            QuickJoinLobbyOptions options = new()
            {
                Player = player,
                Filter = new List<QueryFilter> { }
            };
            options.Filter.Add(new QueryFilter(QueryFilter.FieldOptions.S1, currentGameVersion,
                    QueryFilter.OpOptions.EQ));

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException)
        {
            ModalViewWindowController.Instance.ShowHorizontalWithLocalize(
                localTable, "LobbyNotFound",
                false, true, altAction: () => { }
                );
        }
    }
    #endregion
}
