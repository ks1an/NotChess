using TMPro;
using UnityEngine;

public sealed class LoadText : MonoBehaviour
{
    TextMeshProUGUI text;
    string[] loadingPhrases =
    {
        "Loading...", "Loading resources",
        "I'm counting the crosses", "I'm looking for zeros in my pockets",
        "I connect with the higher mind", "Beeeep beep!", "I'm making my move",
        "Thinking about something important. What if we take an existing name and add \"Not\" to it?"
    };


    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = loadingPhrases[Random.Range(0, loadingPhrases.Length)];
    }
}
