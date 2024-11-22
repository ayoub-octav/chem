using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioClip
{
    public int id;
    public String name;
    public bool isOneShot;
    public List<UnityEngine.AudioClip> audioClip;
    [Range(0,1)] public float volume = 1;
    [Range(0, 2)] public float pitch = 1;
    [Range(0,1)] public float spatialBlend = 1;
    public bool loop;
}
