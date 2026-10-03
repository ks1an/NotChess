public class GameSettingsViewModel
{
    readonly GameSettingsModel _model;

    public ReactiveProperty<bool> DontTranslateNameOfCard = new();

    public ReactiveProperty<int> FullscreenMode = new();
    public ReactiveProperty<int> AntiAlaising = new();
    public ReactiveProperty<int> VSync = new();

    //Audio
    public ReactiveProperty<float> MusicVolume = new();
    public ReactiveProperty<float> EffectVolume = new();

    public GameSettingsViewModel(GameSettingsModel model)
    {
        _model = model;

        _model.DontTranslateNameOfCard.OnChanged += OnModelDontTranslateNameOfCardChanged;

        //GraphicSync
        _model.FullscreenMode.OnChanged += OnModelFullscreenModeChanged;
        _model.AntiAliasing.OnChanged += OnModelAntiAlaisingChanged;
        _model.VSync.OnChanged += OnModelVSyncChanged;

        //Audio
        _model.MusicVolume.OnChanged += OnModel_MusicVolume_Changed;
        _model.EffectVolume.OnChanged += OnModel_EffectVolume_Changed;

        OnResetToModel();
    }

    void OnModelDontTranslateNameOfCardChanged(bool b) => DontTranslateNameOfCard.Value = b;
    public void OnToggle_DontTranslateNameOfCard_Clicked(bool b) => DontTranslateNameOfCard.Value = b;

    #region GRAPHIC
    void OnModelFullscreenModeChanged(int val) => FullscreenMode.Value = val;
    public void OnDropdownChanged_FullscreenMode(int val) => FullscreenMode.Value = val;

    void OnModelAntiAlaisingChanged(int val) => AntiAlaising.Value = val;
    public void OnDropdownChanged_AntiAliasing(int val) => AntiAlaising.Value = val;

    void OnModelVSyncChanged(int val) => VSync.Value = val;
    public void OnDropdownChanged_VSync(int val) => VSync.Value = val;
    #endregion

    #region Audio
    void OnModel_MusicVolume_Changed(float b) => MusicVolume.Value = b;
    public void OnView_MusicVolume_Changed(float b)
    {
        MusicVolume.Value = b;
        BackgroundMusic.instance.SetVolumeAudioSource(b);
    }

    void OnModel_EffectVolume_Changed(float b) => EffectVolume.Value = b;
    public void OnView_EffectVolume_Changed(float b)
    {
        EffectVolume.Value = b;
        GameSound.Instance.UpdateVolume(b);
    }
    #endregion

    public void OnResetToModel()
    {
        DontTranslateNameOfCard.Value = _model.DontTranslateNameOfCard.Value;

        //GRAPHIC
        FullscreenMode.Value = _model.FullscreenMode.Value;
        AntiAlaising.Value = _model.AntiAliasing.Value;
        VSync.Value = _model.VSync.Value;

        //Audio
        MusicVolume.Value = _model.MusicVolume.Value;
        BackgroundMusic.instance.SetVolumeAudioSource(MusicVolume.Value);
        EffectVolume.Value = _model.EffectVolume.Value;
        GameSound.Instance.UpdateVolume(EffectVolume.Value);
    }

    public void OnApplyClicked()
    {
        //GAME LOGIC
        _model.DontTranslateNameOfCard.Value = DontTranslateNameOfCard.Value;

        //Graphic
        _model.FullscreenMode.Value = FullscreenMode.Value;
        _model.AntiAliasing.Value = AntiAlaising.Value;
        _model.VSync.Value = VSync.Value;

        //Audio
        _model.MusicVolume.Value = MusicVolume.Value;
        _model.EffectVolume.Value = EffectVolume.Value;

        _model.ApplySettings();
    }

    public void Dispose()
    {
        _model.DontTranslateNameOfCard.OnChanged -= OnModelDontTranslateNameOfCardChanged;
        _model.FullscreenMode.OnChanged -= OnModelFullscreenModeChanged;
        _model.AntiAliasing.OnChanged -= OnModelAntiAlaisingChanged;
        _model.VSync.OnChanged -= OnModelVSyncChanged;

        _model.MusicVolume.OnChanged -= OnModel_MusicVolume_Changed;
        _model.EffectVolume.OnChanged -= OnModel_EffectVolume_Changed;
    }
}
