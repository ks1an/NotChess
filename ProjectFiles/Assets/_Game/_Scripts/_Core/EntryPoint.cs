using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public sealed class EntryPoint : MonoBehaviour
{
    [SerializeField] float delayForLoadServices;
    float time;
    bool menuSceneStartedLoading;

    async void Awake()
    {
        WaitingWindowController.Instance.Show();
        time = 0;
        menuSceneStartedLoading = false;

        try
        {
            InitializationOptions initializationOptions = new();
            initializationOptions.SetProfile("Player45510");
            await UnityServices.InitializeAsync(initializationOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError("SERVICES NOT INIT. Error: " + e);
        }
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time >= delayForLoadServices && !menuSceneStartedLoading)
        {
            menuSceneStartedLoading = true;
            time = 0;
            SceneLoader.Instance.LoadMenuScene(true);
        }
    }

    void OnDisable()
    {
        LoadPlayerDeck();
        WaitingWindowController.Instance.Hide();
    }

    void LoadPlayerDeck()
    {
        var gc = GameController.Instance;
        if (gc == null || gc.player == null || gc.player.UniqueDeck == null)
        {
            Debug.LogError("EntryPoint: deck service not ready — check GameController.Awake order");
            return;
        }

        if (DeckLibraryManager.Instance != null)
            DeckLibraryManager.Instance.Initialize();
        else
            gc.player.UniqueDeck.Load();
    }
}