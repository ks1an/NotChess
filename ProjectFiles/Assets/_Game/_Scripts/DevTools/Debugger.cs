using UnityEngine;

public sealed class Debugger : MonoBehaviour
{
    public static Debugger Instance { get; private set; }
    private void Awake()
    {
        if(Application.isEditor == false)
        {
            Destroy(gameObject);
        }

        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Debugger > 1 on scene");
        }
    }

    #if UNITY_EDITOR
    public void DebugError(string message)
    {
        Debug.LogError(message);
    }

    public void DebugWarning(string message)
    {
        Debug.LogWarning(message);
    }

    public void DebugLog(string message)
    {
        Debug.LogError(message);
    }
#endif
}
