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

    public void PlaySound(AudioClip[] clips, float volume = 1, float minPinch = 1, float maxPinch = 1)
    {
        audioSource.pitch = mathOp.GetSafeRandom(minPinch, maxPinch);
        audioSource.PlayOneShot(clips[(int)mathOp.GetSafeRandom(0, clips.Length, true)], volume);
    }
}
