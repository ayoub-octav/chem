using UnityEngine;

public interface IAudioPlayer
    {
        public Coroutine GetFadeInCoroutine();
        public Coroutine GetFadeOutCoroutine();
    }
