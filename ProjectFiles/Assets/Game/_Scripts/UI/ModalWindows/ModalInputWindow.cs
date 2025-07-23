using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ModalInputWindow : MonoBehaviour
{
    public static ModalInputWindow Instance;

    [SerializeField] GameObject inputWindow;
    [SerializeField] Button okBttn;
    [SerializeField] Button cancelBttn;
    [SerializeField] TextMeshProUGUI titleTxt;
    [SerializeField] TextMeshProUGUI placeholderTxt;
    [SerializeField] TMP_InputField inputField;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(string title, string placeholder, Action onCancel, Action<string> onOk,
        int characterLimit = 20, string validCharacters = "EVERYTHING")
    {
        Default();

        inputWindow.SetActive(true);
        titleTxt.text = title;

        inputField.characterLimit = characterLimit;
        
        if (validCharacters == "EVERYTHING")
            inputField.onValidateInput = null;
        else
            inputField.onValidateInput = (string text, int charIndex, char addedChar) =>
            {
                return ValidateChar(validCharacters, addedChar);
            };

        placeholderTxt.text = placeholder;
        inputField.Select();

        okBttn.onClick.AddListener(() =>
        {
            if(inputField.text.Length != 0)
                onOk(inputField.text);
            Hide();
        });

        cancelBttn.onClick.AddListener(() =>
        {
            onCancel();
            Hide();
        });
    }

    public void Hide()
    {
        inputWindow.SetActive(false);
    }

    void Default()
    {
        titleTxt.text = "";
        inputField.text = "";
        placeholderTxt.text = "input...";

        cancelBttn.onClick.RemoveAllListeners();
        okBttn.onClick.RemoveAllListeners();
    }

    char ValidateChar(string validCharacters, char addedChar)
    {
        if (validCharacters.IndexOf(addedChar) != -1)
        {
            // Valid
            return addedChar;
        }
        else
        {
            // Invalid
            return '\0';
        }
    }
}