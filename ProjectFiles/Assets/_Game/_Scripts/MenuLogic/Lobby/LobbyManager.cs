using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Localization;

public sealed class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }
    public string CurrentGameMode { get; private set; }

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_MMR = "PlayerMmr";

    public const string KEY_RELAY_CODE = "RelayCode";
    public const string KEY_GAME_VERSION = "GameVersion";
    public const string KEY_GAME_MODE = "GameMode";
    public const string KEY_START_STATUS = "StartStatus";

    public const string LOBBY_STATUS_WAITING = "Waiting";
    public const string LOBBY_STATUS_STARTED = "Started";

    #region Events
    public event EventHandler OnLeftLobby;
    public event EventHandler OnMatchmakerCancelled;

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

    public bool IsMatchmakingLobby => isMatchmakingLobby;
    public bool IsMatchmakingCancelled => isMatchmakingCancelled;
    public bool IsWaitingForMatchmaker =>
     isMatchmakingLobby && joinedLobby != null && joinedLobby.Players.Count < 2;
    float matchmakerWaitStartTime;

    [SerializeField] GameObject lobbyList;
    [SerializeField] EditPlayerName playerEdit;
    [SerializeField] LocalizedStringTable localTable;
    [SerializeField] float refreshLobbyListTimer = 5f;
    [SerializeField] float lobbyPollTimerMax = 2f;
    [SerializeField] float refreshLobbyListTimerMax = 5f;
    [SerializeField] float heartbeatTimerMax = 15f;


    Lobby joinedLobby;

    float heartbeatTimer;
    float lobbyPollTimer;
    string playerName;
    string currentGameVersion;

    bool isMatchmakingCancelled;
    bool isMatchmakingLobby;
    bool isStartingGame;
    bool isPolling;

    void Awake()
    {
        Instance = this;
        playerName = GameController.Instance.playerData.PlayerName.Value;
        currentGameVersion = Application.version;
    }

    void Update()
    {
        HandleRefreshLobbyList(); // Disable if you test multiple builds! Invoke "more request"
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    public void CancelMatchmaker()
    {
        isMatchmakingCancelled = true;
        WaitingWindowController.Instance.Hide();
        InLobbyUI.Instance.Hide();

        if (joinedLobby != null)
        {
            LeaveLobby();
            return;
        }

        OnMatchmakerCancelled?.Invoke(this, EventArgs.Empty);
    }

    #region Handle
    private void HandleRefreshLobbyList()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn && lobbyList.activeSelf)
        {
            refreshLobbyListTimer -= Time.deltaTime;
            if (refreshLobbyListTimer < 0f)
            {
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
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (joinedLobby == null || isPolling) return;

        lobbyPollTimer -= Time.deltaTime;
        if (lobbyPollTimer > 0f) return;
        lobbyPollTimer = lobbyPollTimerMax;

        isPolling = true;

        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = lobby;

            OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

            if (!IsPlayerInLobby())
            {
                bool wasMatchmaker = CurrentGameMode == MatchStates.GameMode_Matchmaking;
                joinedLobby = null;
                isStartingGame = false;
                isMatchmakingLobby = false;

                if (wasMatchmaker)
                    OnMatchmakerCancelled?.Invoke(this, EventArgs.Empty);
                else
                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = null });

                return;
            }

            if (joinedLobby.Data.TryGetValue(KEY_START_STATUS, out var statusData)
                && statusData.Value == LOBBY_STATUS_STARTED)
            {
                if (!IsLobbyHost()
                    && joinedLobby.Data.TryGetValue(KEY_RELAY_CODE, out var codeData))
                {
                    Relay.Instance.JoinRelay(codeData.Value);
                }

                joinedLobby = null;
                isMatchmakingLobby = false;
                isStartingGame = false;
                OnLobbyGameStarted?.Invoke(this, EventArgs.Empty);
            }
            else if (IsLobbyHost() && !isStartingGame && isMatchmakingLobby && joinedLobby.Players.Count >= 2)
            {
                StartGame();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"[Lobby poll] {e.Reason}: {e.Message}");

            switch (e.Reason)
            {
                case LobbyExceptionReason.RateLimited:
                    lobbyPollTimer = 3f;
                    break;

                case LobbyExceptionReason.LobbyNotFound:
                case LobbyExceptionReason.EntityNotFound:
                    if (joinedLobby == null) break;

                    bool wasMatchmaker = CurrentGameMode == MatchStates.GameMode_Matchmaking;
                    joinedLobby = null;
                    isStartingGame = false;
                    isMatchmakingLobby = false;

                    if (wasMatchmaker)
                        OnMatchmakerCancelled?.Invoke(this, EventArgs.Empty);
                    else
                        OnLeftLobby?.Invoke(this, EventArgs.Empty);
                    break;

                default:
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Lobby poll] Unexpected: {e.Message}");
        }
        finally
        {
            isPolling = false;
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
        return new Unity.Services.Lobbies.Models.Player(
            AuthenticationService.Instance.PlayerId,
            null,
            new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                { KEY_PLAYER_MMR,  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, RatingService.Instance.Data.CurMmr.ToString()) },
            });
    }
    #endregion

    public void SetActiveLobbyList(bool active) => lobbyList.SetActive(active);

    #region Matchmaker
    public async void Matchmaker()
    {
        if (joinedLobby != null) return;
        isMatchmakingCancelled = false;

        try
        {
            Unity.Services.Lobbies.Models.Player player = GetPlayer();

            QuickJoinLobbyOptions options = new()
            {
                Player = player,
                Filter = new List<QueryFilter>
                {
                    new(QueryFilter.FieldOptions.S1, currentGameVersion, QueryFilter.OpOptions.EQ),
                    new(QueryFilter.FieldOptions.AvailableSlots, "1", QueryFilter.OpOptions.EQ),
                   new(QueryFilter.FieldOptions.S2, MatchStates.GameMode_Matchmaking, QueryFilter.OpOptions.EQ),
                }
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            CurrentGameMode = MatchStates.GameMode_Matchmaking;
            if (isMatchmakingCancelled)
            {
                await LobbyService.Instance.RemovePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId);
                isMatchmakingCancelled = false;
                CurrentGameMode = null;
                return;
            }

            isMatchmakingLobby = false;
            joinedLobby = lobby;
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException e)
        {
            if (isMatchmakingCancelled)
            {
                isMatchmakingCancelled = false;
                CurrentGameMode = null;
                return;
            }

            if (e.Reason == LobbyExceptionReason.LobbyNotFound
                || e.Reason == LobbyExceptionReason.EntityNotFound
                || e.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                await CreateMatchmakerLobby();
            }
            else
            {
                Debug.LogWarning($"[Matchmaker] QuickJoin failed: {e.Reason}");
                ModalViewWindowController.Instance.ShowHorizontal(false, "[Matchmaker] QuickJoin failed:",
                    $"Context: {e.Reason}\n", true, altTxt: "Ok", altAction: () => { });
            }
        }
    }

    private async Task CreateMatchmakerLobby()
    {
        try
        {
            Unity.Services.Lobbies.Models.Player player = GetPlayer();

            CreateLobbyOptions options = new()
            {
                Player = player,
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_STATUS, new DataObject(DataObject.VisibilityOptions.Member, LOBBY_STATUS_WAITING) },
                    { KEY_RELAY_CODE,   new DataObject(DataObject.VisibilityOptions.Member, "") },
                    { KEY_GAME_VERSION, new DataObject(DataObject.VisibilityOptions.Public, currentGameVersion, index: DataObject.IndexOptions.S1) },
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, MatchStates.GameMode_Matchmaking, index: DataObject.IndexOptions.S2) },
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(MatchStates.GameMode_Matchmaking, 2, options);
            CurrentGameMode = MatchStates.GameMode_Matchmaking;

            if (isMatchmakingCancelled)
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
                isMatchmakingCancelled = false;
                CurrentGameMode = null;
                return;
            }

            matchmakerWaitStartTime = Time.realtimeSinceStartup;
            isMatchmakingLobby = true;
            joinedLobby = lobby;
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException e)
        {
            isMatchmakingCancelled = false;
            Debug.LogError(e);
        }
    }
    #endregion

    #region HostCanDo
    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
    {
        Unity.Services.Lobbies.Models.Player player = GetPlayer();

        CreateLobbyOptions options = new()
        {
            Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_STATUS, new DataObject(DataObject.VisibilityOptions.Member, LOBBY_STATUS_WAITING) },
                    { KEY_RELAY_CODE,   new DataObject(DataObject.VisibilityOptions.Member, "") },
                    {KEY_GAME_VERSION, new DataObject(DataObject.VisibilityOptions.Public, currentGameVersion, index: DataObject.IndexOptions.S1) },
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, MatchStates.GameMode_CustomLobby, index: DataObject.IndexOptions.S2) },
                }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        CurrentGameMode = MatchStates.GameMode_CustomLobby;

        joinedLobby = lobby;
        isMatchmakingLobby = false;
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
        if (!IsLobbyHost() || isStartingGame) return;
        isStartingGame = true;

        try
        {
            string relayCode = await Relay.Instance.CreateRelay();
            if (string.IsNullOrEmpty(relayCode))
            {
                ModalViewWindowController.Instance.ShowHorizontal(false, "Relay code is empty",
    $"Relay code: {relayCode}\n", true, altTxt: "Ok", altAction: () => { });
                isStartingGame = false;
                return;
            }

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                        {
                            { KEY_START_STATUS, new DataObject(DataObject.VisibilityOptions.Member, LOBBY_STATUS_STARTED) },
                            { KEY_RELAY_CODE,   new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                        }
            });

            joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"[StartGame] Lobby: {e.Reason}: {e.Message}");
            isStartingGame = false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[StartGame] {e.GetType().Name}: {e.Message}");
            isStartingGame = false;
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

                Filters = new List<QueryFilter>
                {
                    new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new(QueryFilter.FieldOptions.S1, currentGameVersion, QueryFilter.OpOptions.EQ),
                    new(QueryFilter.FieldOptions.S2, MatchStates.GameMode_Matchmaking, QueryFilter.OpOptions.NE),
                },

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

        if (joinedLobby == null) return;

        try
        {
            UpdatePlayerOptions options = new()
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                    { KEY_PLAYER_MMR,  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, RatingService.Instance.Data.CurMmr.ToString()) },
                }
            };

            string playerId = AuthenticationService.Instance.PlayerId;
            Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
            joinedLobby = lobby;

            OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby == null) return;

        bool wasMatchmaker = CurrentGameMode == MatchStates.GameMode_Matchmaking;
        string lobbyId = joinedLobby.Id;
        bool iAmHost = IsLobbyHost();

        joinedLobby = null;
        isStartingGame = false;
        isMatchmakingLobby = false;
        isMatchmakingCancelled = false;

        try
        {
            if (iAmHost && wasMatchmaker)
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            else
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"[LeaveLobby] {e.Reason}: {e.Message}");
        }

        if (wasMatchmaker)
            OnMatchmakerCancelled?.Invoke(this, EventArgs.Empty);
        else
            OnLeftLobby?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region ClientCanDo
    public async void JoinLobbyByCode(string lobbyCode)
    {
        isMatchmakingLobby = false;
        Unity.Services.Lobbies.Models.Player player = GetPlayer();

        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions
        {
            Player = player
        });

        if (!lobby.Data.TryGetValue(KEY_GAME_VERSION, out DataObject versionData) || versionData.Value != currentGameVersion)
        {
            ModalViewWindowController.Instance.ShowHorizontal(false, "Version does not match",
                $"Your version:{currentGameVersion}\n" +
                $"Lobby Version:{versionData.Value}", true, altTxt: "Ok", altAction: () => { });
            return;
        }
        CurrentGameMode = ReadGameModeFromLobby(lobby);
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

        CurrentGameMode = ReadGameModeFromLobby(lobby);
        isMatchmakingLobby = false;
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
            CurrentGameMode = ReadGameModeFromLobby(lobby);
            isMatchmakingLobby = false;
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

    string ReadGameModeFromLobby(Lobby lobby)
    {
        if (lobby?.Data == null) return MatchStates.GameMode_CustomLobby;
        if (!lobby.Data.TryGetValue(KEY_GAME_MODE, out var data)) return MatchStates.GameMode_CustomLobby;
        if (string.IsNullOrEmpty(data.Value)) return MatchStates.GameMode_CustomLobby;
        return data.Value;
    }
}
