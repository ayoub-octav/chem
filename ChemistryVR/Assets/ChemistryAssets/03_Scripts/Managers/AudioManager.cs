using System;
using System.Collections;
using System.Collections.Generic;
using Meta.Voice.Audio;
using Meta.WitAi.TTS.Utilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private List<AudioClip> audioClips;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Play(string clipName,AudioSource source,float fadeInDuration = 0)
    {
        var clip = audioClips.Find((clip => clipName == clip.name));
        if (clip == null)
        {
            Debug.LogError($"Audio Name not found {clipName}");
        }

        if (source == null)
        {
            Debug.LogError($"Audio source is null");
        }
        Play(clip, source, fadeInDuration);
    }

    public void Play(int clipID, AudioSource source,float fadeInDuration = 0)
    {
        if(clipID == 0) return;        
        var clip = audioClips.Find((clip => clipID == clip.id));
        Play(clip, source, fadeInDuration);
    }

    public void SetVolume(float volume, AudioSource source)
    {
        source.volume = volume;
    }
    
    public void Stop(AudioSource source,float fadeOutDuration = 0)
    {
        if (fadeOutDuration != 0)
        {
            FadeOutVolume(source, fadeOutDuration);
        }
        else
        {
            source.Stop();
        }
    }


    private IEnumerator FadeInCoroutine(AudioSource audioSource, float duration, float targetVolume)
    {
        audioSource.volume = 0f;
        audioSource.Play();

        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume , currentTime / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
    
    private IEnumerator FadeOutCoroutine(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;  // Reset volume for next time the audio is played
    }
    
    private void Play(AudioClip clip, AudioSource source,float fadeInDuration = 0)
    {
        SetClipParameters(clip,source);
        if (clip.isOneShot)
        {
            source.PlayOneShot(source.clip);
        }
        else
        {
            source.Play();
        }

        FadeInVolume(clip, source, fadeInDuration);
    }

    private void FadeInVolume(AudioClip clip, AudioSource source, float fadeInDuration)
    {
        //Fadein should always play after Play
        if (fadeInDuration == 0) return;
        Coroutine fadeInCoroutine = source.GetComponent<IAudioPlayer>().GetFadeInCoroutine();
        Coroutine fadeOutCoroutine = source.GetComponent<IAudioPlayer>().GetFadeOutCoroutine();
        
        //FadeIn coroutine should only be interupted by a FadeOut couroutine
        if (fadeInCoroutine == null)
            fadeInCoroutine = StartCoroutine(FadeInCoroutine(source, fadeInDuration, clip.volume));
        
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }
    }

    private void FadeOutVolume(AudioSource source, float fadeOutDuration)
    {
        Coroutine fadeInCoroutine = source.GetComponent<IAudioPlayer>().GetFadeInCoroutine();
        Coroutine fadeOutCoroutine = source.GetComponent<IAudioPlayer>().GetFadeOutCoroutine();
        
        //FadeIn coroutine should only be interupted by a FadeOut couroutine
        if (fadeOutCoroutine == null)
            fadeOutCoroutine = StartCoroutine(FadeOutCoroutine(source, fadeOutDuration));
        if (fadeInCoroutine != null)
        {
            StopCoroutine(fadeInCoroutine);
        }
    }

    private void SetClipParameters(AudioClip clip, AudioSource source)
    {
        source.clip = clip.audioClip[Random.Range(0, clip.audioClip.Count)];
        source.volume = clip.volume;
        source.spatialBlend = clip.spatialBlend;
        source.pitch = clip.pitch;
        source.loop = clip.loop;
    }

}
