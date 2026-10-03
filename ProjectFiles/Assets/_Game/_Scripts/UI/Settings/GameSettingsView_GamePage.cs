using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsView_GamePage : GameSettingsView
{
    [SerializeField] Toggle dontTranslateNameOfCardToogle;
    [SerializeField] TMP_Dropdown fullscreenModeDropdown;
    [SerializeField] SegmentedBttn vsyncBttns;
    [SerializeField] CycleButton antializingCycleBttn;
    [SerializeField] Transform fullscreenContainer, vsyncContainer;
    [SerializeField] TextMeshProUGUI musicNameTxt;
    [SerializeField] Button musicBttn;
    [SerializeField] Button applyBttn, closeBttn;
    [SerializeField] SnappedSlider musicVolumeSlider, effectVolumeSlider;
    [SerializeField] TextMeshProUGUI musicVolumeTxt, effectVolumeTxt;

    bool isMobile;

    public override void Init(GameSettingsViewModel modelView)
    {
        base.Init(modelView);

        isMobile = Application.isMobilePlatform;

        if (isMobile)
        {
            if (fullscreenContainer != null) fullscreenContainer.gameObject.SetActive(false);
            if (vsyncContainer != null) vsyncContainer.gameObject.SetActive(false);
        }
        else
        {
            fullscreenModeDropdown.onValueChanged.AddListener(value =>
            {
                viewModel.OnDropdownChanged_FullscreenMode(value);
                CheckChangedParamsAndSetApplyBttn();
            });
            fullscreenModeDropdown.value = viewModel.FullscreenMode.Value;

            vsyncBttns.OnValueChanged += value =>
            {
                viewModel.OnDropdownChanged_VSync(value);
                CheckChangedParamsAndSetApplyBttn();
            };
            vsyncBttns.DisplayValue(viewModel.VSync.Value);
        }

        // Card
        dontTranslateNameOfCardToogle.onValueChanged.AddListener(value =>
        {
            viewModel.OnToggle_DontTranslateNameOfCard_Clicked(value);
            CheckChangedParamsAndSetApplyBttn();
        });
        dontTranslateNameOfCardToogle.isOn = viewModel.DontTranslateNameOfCard.Value;

        // GRAPHIC
        antializingCycleBttn.OnValueChanged += value =>
        {
            viewModel.OnDropdownChanged_AntiAliasing(value);
            CheckChangedParamsAndSetApplyBttn();
        };
        antializingCycleBttn.DisplayValue(viewModel.AntiAlaising.Value);

        // AUDIO
        musicVolumeSlider.OnValueChanged += value =>
        {
            viewModel.OnView_MusicVolume_Changed(value);
            CheckChangedParamsAndSetApplyBttn();
        };
        Display_MusicVolume(viewModel.MusicVolume.Value);

        effectVolumeSlider.OnValueChanged += value =>
        {
            viewModel.OnView_EffectVolume_Changed(value);
            CheckChangedParamsAndSetApplyBttn();
        };
        Display_EffectVolume(viewModel.EffectVolume.Value);

        // OTHER
        musicBttn.onClick.AddListener(OnClickedMusicBttn);
        BackgroundMusic.CurrentMusic.OnChanged += Display_Music;
        Display_Music(BackgroundMusic.CurrentMusic.Value);

        applyBttn.onClick.AddListener(OnApplyClicked);
        closeBttn.onClick.AddListener(viewModel.OnResetToModel);
        applyBttn.interactable = false;
    }

    void OnApplyClicked()
    {
        viewModel.OnApplyClicked();
        applyBttn.interactable = false;
    }

    void CheckChangedParamsAndSetApplyBttn()
    {
        applyBttn.interactable = false;
        var settings = GameController.Instance.gameSettings;

        // CARD
        if (viewModel.DontTranslateNameOfCard.Value != settings.DontTranslateNameOfCard.Value)
        { applyBttn.interactable = true; return; }

        // GRAPHIC — только для ПК
        if (!isMobile)
        {
            if (viewModel.FullscreenMode.Value != settings.FullscreenMode.Value)
            { applyBttn.interactable = true; return; }
            if (viewModel.VSync.Value != settings.VSync.Value)
            { applyBttn.interactable = true; return; }
        }

        if (viewModel.AntiAlaising.Value != settings.AntiAliasing.Value)
        { applyBttn.interactable = true; return; }

        // AUDIO
        int currentMusic = Mathf.RoundToInt(viewModel.MusicVolume.Value * 100);
        int savedMusic = Mathf.RoundToInt(settings.MusicVolume.Value * 100);
        if (currentMusic != savedMusic) { applyBttn.interactable = true; return; }

        int currentEffect = Mathf.RoundToInt(viewModel.EffectVolume.Value * 100);
        int savedEffect = Mathf.RoundToInt(settings.EffectVolume.Value * 100);
        if (currentEffect != savedEffect) { applyBttn.interactable = true; return; }
    }

    protected override void Display_DontTranslateNameOfCard(bool b)
        => dontTranslateNameOfCardToogle.isOn = b;

    protected override void Display_Antialaizing(int val)
        => antializingCycleBttn.DisplayValue(val);

    protected override void Display_FullscreenMode(int val)
    {
        if (!isMobile && fullscreenModeDropdown != null)
            fullscreenModeDropdown.value = val;
    }

    protected override void Display_VSync(int val)
    {
        if (!isMobile)
            vsyncBttns.DisplayValue(val);
    }

    #region Audio
    protected override void Display_MusicVolume(float val)
    {
        musicVolumeSlider.DisplayValue(val);
        musicVolumeTxt.text = Mathf.RoundToInt(val * 100).ToString();
    }

    protected override void Display_EffectVolume(float val)
    {
        effectVolumeSlider.DisplayValue(val);
        effectVolumeTxt.text = Mathf.RoundToInt(val * 100).ToString();
    }

    protected void Display_Music(Music music)
        => musicNameTxt.text = music.name + " by " + music.author;
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

        dontTranslateNameOfCardToogle.onValueChanged.RemoveAllListeners();
        antializingCycleBttn.ClearListeners();
        musicVolumeSlider.ClearListeners();     
        effectVolumeSlider.ClearListeners();

        if (!isMobile)
        {
            if (fullscreenModeDropdown != null)
                fullscreenModeDropdown.onValueChanged.RemoveAllListeners();
            vsyncBttns.ClearListeners();
        }

        musicBttn.onClick.RemoveListener(OnClickedMusicBttn);
        applyBttn.onClick.RemoveListener(OnApplyClicked);
        closeBttn.onClick.RemoveAllListeners();

        BackgroundMusic.CurrentMusic.OnChanged -= Display_Music;
    }
}