using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsView_GamePage : GameSettingsView
{
    [SerializeField] Toggle dontTranslateNameOfCardToogle;
    [SerializeField]
    TMP_Dropdown fullscreenModeDropdown,
        antializingDropdown, vsyncDropdown;
    [SerializeField] TextMeshProUGUI musicNameTxt;
    [SerializeField] Button musicBttn;
    [SerializeField] Button applyBttn, closeBttn;
    [SerializeField] Slider musicVolumeSlider, effectVolumeSlider;
    [SerializeField] TextMeshProUGUI musicVolumeTxt, effectVolumeTxt;

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

        //Audio
        musicVolumeSlider.onValueChanged.AddListener(viewModel.OnView_MusicVolume_Changed);
        Display_MusicVolume(viewModel.MusicVolume.Value);
        effectVolumeSlider.onValueChanged.AddListener(viewModel.OnView_EffectVolume_Changed);
        Display_EffectVolume(viewModel.EffectVolume.Value);

        //OTHER
        musicBttn.onClick.AddListener(OnClickedMusicBttn);
        BackgroundMusic.CurrentMusic.OnChanged += Display_Music;
        Display_Music(BackgroundMusic.CurrentMusic.Value);

        applyBttn.onClick.AddListener(viewModel.OnApplyClicked);
        closeBttn.onClick.AddListener(viewModel.OnResetToModel);
    }


    protected override void Display_DontTranslateNameOfCard(bool b) => dontTranslateNameOfCardToogle.isOn = b;

    protected override void Display_Antialaizing(int val) => antializingDropdown.value = val;
    protected override void Display_FullscreenMode(int val) => fullscreenModeDropdown.value = val;
    protected override void Display_VSync(int val) => vsyncDropdown.value = val;

    #region Audio
    protected override void Display_MusicVolume(float val)
    {
        musicVolumeSlider.value = val;
        musicVolumeTxt.text = ((int)(val * 100)).ToString();
    }
    protected override void Display_EffectVolume(float val)
    {
        effectVolumeSlider.value = val;
        effectVolumeTxt.text = ((int)(val * 100)).ToString();
    }
    protected void Display_Music(Music music) => musicNameTxt.text = music.name + " by " + music.author;
    #endregion


    protected void OnClickedMusicBttn()
    {
        TextEditor te = new()
        {
            text = musicNameTxt.text
        };
        te.SelectAll();
        te.Copy();

        Application.OpenURL(BackgroundMusic.CurrentMusic.Value.copyrightLink);
    }


    protected override void Dispose()
    {
        base.Dispose();

        dontTranslateNameOfCardToogle.onValueChanged.RemoveListener(viewModel.OnToggle_DontTranslateNameOfCard_Clicked);

        //GRAPHIC
        fullscreenModeDropdown.onValueChanged.RemoveListener(viewModel.OnDropdownChanged_FullscreenMode);
        antializingDropdown.onValueChanged.RemoveListener(viewModel.OnDropdownChanged_AntiAliasing);
        vsyncDropdown.onValueChanged.RemoveListener(viewModel.OnDropdownChanged_VSync);

        musicBttn.onClick.RemoveListener(OnClickedMusicBttn);
        BackgroundMusic.CurrentMusic.OnChanged -= Display_Music;

        applyBttn.onClick.RemoveListener(viewModel.OnApplyClicked);
        closeBttn.onClick.RemoveListener(viewModel.OnResetToModel);
    }
}
