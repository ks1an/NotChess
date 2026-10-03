using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CycleButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI valueText;

    [SerializeField] List<string> options = new();

    int currentIndex;

    public int CurrentIndex => currentIndex;
    public int OptionsCount => options.Count;
    public event Action<int> OnValueChanged;

    void Awake()
    {
        if (button == null) Debug.LogError("Button == null on cycle bttn: " + gameObject.name);

        button.onClick.AddListener(Next);
    }

    public void DisplayValue(int index)
    {
        if (options.Count == 0) return;

        currentIndex = Mathf.Clamp(index, 0, options.Count - 1);
        valueText.text = options[currentIndex];
    }

    void Next()
    {
        if (options.Count == 0) return;

        currentIndex = (currentIndex + 1) % options.Count;
        valueText.text = options[currentIndex];

        OnValueChanged?.Invoke(currentIndex);
    }

    public void ClearListeners()
    {
        OnValueChanged = null;
    }

    void OnDestroy()
    {
        button.onClick.RemoveListener(Next);
        OnValueChanged = null;
    }
}