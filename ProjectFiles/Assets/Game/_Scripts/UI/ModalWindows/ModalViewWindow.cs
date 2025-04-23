using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ModalViewWindow : MonoBehaviour
{
    [SerializeField] Transform _box;

    [Header("Header")]
    [SerializeField] Transform _headerArea;
    [SerializeField] TextMeshProUGUI _title;

    [Header("Content")]
    [SerializeField] Transform _contentArea;

    [Header("HorizontalContainer")]
    [SerializeField] TextMeshProUGUI _horizontalContainerText;
    [SerializeField] Transform _horizontalLayoutArea, _iconContainer;
    [SerializeField] Image _iconImage;

    [Header("Footer")]
    [SerializeField] Transform _footerArea;
    [SerializeField] Button _confirmBttn, _declineBttn, _altBttn;
    [SerializeField] TextMeshProUGUI _confirmTxt, _declineTxt, _altTxt;

    Action onAlternateAction, onDeclineAction, onConfirmAction, doItAnyway;

    public void ShowHorizontal(string title, string message, string confirmTxt = null, Action greenAction = null, string declineTxt = null,
        Action redAction = null, string altTxt = null, Action altAction = null, Sprite icon = null, Action doItAnyway = null)
    {
        _horizontalLayoutArea.gameObject.SetActive(true);

        #region Header

        _headerArea.gameObject.SetActive(!string.IsNullOrEmpty(title));
        _title.text = title;

        #endregion

        #region Content
        if (icon == null && message == null)
            _contentArea.gameObject.SetActive(false);
        else
        {
            _contentArea.gameObject.SetActive(true);

            if (icon == null)
                _iconContainer.gameObject.SetActive(false);
            else
            {
                _iconContainer.gameObject.SetActive(true);
                _iconImage.sprite = icon;
            }

            if (message == null)
                _horizontalContainerText.gameObject.SetActive(false);
            else
            {
                _horizontalContainerText.gameObject.SetActive(true);
                _horizontalContainerText.text = message;
            }
        }

        #endregion

        #region Footer

        this.doItAnyway = doItAnyway;
        if (greenAction != null)
        {
            _confirmBttn.gameObject.SetActive(true);
            onConfirmAction = greenAction;
            _confirmTxt.text = confirmTxt;
        }
        else
            _confirmBttn.gameObject.SetActive(false);

        if (redAction != null)
        {
            _declineBttn.gameObject.SetActive(true);
            _declineTxt.text = declineTxt;
            onDeclineAction = redAction;
        }
        else
            _declineBttn.gameObject.SetActive(false);

        if (altAction != null)
        {
            _altBttn.gameObject.SetActive(true);
            _altTxt.text = altTxt;
            onAlternateAction = altAction;
        }
        else
            _altBttn.gameObject.SetActive(false);

        #endregion
    }

    #region ActionsInvoke
    public void Confirm()
    {
        onConfirmAction?.Invoke();
        DoItAnyway();
        CloseModalWindow();
    }
    public void Alternate()
    {
        onAlternateAction?.Invoke();
        DoItAnyway();
        CloseModalWindow();
    }
    public void Decline()
    {
        onDeclineAction?.Invoke();
        DoItAnyway();
        CloseModalWindow();
    }
    void DoItAnyway()
    {
        doItAnyway?.Invoke();
        CloseModalWindow();
    }
    #endregion

    public void CloseModalWindow() => gameObject.SetActive(false);
}
