using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    public event Action OnSomeSceneStartLoading;
    public event Action OnMenuSceneLoaded;
    public event Action OnBoardSceneLoaded;

    [SerializeField] string menuScene;
    [SerializeField] string boardScene;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSomeSceneLoaded;
        }
    }

    //Public methods
    public void LoadMenuScene(bool async) => ChangeScene(menuScene, async);
    public void LoadBoardScene(bool async) => ChangeScene(boardScene, async);

    public void ChangeScene(string sceneName, bool async)
    {
        OnSomeSceneStartLoading?.Invoke();

        if (async)
            SceneManager.LoadSceneAsync(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    //Private methods
    private void OnSomeSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == menuScene)
            OnMenuSceneLoaded?.Invoke();
        else if (scene.name == boardScene)
            OnBoardSceneLoaded?.Invoke();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSomeSceneLoaded;
    }
}
