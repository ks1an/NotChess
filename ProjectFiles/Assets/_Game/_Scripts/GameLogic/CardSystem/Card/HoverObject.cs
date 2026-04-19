using System;
using UnityEngine;

public sealed class HoverObject : MonoBehaviour
{
    Action<GameObject> actionOnMouseEnter, actionOnMouseExit;
    Color highlihgtColor, standartColor;

    Material material;

    public void OnMouseEnter()
    {
        actionOnMouseEnter?.Invoke(gameObject);
    }

    public void OnMouseExit()
    {
        actionOnMouseExit?.Invoke(gameObject);
    }

    public void SetSettigns(Action<GameObject> onMouseEnter, Action<GameObject> onMouseExit,
        bool needToHighlight = false, Color highlihgtColor = new Color())
    {
        if (needToHighlight)
        {
            this.highlihgtColor = highlihgtColor;
            material = GetComponent<Renderer>().material;
            standartColor = material.color;
            actionOnMouseEnter = (GameObject) => { ToogleHighlight(true); onMouseEnter.Invoke(gameObject); };
            actionOnMouseExit = (GameObject) => { ToogleHighlight(false); onMouseExit.Invoke(gameObject); };
        }
        else
        {
            actionOnMouseEnter = onMouseEnter;
            actionOnMouseExit = onMouseExit;
        }
    }

    void ToogleHighlight(bool turnOn)
    {
        if (turnOn)
        {
            material.color = highlihgtColor;
        }
        else
        {
            material.color = standartColor;
        }
    }
}
