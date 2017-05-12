using UnityEngine;
using System.Collections;

public class WeatherController : MonoBehaviour
{
    private AudioSource audioSource;
    public ParticleSystem Rain;
    public ParticleSystem Snow;
    public AudioClip RainSoundLow;
    public AudioClip RainSoundHard;
    public float RainVolume;

    private ClimateController climate;

    public float RainCycle;
    private float weatherTime;
    private float weatherTimer;

    private bool raining;
    private Season startedSeason;
    private ParticleSystem.EmissionModule rainEmission;
    private ParticleSystem.EmissionModule snowEmission;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            gameObject.AddComponent<AudioSource>();
            audioSource = GetComponent<AudioSource>();

        }
        climate = GameObject.Find("ClimateController").GetComponent<ClimateController>();
        weatherTime = RainCycle;

        rainEmission = Rain.emission;
        snowEmission = Snow.emission;
    }

    void Start() 
    {
        
    }
    
    void Update()
    {
        float dt = Time.deltaTime;

        weatherTimer += dt * (climate.RainModifier + 2);

        if (weatherTimer > weatherTime)
        {
            ChangeRain(climate.RainModifier);
        }

        if (startedSeason != climate.GetSeason())
        {
            if (climate.GetSeason() == Season.Winter && raining)
            {
                RainStop();
                Snow.Play();
            }

            if (climate.GetSeason() == Season.Spring && raining)
            {
                Snow.Stop();
                RainPlay();
            }

            if (climate.GetSeason() == Season.Autumn)
            {
                if (!raining)
                {
                    weatherTimer = weatherTime; // Start raining immediately
                }
                else
                {
                    weatherTimer = 0f; // Reset rain
                }
                
            }

            
        }
    }

    private void ChangeRain(float modifier)
    {
        Debug.Log(!raining);
        raining = !raining;

        weatherTimer = 0f;
        weatherTime = RainCycle * (1f + (modifier + 2f));

        UpdateParticles();
    }

    private void UpdateParticles()
    {
        var currentSeason = climate.GetSeason();
        startedSeason = currentSeason;

        if (!raining)
        {
            RainStop();
            SnowStop();
        }
        else
        {
            if (currentSeason == Season.Winter)
            {
                SnowPlay();
            }
            else
            {
                RainPlay();
            }
        }
    }

    private void RainPlay()
    {
        var rain = climate.GetRain();
        if (rain > 0.5f)
        {
            audioSource.PlayOneShot(RainSoundHard);
        }
        else
        {
            audioSource.PlayOneShot(RainSoundLow);
        }
        
        Rain.Play();
    }
    private void RainStop()
    {
        audioSource.Stop();
        Rain.Stop();
    }

    private void SnowPlay()
    {
        Snow.Play();
    }
    private void SnowStop()
    {
        Snow.Stop();
    }
}
