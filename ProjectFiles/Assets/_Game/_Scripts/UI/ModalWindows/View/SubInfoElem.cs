using TMPro;
using UnityEngine;

public sealed class SubInfoElem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title, content;
    public void Init(string title, string content)
    {
        this.title.text = title;
        this.content.text = content;
    }
}
