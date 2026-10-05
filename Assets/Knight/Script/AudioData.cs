using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/AudioData")]
public class AudioData : ScriptableObject
{
    public AudioClip audioClip;
    public bool off;
    public float volume = 1;
    public float pitch = 1;

}
