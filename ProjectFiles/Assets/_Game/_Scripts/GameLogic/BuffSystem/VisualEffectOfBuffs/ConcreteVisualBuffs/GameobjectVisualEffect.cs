using UnityEngine;

public class GameobjectVisualEffect : MonoBehaviour
{
    public void DoOnSpawn()
    {

    }

    public void DoOnTurned()
    {

    }

    public void DoOnRemoved()
    {
        Destroy(gameObject);
    }
}
