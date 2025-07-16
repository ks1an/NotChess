using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public sealed class Relay : MonoBehaviour
{
    public static Relay Instance;
    [HideInInspector] public string joinCode;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            WaitingWindowController.Instance.ShowWithRandomTxt();
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(LobbyManager.Instance.GetJoinedLobby().MaxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            this.joinCode = joinCode;

            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            MatchController.Instance.CreateGame(true);
            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException e) { Debug.Log(e); return null; }
    }

    public async void JoinRelay(string code)
    {
        try
        {
            WaitingWindowController.Instance.ShowWithRandomTxt();
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(code);
            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            MatchController.Instance.CreateGame(true);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e) { Debug.Log(e); }
    }
}
