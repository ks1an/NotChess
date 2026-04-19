using UnityEngine;

public abstract class GameSettingsView : MonoBehaviour
{
    protected GameSettingsViewModel viewModel;

    public virtual void Init(GameSettingsViewModel modelView)
    {
        viewModel = modelView;

        viewModel.DontTranslateNameOfCard.OnChanged += Display_DontTranslateNameOfCard;

        //GRAPHIC
        viewModel.FullscreenMode.OnChanged += Display_FullscreenMode;
        viewModel.AntiAlaising.OnChanged += Display_Antialaizing;
        viewModel.VSync.OnChanged += Display_VSync;
        //Audio
        viewModel.MusicVolume.OnChanged += Display_MusicVolume;
        viewModel.EffectVolume.OnChanged += Display_EffectVolume;   
    }

    protected abstract void Display_DontTranslateNameOfCard(bool b);

    //GRAPHIC
    protected abstract void Display_FullscreenMode(int val);
    protected abstract void Display_Antialaizing(int val);
    protected abstract void Display_VSync(int val);
    //AUDIO
    protected abstract void Display_MusicVolume(float val);
    protected abstract void Display_EffectVolume(float val);

    protected virtual void Dispose()
    {
        viewModel.DontTranslateNameOfCard.OnChanged -= Display_DontTranslateNameOfCard;

        viewModel.FullscreenMode.OnChanged -= Display_FullscreenMode;
        viewModel.AntiAlaising.OnChanged -= Display_Antialaizing;
        viewModel.VSync.OnChanged -= Display_VSync;

        viewModel.MusicVolume.OnChanged -= Display_MusicVolume;
        viewModel.EffectVolume.OnChanged -= Display_EffectVolume;

        viewModel.Dispose();
    }

    private void OnDisable()
    {
        Dispose();
    }
}
