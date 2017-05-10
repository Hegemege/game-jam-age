using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public class ClimateController : MonoBehaviour 
{
    [HideInInspector]
    public int Year;

    // Season changing
    private float seasonTimer;
    public float SeasonLength;

    public float TemperatureSpring;
    public float TemperatureSummer;
    public float TemperatureAutumn;
    public float TemperatureWinter;

    public float RainSpring;
    public float RainSummer;
    public float RainAutumn;
    public float RainWinter;

    public float SunlightSpring;
    public float SunlightSummer;
    public float SunlightAutumn;
    public float SunlightWinter;

    // Season parameters
    [HideInInspector]
    public float Rain;
    [HideInInspector]
    public float Sunlight;
    [HideInInspector]
    public float Temperature;

    public Season StartingSeason;
    [HideInInspector]
    public Season CurrentSeason;

    void Awake()
    {
        Year = 1;
    }

    void Start() 
    {
        
    }
    
    void Update() 
    {
        
    }


    private void CheckSeason()
    {
        // Changes the season and the temperature after x seconds
        seasonTimer += Time.deltaTime;
        if (seasonTimer > SeasonLength)
        {
            seasonTimer = 0;
            switch (CurrentSeason)
            {
                case Season.Spring:
                    CurrentSeason = Season.Summer;
                    break;
                case Season.Summer:
                    CurrentSeason = Season.Autumn;
                    break;
                case Season.Autumn:
                    CurrentSeason = Season.Winter;
                    break;
                case Season.Winter:
                    CurrentSeason = Season.Spring;
                    Year += 1;
                    break;
            }
        }

    }

    private void UpdateParameters()
    {
        switch (CurrentSeason)
        {
            case Season.Spring:
                Temperature = TemperatureSpring;
                Rain = RainSpring;
                Sunlight = SunlightSpring;
                break;
            case Season.Summer:
                Temperature = TemperatureSummer;
                Rain = RainSummer;
                Sunlight = SunlightSummer;
                break;
            case Season.Autumn:
                Temperature = TemperatureAutumn;
                Rain = RainAutumn;
                Sunlight = SunlightAutumn;
                break;
            case Season.Winter:
                Temperature = TemperatureWinter;
                Rain = RainWinter;
                Sunlight = SunlightWinter;
                break;
        }
    }
}
