using System;
using UnityEngine;

public sealed class HoverObject : MonoBehaviour
{
    Action<GameObject> actionOnMouseEnter, actionOnMouseExit;

    public void SetSettigns(Action<GameObject> onMouseEnter, Action<GameObject> onMouseExit)
    {
        actionOnMouseEnter = onMouseEnter;
        actionOnMouseExit = onMouseExit;
    }

    void OnMouseEnter()
    {
        actionOnMouseEnter?.Invoke(gameObject);
    }
    void OnMouseExit()
    {
        actionOnMouseExit?.Invoke(gameObject);
    }
}
