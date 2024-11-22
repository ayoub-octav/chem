
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
    
public class AudioPlayer : MonoBehaviour, IAudioPlayer
{
    [Header("Audio Settings (only one is needed, audioFileID has priority)")]
    [SerializeField] private int audioFileID;  
    [SerializeField] private string audioFileName;  
    
    private AudioSource audioSource;
    private AudioManager audioManager;
    
    [HideInInspector] public Coroutine fadeInCoroutine;
    [HideInInspector] public Coroutine fadeOutCoroutine;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent onAwake;
    [SerializeField] private UnityEvent onEnable;
    [SerializeField] private UnityEvent onStart;
    [SerializeField] private UnityEvent onDisable;
    [SerializeField] private UnityEvent onDestroy;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioManager = AudioManager.Instance;
        
        onAwake?.Invoke();
    }

    private void OnEnable()
    {
        onEnable?.Invoke();
    }

    private void Start()
    {
        onStart?.Invoke();
    }

    /// <summary>
    /// Function to play Audio with the provided file name or FileID
    /// </summary>
    /// <param name="duration">duration of the fade in</param>
    public void PlayAudio(float duration = 0)
    {
        if (audioFileName == null && audioFileID == 0)
        {
            Debug.LogError($"[Audio] AudiofileName and AudioFileId are required.", this);
        }
        else if (audioFileID > 0)
        {
            audioManager.Play(audioFileID, audioSource, duration);
        }
        else
        {
            audioManager.Play(audioFileName, audioSource, duration);
        }
    }

    /// <summary>
    /// Function to play Audio with the provided file name for animations
    /// </summary>
    /// <param name="name">name of the clip in audio manager</param>
    public void Play(string name)
    {
        audioManager.Play(name, audioSource);
    }

    private int isReverse;

    /// <summary>
    /// isReverse isn't bool because animation events doesn't support bool :)
    /// </summary>
    /// <param name="isReverse"></param>
    public void SetIsReverse(int isReverse)
    {
        this.isReverse = isReverse;
    }
    
    /// <summary>
    /// This function to specifically play the molecule transformation
    /// </summary>
    public void PlayTransformationAnimation()
    {
        if (isReverse == 1)
        {
            audioManager.Play("transformation_phase2_reverse", audioSource);
        }
        else if(isReverse == 0)
        {
            audioManager.Play("transformation_phase2", audioSource);
        }
        isReverse = -1;
    }
    
    
    
    /// <summary>
    /// Function to Stop Audio
    /// </summary>
    /// <param name="duration">duration of the fade out</param>
    public void StopAudio(float duration = 0)
    {
        audioManager.Stop(audioSource, duration);
    }
    
    
    public Coroutine GetFadeInCoroutine()
    {
        return fadeInCoroutine;
    }

    public Coroutine GetFadeOutCoroutine()
    {
        return fadeOutCoroutine;
    }

    private void OnDisable()
    {
        onDisable?.Invoke();
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke();
    }
}
