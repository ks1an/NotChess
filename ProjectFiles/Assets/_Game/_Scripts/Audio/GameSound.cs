using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class GameSound : MonoBehaviour
{
    public static GameSound Instance;

    AudioSource audioSource;
    readonly MathOperations mathOp = MathOperations.GetInstance();

    void Awake()
    {
        if (Instance == null)
        {
            audioSource = GetComponent<AudioSource>();
            Instance = this;
        }
        else Destroy(Instance);
    }

    public void PlayRandomSound(AudioClip[] clips, float volumeMultiplicator = 1, float minPinch = 1, float maxPinch = 1)
    {
        PlaySound(clips[(int)mathOp.GetSafeRandom(0, clips.Length, true)], volumeMultiplicator * GameController.Instance.gameSettings.EffectVolume.Value, minPinch, maxPinch);
    }

    public void PlaySound(AudioClip clip, float volume, float minPinch = 1, float maxPinch = 1)
    {
        audioSource.pitch = mathOp.GetSafeRandom(minPinch, maxPinch);
        audioSource.PlayOneShot(clip, volume);
    }
}
