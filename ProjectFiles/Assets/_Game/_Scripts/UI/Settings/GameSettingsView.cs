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
    }

    protected abstract void Display_DontTranslateNameOfCard(bool b);

    protected abstract void Display_FullscreenMode(int val);
    protected abstract void Display_Antialaizing(int val);
    protected abstract void Display_VSync(int val);

    protected virtual void Dispose()
    {
        viewModel.DontTranslateNameOfCard.OnChanged -= Display_DontTranslateNameOfCard;
        viewModel.FullscreenMode.OnChanged -= Display_FullscreenMode;
        viewModel.AntiAlaising.OnChanged -= Display_Antialaizing;
        viewModel.VSync.OnChanged -= Display_VSync;

        viewModel.Dispose();
    }

    private void OnDisable()
    {
        Dispose();
    }
}
