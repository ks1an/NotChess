using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public sealed class EntryPoint : MonoBehaviour
{
    [SerializeField] float delayForLoadServices;
    float time;
    async void Awake()
    {
        WaitingWindowController.Instance.ShowWithRandomTxt();
        time = 0;

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
        if(time >= delayForLoadServices)
        {
            SceneLoader.Instance.LoadMenuScene(true);
        }
    }

    void OnDisable()
    {
        WaitingWindowController.Instance.Hide();
    }
}
