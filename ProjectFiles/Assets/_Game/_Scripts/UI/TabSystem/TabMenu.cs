using UnityEngine;

public class TabMenu : MonoBehaviour
{
    [Header("PagesView")]
    [SerializeField] GameSettingsView_GamePage settingsPage;
    [SerializeField] GameSettingsViewModel settingsVM;
    [Space(5)]
    [SerializeField] GameObject deckPage;

    GameObject enabledPage;

    public void DisableAllTab()
    {
        settingsPage.gameObject.SetActive(false);
        settingsVM = null;

        deckPage.SetActive(false);

        gameObject.SetActive(false);
        enabledPage = null;
    }

    public void ActiveSettingsPage()
    {
        settingsVM = new(GameController.Instance.gameSettings);
        settingsPage.Init(settingsVM);
        ReplacePageToNew(settingsPage.gameObject);
    }

    public void ActiveDeckPage()
    {
        ReplacePageToNew(deckPage);
    }

    void ReplacePageToNew(GameObject newPage)
    {
        if (enabledPage != null)
            enabledPage.SetActive(false);

        enabledPage = newPage;
        enabledPage.SetActive(true);
    }
}
