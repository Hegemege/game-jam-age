using UnityEngine;
using System.Collections;

public class WeatherController : MonoBehaviour
{
    public ParticleSystem Rain;
    public ParticleSystem Snow;

    private ClimateController climate;

    public float RainCycle;
    private float weatherTime;
    private float weatherTimer;

    private bool raining;
    private Season startedSeason;

    void Awake()
    {
        climate = GameObject.Find("ClimateController").GetComponent<ClimateController>();
        weatherTime = RainCycle;
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
                Rain.Stop();
                Snow.Play();
            }

            if (climate.GetSeason() == Season.Spring && raining)
            {
                Snow.Stop();
                Rain.Play();
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
            Rain.Stop();
            Snow.Stop();
        }
        else
        {
            if (currentSeason == Season.Winter)
            {
                Snow.Play();
            }
            else
            {
                Rain.Play();
            }
        }
    }
}
