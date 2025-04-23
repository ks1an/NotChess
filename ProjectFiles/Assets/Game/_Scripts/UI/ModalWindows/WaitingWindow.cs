using TMPro;
using UnityEngine;

public sealed class WaitingWindow : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    readonly string[] loadingPhrases =
    {
        "Loading...",
        "Loading resources",
        "I'm counting the crosses",
        "I'm looking for zeros in my pockets",
        "I connect with the higher mind",
        "Beeeep beep!",
        "I'm making my move",
        "Thinking about something important. What if we take an existing name and add \"Not\" to it?"
    };

    public void SetLoadText()
    {
        int index;
        index = Random.Range(0, loadingPhrases.Length);
        text.text = loadingPhrases[index];
    }
}
