using UnityEngine;

public abstract class VisualBuff_SO : ScriptableObject
{
    [Header("Audio"), Tooltip("If there is no sound, then do not touch anything in this block.")]
    public float volume = 1f;
    public float minPitch = 1f, maxPitch = 1f;
    public AudioClip[] audioClipsOnAdded;
    public AudioClip[] audioClipsOnRemoved;
    public AudioClip[] audioClipsOnTurned;
}
