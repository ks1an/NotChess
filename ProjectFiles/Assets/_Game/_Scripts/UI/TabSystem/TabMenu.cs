using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TabMenu : MonoBehaviour
{
    [Header("PagesView")]
    [SerializeField] GameSettingsView_GamePage settingsPage;
    [SerializeField] GameSettingsViewModel settingsVM;
    [Space(5)]
    [SerializeField] GameObject deckPage;
    [SerializeField] Button deckBttnTAB, settingBttnTab;

    GameObject enabledPage;
    Button activePageBttnTab;
    Color originalColorActivePageBttnTab;

    public void DisableAllTab()
    {
        settingsPage.gameObject.SetActive(false);
        settingsVM = null;

        deckPage.SetActive(false);

        gameObject.SetActive(false);
        enabledPage = null;

        activePageBttnTab.image.color = originalColorActivePageBttnTab;
        activePageBttnTab = null;
    }

    public void ActiveSettingsPage()
    {
        settingsVM = new(GameController.Instance.gameSettings);
        settingsPage.Init(settingsVM);
        ReplacePageToNew(settingsPage.gameObject, settingBttnTab);
    }

    public void ActiveDeckPage()
    {
        ReplacePageToNew(deckPage, deckBttnTAB);
    }

    void ReplacePageToNew(GameObject newPage, Button activeTabBttn)
    {
        if (newPage == enabledPage) return;
        if (enabledPage != null)
        {
            activePageBttnTab.image.DOColor(originalColorActivePageBttnTab, activePageBttnTab.colors.fadeDuration);
            enabledPage.SetActive(false);
        }

        enabledPage = newPage;
        enabledPage.SetActive(true);

        activePageBttnTab = activeTabBttn;
        originalColorActivePageBttnTab = activeTabBttn.image.color;
        Color resultColor = originalColorActivePageBttnTab * (Color.white + activeTabBttn.colors.highlightedColor/1.5f);
        activePageBttnTab.image.DOColor(resultColor, activePageBttnTab.colors.fadeDuration);
    }
}
