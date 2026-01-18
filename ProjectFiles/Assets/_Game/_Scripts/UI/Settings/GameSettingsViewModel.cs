using UnityEngine;

public class GameSettingsViewModel
{
    readonly GameSettingsModel _model;

    public ReactiveProperty<bool> DontTranslateNameOfCard = new();

    public ReactiveProperty<int> FullscreenMode = new();
    public ReactiveProperty<int> AntiAlaising = new();
    public ReactiveProperty<int> VSync = new();

    public GameSettingsViewModel(GameSettingsModel model)
    {
        _model = model;

        _model.DontTranslateNameOfCard.OnChanged += OnModelDontTranslateNameOfCardChanged;
        DontTranslateNameOfCard.Value = _model.DontTranslateNameOfCard.Value;

        //GraphicSync
        _model.FullscreenMode.OnChanged += OnModelFullscreenModeChanged;
        FullscreenMode.Value = _model.FullscreenMode.Value;

        _model.AntiAliasing.OnChanged += OnModelAntiAlaisingChanged;
        AntiAlaising.Value = _model.AntiAliasing.Value;

        _model.VSync.OnChanged += OnModelVSyncChanged;
        VSync.Value = _model.VSync.Value;
    }

    void OnModelDontTranslateNameOfCardChanged(bool b) => DontTranslateNameOfCard.Value = b;
    public void OnToggle_DontTranslateNameOfCard_Clicked(bool b) => DontTranslateNameOfCard.Value = b;

    //GRAPHIC
    void OnModelFullscreenModeChanged(int val) => FullscreenMode.Value = val;
    public void OnDropdownChanged_FullscreenMode(int val) => FullscreenMode.Value = val;

    void OnModelAntiAlaisingChanged(int val) => AntiAlaising.Value = val;
    public void OnDropdownChanged_AntiAliasing(int val) => AntiAlaising.Value = val;

    void OnModelVSyncChanged(int val) => VSync.Value = val;
    public void OnDropdownChanged_VSync(int val) => VSync.Value = val;

    public void OnResetToModel()
    {
        DontTranslateNameOfCard.Value = _model.DontTranslateNameOfCard.Value;

        //GRAPHIC
        FullscreenMode.Value = _model.FullscreenMode.Value;
        AntiAlaising.Value = _model.AntiAliasing.Value;
        VSync.Value = _model.VSync.Value;
    }

    public void OnApplyClicked()
    {
        //GAME LOGIC
        _model.DontTranslateNameOfCard.Value = DontTranslateNameOfCard.Value;

        //Graphic
        _model.FullscreenMode.Value = FullscreenMode.Value;
        _model.AntiAliasing.Value = AntiAlaising.Value;
        _model.VSync.Value = VSync.Value;

        _model.ApplySettings();
    }

    public void Dispose()
    {
        _model.DontTranslateNameOfCard.OnChanged -= OnModelDontTranslateNameOfCardChanged;
        _model.FullscreenMode.OnChanged -= OnModelFullscreenModeChanged;
        _model.AntiAliasing.OnChanged -= OnModelAntiAlaisingChanged;
        _model.VSync.OnChanged -= OnModelVSyncChanged;
    }
}
