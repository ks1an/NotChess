using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

//Hangs on an object in the EntryPoint scene
public sealed class EntryPoint : MonoBehaviour
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
