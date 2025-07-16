using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class OnAppLoad : MonoBehaviour
{
    async void Awake()
    {
        try
        {
            InitializationOptions initializationOptions = new();
            initializationOptions.SetProfile("Player45510");
            await UnityServices.InitializeAsync(initializationOptions);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (ServicesInitializationException) 
        {
            WaitingWindowController.Instance.ShowOnNetServicesNotInit();
        }
    }
    void Start()
    {
        WaitingWindowController.Instance.ShowWithRandomTxt();
        SceneManager.LoadSceneAsync("MenuScene");
    }

    void OnDisable()
    {
        WaitingWindowController.Instance.Hide();
    }
}
