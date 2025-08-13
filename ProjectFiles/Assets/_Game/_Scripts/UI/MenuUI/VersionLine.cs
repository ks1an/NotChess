using TMPro;
using UnityEngine;

public sealed class VersionLine : MonoBehaviour
{
    TextMeshProUGUI _text;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _text.text = "1(NC)_" + $"<b>{Application.version}</b>";
    }
}
