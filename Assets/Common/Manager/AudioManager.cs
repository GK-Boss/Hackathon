using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static public AudioManager instance;

    //[Header("BGM")]
    //[SerializeField] private AudioSource bgm;
    //[Header("BGS")]
    //[SerializeField] private AudioSource buildBGS;
    //private float buildBGS_Volume;
    //private float buildBGS_PastVolume;
    [Header("BGM")]
    [SerializeField] private AudioSource bgm;

    [Header("SE")]
    [SerializeField] private List<AudioSource> se;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        //if (buildBGS_PastVolume == buildBGS_Volume)
        //{
        //    buildBGS_Volume -= buildBGS.volume * 2 * Time.deltaTime;
        //    if (buildBGS_Volume < 0)
        //    {
        //        buildBGS_Volume = 0;
        //    }

        //    buildBGS.volume = buildBGS_Volume;
        //}

        //buildBGS_PastVolume = buildBGS_Volume;
    }

    public void PlayBGM()
    {

    }


    public void ChangeBGM(AudioData data)
    {
        StartCoroutine(BGM_Coroutin(data));
    }

    private IEnumerator BGM_Coroutin(AudioData data)
    {
        float progress = 0;

        while (progress < 1)
        {
            bgm.volume = 1.0f - progress;
            progress += Time.deltaTime;
            yield return null;
        }

        bgm.volume = 0;
        progress = 0;
        yield return null;

            bgm.clip = data.audioClip;
            bgm.Play();

            while (progress < 1)
            {
                bgm.volume = progress;
                progress += Time.deltaTime;
                yield return null;
            }

        bgm.volume = 1;
    }

    //public void PlayBuildBGS()
    //{
    //    buildBGS_Volume += (0.5f - buildBGS.volume) * Time.deltaTime;

    //    buildBGS.volume = buildBGS_Volume;
    //}
    public void PlaySE(AudioData data)
    {
        PlaySE(new List<AudioData> { data });
    }

    public void PlaySE(List<AudioData> data)
    {
        List<AudioSource> usedSources = new List<AudioSource>();

        for (int i = 0; i < data.Count; i++)
        {
            AudioSource freeSource = se.Find(s => !s.isPlaying && !usedSources.Contains(s));

            if (freeSource == null)
            {
                freeSource = se.Find(s => !usedSources.Contains(s));

                if (freeSource == null) freeSource = se[0];

                Debug.Log("SEâﬂèË");
            }

            usedSources.Add(freeSource);

            freeSource.clip = data[i].audioClip;
            freeSource.volume = data[i].off ? 0 : data[i].volume;
            freeSource.pitch = data[i].pitch;
            freeSource.Play();
        }
    }
}
