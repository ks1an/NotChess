using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public static ReactiveProperty<Music> CurrentMusic = new();

    [SerializeField] List<Music> menuMusic;
    [SerializeField] List<Music> boardMusic;
    [SerializeField] float transitionTime = 0.5f;

    AudioSource audioSource;
    int currentMusicIndex;
    float baseVolume;
    readonly MathOperations mathOp = MathOperations.GetInstance();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SceneLoader.Instance.OnMenuSceneLoaded += PlaySceneMusic;
        SceneLoader.Instance.OnBoardSceneLoaded += PlaySceneBoardMusic;
        GameController.Instance.gameSettings.MusicVolume.OnChanged += SetVolumeAudioSource;
        baseVolume = GameController.Instance.gameSettings.MusicVolume.Value;
    }

    public void PlaySceneMusic() { FindAndPlayMusic(menuMusic); }
    public void PlaySceneBoardMusic() {FindAndPlayMusic(boardMusic); }

    void FindAndPlayMusic(List<Music> workspaceMusicList)
    {

        if (workspaceMusicList == null || workspaceMusicList.Count == 0)
        {
            Debug.LogWarning("Music list is empty or null!");
            return;
        }

        int newMusicIndex;

        if (workspaceMusicList.Count == 1)
        {
            newMusicIndex = 0;
        }
        else
        {
            do
            {
                newMusicIndex = (int)mathOp.GetSafeRandom(0, workspaceMusicList.Count, true);
            } while (currentMusicIndex == newMusicIndex);
        }
        currentMusicIndex = newMusicIndex;

        StopAllCoroutines();
        StartCoroutine(PlayAudio(workspaceMusicList[currentMusicIndex], workspaceMusicList));
    }

    IEnumerator PlayAudio(Music music, List<Music> musicList)
    {
        audioSource.DOFade(0, transitionTime / 2);
        music.clip.LoadAudioData();
        yield return new WaitForSeconds(transitionTime / 2);
        audioSource.Stop();

        audioSource.pitch = mathOp.GetSafeRandom(music.minPinch, music.maxPinch);
        float volume = music.volumeMultiplicator * baseVolume;
        audioSource.volume = volume;
        audioSource.clip = music.clip;
        audioSource.Play();
        audioSource.DOFade(volume, transitionTime / 2);
        CurrentMusic.Value = music;

        yield return new WaitForSeconds(music.clip.length - transitionTime/2);
        music.clip.UnloadAudioData();
        FindAndPlayMusic(musicList);
    }

    void SetVolumeAudioSource(float volume) => audioSource.volume = volume;

    private void OnDisable()
    {
        SceneLoader.Instance.OnSomeSceneStartLoading -= PlaySceneMusic;
        SceneLoader.Instance.OnBoardSceneLoaded -= PlaySceneBoardMusic;
        GameController.Instance.gameSettings.MusicVolume.OnChanged -= SetVolumeAudioSource;
    }
}

[Serializable]
public class Music
{
    public string name;
    public string author;
    public string copyrightLink;

    [Header("Audio")]
    public AudioClip clip;
    public float volumeMultiplicator = 1;
    public int minPinch = 1;
    public int maxPinch = 1;
}
