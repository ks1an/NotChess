using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : MonoBehaviour
{
    public const string menuScene = "MenuScene";
    public const string boardScene = "BoardScene";

    public static SceneLoader Instance { get; private set; }

    public event System.Action OnSomeSceneStartLoading;
    public event System.Action OnMenuSceneLoaded;
    public event System.Action OnBoardSceneLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSomeSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            Debug.LogError("SceneLoader > 0 in scene. Destroying duplicate.");
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSomeSceneLoaded;
    }

    public bool IsBoardScene() { return boardScene == SceneManager.GetActiveScene().name; }
    public bool IsMenuScene() { return menuScene == SceneManager.GetActiveScene().name; }

    public void LoadMenuScene(bool async) => ChangeScene(menuScene, async);
    public void LoadBoardScene(bool async) => ChangeScene(boardScene, async);


    private void ChangeScene(string sceneName, bool async)
    {
        OnSomeSceneStartLoading?.Invoke();

        if (async)
            SceneManager.LoadSceneAsync(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    private void OnSomeSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == menuScene)
            OnMenuSceneLoaded?.Invoke();
        else if (scene.name == boardScene)
            OnBoardSceneLoaded?.Invoke();
    }
}
