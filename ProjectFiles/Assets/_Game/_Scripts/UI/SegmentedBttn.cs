using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegmentedBttn : MonoBehaviour
{
    [Serializable]
    public class Segment
    {
        public Button button;
        public Image background;
        public TextMeshProUGUI textValue;
    }

    [SerializeField] List<Segment> segments = new();

    [Header("Colors")]
    [SerializeField] Color selectedBg = new(1f, 0.85f, 0.2f);
    [SerializeField] Color selectedText = new(0.1f, 0.1f, 0.1f);
    [SerializeField] Color normalBg = new(0.2f, 0.25f, 0.35f);
    [SerializeField] Color normalText = Color.white;

    int currentIndex;

    public int CurrentIndex => currentIndex;
    public int SegmentsCount => segments.Count;
    public event Action<int> OnValueChanged;

    void Awake()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            int index = i;
            segments[i].button.onClick.AddListener(() => Select(index));
        }
    }

    public void DisplayValue(int index)
    {
        if (segments.Count == 0) return;

        currentIndex = Mathf.Clamp(index, 0, segments.Count - 1);
        ApplyVisuals(currentIndex);
    }

    void Select(int index)
    {
        if (index == currentIndex) return;
        if (index < 0 || index >= segments.Count) return;

        currentIndex = index;
        ApplyVisuals(currentIndex);

        OnValueChanged?.Invoke(currentIndex);
    }

    void ApplyVisuals(int activeIndex)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            bool isActive = i == activeIndex;

            if (segments[i].background != null)
                segments[i].background.color = isActive ? selectedBg : normalBg;

            if (segments[i].textValue != null)
                segments[i].textValue.color = isActive ? selectedText : normalText;
        }
    }

    public void ClearListeners() => OnValueChanged = null;

    void OnDestroy()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i].button != null)
                segments[i].button.onClick.RemoveAllListeners();
        }
        OnValueChanged = null;
    }
}
