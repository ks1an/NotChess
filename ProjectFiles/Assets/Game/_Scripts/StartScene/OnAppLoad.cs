using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class OnAppLoad : MonoBehaviour
{
    async void Awake()
    {
        InitializationOptions initializationOptions = new();
        initializationOptions.SetProfile("Player45510");
        await UnityServices.InitializeAsync(initializationOptions);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        SceneManager.LoadSceneAsync("MenuScene");
    }
    void Start()
    {
        WaitingWindowController.Instance.Show();
    }

    void OnDisable()
    {
        WaitingWindowController.Instance.Hide();
    }
}
