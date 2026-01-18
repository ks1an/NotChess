using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsView_GamePage : GameSettingsView
{
    [SerializeField] Toggle dontTranslateNameOfCardToogle;
    [SerializeField] TMP_Dropdown fullscreenModeDropdown,
        antializingDropdown, vsyncDropdown;
    [SerializeField] Button applyBttn, closeBttn;

    public override void Init(GameSettingsViewModel modelView)
    {
        base.Init(modelView);

        dontTranslateNameOfCardToogle.onValueChanged.AddListener(viewModel.OnToggle_DontTranslateNameOfCard_Clicked);
        dontTranslateNameOfCardToogle.isOn = viewModel.DontTranslateNameOfCard.Value;

        //GRAPHIC
        fullscreenModeDropdown.onValueChanged.AddListener(viewModel.OnDropdownChanged_FullscreenMode);
        fullscreenModeDropdown.value = viewModel.FullscreenMode.Value;

        antializingDropdown.onValueChanged.AddListener(viewModel.OnDropdownChanged_AntiAliasing);
        antializingDropdown.value = viewModel.AntiAlaising.Value;

        vsyncDropdown.onValueChanged.AddListener(viewModel.OnDropdownChanged_VSync);
        vsyncDropdown.value = viewModel.VSync.Value;


        applyBttn.onClick.AddListener(viewModel.OnApplyClicked);
        closeBttn.onClick.AddListener(viewModel.OnResetToModel);
    }


    protected override void Display_DontTranslateNameOfCard(bool b) => dontTranslateNameOfCardToogle.isOn = b;

    protected override void Display_Antialaizing(int val) => antializingDropdown.value = val;
    protected override void Display_FullscreenMode(int val) => fullscreenModeDropdown.value = val;
    protected override void Display_VSync(int val) => vsyncDropdown.value = val;


    protected override void Dispose()
    {
        base.Dispose();

        dontTranslateNameOfCardToogle.onValueChanged.RemoveListener(viewModel.OnToggle_DontTranslateNameOfCard_Clicked);
       
        //GRAPHIC
        fullscreenModeDropdown.onValueChanged.RemoveListener(viewModel.OnDropdownChanged_FullscreenMode);
        antializingDropdown.onValueChanged.RemoveListener(viewModel.OnDropdownChanged_AntiAliasing);
        vsyncDropdown.onValueChanged.RemoveListener(viewModel.OnDropdownChanged_VSync);

        applyBttn.onClick.RemoveListener(viewModel.OnApplyClicked);
        closeBttn.onClick.RemoveListener(viewModel.OnResetToModel);
    }
}
