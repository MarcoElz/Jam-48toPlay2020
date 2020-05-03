using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip bossClip;
    [SerializeField] AudioClip gameClip;

    public static MusicController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void PlayClip(float volume, float duration)
    {
        StartCoroutine(StartClipRoutine(volume, duration));
    }

    public void PlayBossMusic(float volume)
    {
        audioSource.Stop();
        audioSource.clip = bossClip;
        audioSource.Play();
        audioSource.volume = volume;
    }

    public void PlayNormalMusic(float volume)
    {
        if(audioSource.isPlaying)
        {
            return;
        }
        audioSource.Stop();
        audioSource.clip = gameClip;
        audioSource.Play();
        audioSource.volume = volume;
    }

    public void StopClip(float duration)
    {
        StartCoroutine(StopClipRoutine(duration));
    }

    IEnumerator StartClipRoutine(float volume, float duration)
    {
        float timer = 0f;
        float multiplier = 1 / duration;

        float toIncrement = volume - audioSource.volume;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        while (timer < 1.0f)
        {
            timer += Time.deltaTime * multiplier;
            audioSource.volume += toIncrement * (Time.deltaTime * multiplier);
            yield return null;
        }
    }

    IEnumerator StopClipRoutine(float duration)
    {
        float timer = 0f;
        float multiplier = 1 / duration;

        float toIncrement = audioSource.volume;

        while (timer < 1.0f)
        {
            timer += Time.deltaTime * multiplier;
            audioSource.volume -= toIncrement * (Time.deltaTime * multiplier);
            yield return null;
        }

        audioSource.Stop();
    }

}
