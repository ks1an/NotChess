using TMPro;
using UnityEngine;

public sealed class WaitingWindow : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textGUI;
    /*readonly string[] loadingPhrases_CurLineGod;
    readonly string[] loadingPhrases_MultiDvForCurGod;
    readonly string[] loadingPhrases_MultiDvForObserver;*/


    readonly string[] loadingPhrasesCurLine =
    {
        "Loading...",
        "Loading resources",
        "I'm counting the crosses",
        "I'm looking for zeros in my pockets",
        "I connect with the higher mind",
        "Beeeep beep!",
        "I'm making my move",
        "Thinking about something important. What if we take an existing name and add \"Not\" to it?",
        "Arranging the pieces",
        "Blowing away the dust",
        "I'm ordering cement for the field",
        "Sorting crosses and zeros",
        "The figures touch the grass, try it yourself",
        "Drawing cards",
        "I draw crosses on the calendar",
        "If you find a bug, let me know, and I'll come up with an exoneration",
        "To win, you need to make a line of pieces on the board. I will give it to you.",
        "Coloring the pieces",
        "I can add something clever, for example...... Okay, that'll do",
        "Teaching figures to walk",
        "What if the stones are soft, just harden when touched?",
        "I buy crutches",
        "Feeding the developer",
        "Jumping under textures is prohibited, error correction is at your expense",
        "Be aware that there is a ban word in the download text, beware",
        "To aim, press RMB... oops, wrong place"
    };
    /*readonly string[] loadingPhrasesMetaLine_MultiSelf =
    {
        "But in the previous version line it was better... \nOr not? What has changed?",
        "Do you know about an important rule? If yes, then write.",
        "Has something changed?",
        "I exist? But how?", "Again.", "Again?"
    };*/

    public void SetLoadText()
    {
        int index;
        index = Random.Range(0, loadingPhrasesCurLine.Length);
        textGUI.text = loadingPhrasesCurLine[index];
    }

    public void SetTxtOnNetServicesNotInit()
    {
        textGUI.text = "Network <color=#ÑÑ0000>services not loaded!</color> Launching single player game";
    }
}
