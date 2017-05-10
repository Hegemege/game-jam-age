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
    public float SeasonInterpolationLength;
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
        CurrentSeason = StartingSeason;
        SeasonInterpolationLength = Mathf.Clamp(SeasonInterpolationLength, 0, SeasonLength);
        InterpolateParameters(SeasonInterpolationLength / 2, CurrentSeason);
    }

    void Start() 
    {
        
    }
    
    void Update() 
    {
        UpdateSeason();
    }

    

    


    private void UpdateSeason()
    {
        // Changes the season and the temperature after x seconds
        seasonTimer += Time.deltaTime;
        if (seasonTimer > SeasonLength)
        {
            seasonTimer = 0;
            CurrentSeason = GetNextSeason(CurrentSeason);
        }
        if (seasonTimer < SeasonInterpolationLength)
        {
            // We are closing to the current seasons target parameters
            float t = seasonTimer + SeasonInterpolationLength / 2.0f;
            InterpolateParameters(t, CurrentSeason);
        }
        else if (seasonTimer > (SeasonLength - (SeasonInterpolationLength / 2))) 
        {
            // The current season is ending
            float t = seasonTimer - (SeasonLength - (SeasonInterpolationLength / 2));
            InterpolateParameters(t, GetNextSeason(CurrentSeason));
        }
        else
        {
            // Mid season, no actual intepolation happens here
            float t = SeasonInterpolationLength;
            InterpolateParameters(t, CurrentSeason);
        }
        

    }

    private void InterpolateParameters(float timeFromPrevSeason, Season targetSeason)
    {
        var targetTemp = GetSeasonTemperature(targetSeason);
        var targetRain = GetSeasonRain(targetSeason);
        var targetSun = GetSeasonSunlight(targetSeason);

        var prevSeason = GetPrevSeason(targetSeason);
        var prevTemp = GetSeasonTemperature(prevSeason);
        var prevRain = GetSeasonRain(prevSeason);
        var prevSun = GetSeasonSunlight(prevSeason);

        float position = Mathf.Clamp(timeFromPrevSeason / SeasonInterpolationLength, 0, 1);
        Temperature = prevTemp + position * (targetTemp - prevTemp);
        Rain = prevRain + position * (targetRain - prevRain);
        Sunlight = prevSun + position * (targetSun - prevSun);
    }



    // Helper methods

    private float[] GetSeasonParameters(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return new float[] {TemperatureSpring, RainSpring, SunlightSpring };
            case Season.Summer:
                return new float[] {TemperatureSummer, RainSummer, SunlightSummer };
            case Season.Autumn:
                return new float[] {TemperatureAutumn, RainAutumn, SunlightAutumn };
            case Season.Winter:
                return new float[] {TemperatureWinter, RainWinter, SunlightWinter };
            default:
                Debug.LogError("Unknown season!" + season);
                return new float[] {0, 0, 0 }; ;
        }
    }

    private float GetSeasonTemperature(Season season)
    {
        return GetSeasonParameters(season)[0];
    }

    private float GetSeasonRain(Season season)
    {
        return GetSeasonParameters(season)[1];
    }

    private float GetSeasonSunlight(Season season)
    {
        return GetSeasonParameters(season)[2];
    }

    private Season GetPrevSeason(Season season)
    {
        int s = SeasonToInt(season);
        s = Mathf.Abs(s - 1) % 4;
        return IntToSeason(s);
    }

    private Season GetNextSeason(Season season)
    {
        int s = SeasonToInt(season);
        s = (s + 1) % 4;
        return IntToSeason(s);
    }

    private int SeasonToInt(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return 0;
            case Season.Summer:
                return 1;
            case Season.Autumn:
                return 2;
            case Season.Winter:
                return 3;
            default:
                Debug.LogError("Unknown season!" + season);
                return 0;
        }
    }

    private Season IntToSeason(int season)
    {
        switch (season)
        {
            case 0:
                return Season.Spring;
            case 1:
                return Season.Summer;
            case 2:
                return Season.Autumn;
            case 3:
                return Season.Winter;
            default:
                Debug.LogError("Unknown season!" + season);
                return 0;
        }
    }
}
