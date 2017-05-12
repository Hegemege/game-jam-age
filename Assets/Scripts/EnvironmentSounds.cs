using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSounds : MonoBehaviour {

    public AudioClip[] AllSeasonSounds;
    public AudioClip[] TreeGrowingSounds;
    /*
    public AudioClip[] SpringSounds;
    public AudioClip[] SummerSounds;
    public AudioClip[] AutumnSounds;
    public AudioClip[] WinterSounds;
    */

    public float MinInterval;
    public float MaxInterval;
    public float MinVolume;
    public float MaxVolume;

    private AudioSource source;
    private float timeToNextSound;
    private ClimateController climate;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        climate = GameObject.Find("ClimateController").GetComponent<ClimateController>();
    }

    // Use this for initialization
    void Start () {
        RandomizeInterval();
	}
	
	// Update is called once per frame
	void Update () {
        timeToNextSound -= Time.deltaTime;
        if (timeToNextSound < 0)
        {
            RandomizeInterval();
            var clips = GetSeasonClips(climate.GetSeason());
            var volume = Random.Range(MinVolume, MaxVolume);
            PlayRandomClip(clips, 0, volume);
        }

	}

    private AudioClip[] GetSeasonClips(Season season)
    {
        return AllSeasonSounds;
    }

    /*
    private AudioClip[] GetSeasonClips(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return SpringSounds;
            case Season.Summer:
                return SummerSounds;
            case Season.Autumn:
                return AutumnSounds;
            case Season.Winter:
                return WinterSounds;
            default:
                Debug.LogError("Unknown season!" + season);
                return SpringSounds;
        }
    }
    */



    private void PlayRandomClip(AudioClip[] clips, float delay, float volume)
    {
        var len = clips.GetLength(0);
        if (len > 0)
        {
            StartCoroutine(Wait(delay));
            var clip = clips[Random.Range(0, len - 1)];
            source.PlayOneShot(clip, volume);
        }
        
    } 

    private void RandomizeInterval()
    {
        timeToNextSound = Random.Range(MinInterval, MaxInterval);
    }

    public void PlayRandomTreeClip(float delay, float volume)
    {
        PlayRandomClip(TreeGrowingSounds, delay, volume);
    }
    
    IEnumerator Wait(float delay)
    {
        yield return new WaitForSeconds(delay);
    }
}
