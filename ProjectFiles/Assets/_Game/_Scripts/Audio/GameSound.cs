using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class GameSound : MonoBehaviour
{
    public static GameSound Instance;

    AudioSource audioSource;
    readonly MathOperations mathOp = MathOperations.GetInstance();
    float baseVolume;

    void Awake()
    {
        if (Instance == null)
        {
            audioSource = GetComponent<AudioSource>();
            Instance = this;
        }
        else Destroy(Instance);
        GameController.Instance.gameSettings.EffectVolume.OnChanged += UpdateVolume;
        baseVolume = GameController.Instance.gameSettings.EffectVolume.Value;
    }

    public void PlayRandomSound(AudioClip[] clips, float volumeMultiplicator = 1, float minPinch = 1, float maxPinch = 1)
    {
        PlaySound(clips[(int)mathOp.GetSafeRandom(0, clips.Length, true)], volumeMultiplicator * baseVolume, minPinch, maxPinch);
    }

    public void PlaySound(AudioClip clip, float volume, float minPinch = 1, float maxPinch = 1)
    {
        audioSource.pitch = mathOp.GetSafeRandom(minPinch, maxPinch);
        audioSource.PlayOneShot(clip, volume);
    }

    public void UpdateVolume(float newVolume) => baseVolume = newVolume;

    private void OnDestroy()
    {
        GameController.Instance.gameSettings.EffectVolume.OnChanged -= UpdateVolume;
    }
}
