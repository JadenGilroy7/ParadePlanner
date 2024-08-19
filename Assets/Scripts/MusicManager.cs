using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public AudioSource audioSource;

    public AudioClip[] musicClips;
    private int currentClipIndex = 0;

    private void Awake()
    {
        // Ensure only one instance of MusicPlayer exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayCurrentClip();
    }

    private void Update()
    {
        // Loop song
        if (!audioSource.isPlaying)
        {
            PlayCurrentClip();
        }
    }

    public void PlayCurrentClip()
    {
        if (musicClips.Length > 0)
        {
            audioSource.clip = musicClips[currentClipIndex];
            audioSource.Play();
        }
    }

    public void ChangeClip(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < musicClips.Length)
        {
            currentClipIndex = clipIndex;
            PlayCurrentClip();
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
}

