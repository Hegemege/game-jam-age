using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public class ClimateController : MonoBehaviour 
{
    public GameObject SpringBG;
    public GameObject SummerBG;
    public GameObject AutumnBG;
    public GameObject WinterBG;
    public GameObject SpringFront;
    public GameObject SummerFront;
    public GameObject AutumnFront;
    public GameObject WinterFront;

    private Text temperatureGauge;
    private Text yearText;
    private ColorChanger seasonColors;

    [HideInInspector]
    public int Year;

    // User input
    public float RainStepSize;
    public float SunlightStepSize;
    public float TemperatureModifier { get; set; }
    public float RainModifier { get; set; }
    public float SunlightModifier { get; set; }

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
    private float rain;
    private float sunlight;
    private float temperature;

    public Season StartingSeason;
    private Season currentSeason;

    void Awake()
    {
        temperatureGauge = GameObject.Find("TemperatureGauge").GetComponent<Text>();
        yearText = GameObject.Find("YearText").GetComponent<Text>();

        seasonColors = GetComponentInChildren<ColorChanger>();


		var sunSlider = GameObject.Find("SunSlider").GetComponent<Slider>();
		sunSlider.onValueChanged.AddListener((value)=>{SunlightModifier = value;});
		var rainSlider = GameObject.Find("RainSlider").GetComponent<Slider>();
		rainSlider.onValueChanged.AddListener((value)=>{RainModifier = value;});



        Year = 1;
        currentSeason = StartingSeason;
        SeasonInterpolationLength = Mathf.Clamp(SeasonInterpolationLength, 0, SeasonLength);
        InterpolateParameters(1, currentSeason);

        SetSeasonImgAlpha(0, Season.Autumn);
        SetSeasonImgAlpha(0, Season.Summer);
        SetSeasonImgAlpha(0, Season.Spring);
        SetSeasonImgAlpha(0, Season.Winter);

        UpdateSeason();
    }

    void Start() 
    {
        
    }
    
    void Update() 
    {
        UpdateSeason();

        temperatureGauge.text = temperature.ToString("F1") + " °C";
        yearText.text = "Year: " + Year.ToString();
    }

    public Season GetSeason()
    {
        return currentSeason;
    }

    public float GetTemperature()
    {
        return temperature;
    }

    public float GetRain()
    {
        return rain;
    }

    public float GetSunlight()
    {
        return sunlight;
    }


    private void UpdateSeason()
    {
        // Change the season after x seconds
        seasonTimer += Time.deltaTime;
        if (seasonTimer > SeasonLength)
        {
            seasonTimer = 0;
            ChangeSeason(currentSeason, GetNextSeason(currentSeason));
        }

        // Interpolate temperature when the season is changing
        // Interpolation starts at the end of each season
        var midSeasonTime = SeasonLength - SeasonInterpolationLength;
        var time = seasonTimer - midSeasonTime;
        var ratio = Mathf.Clamp(time / SeasonInterpolationLength, 0, 1); // Ratio stays 0, until the season starts to change
        InterpolateParameters(ratio, GetNextSeason(currentSeason));

        // Change the background
        SetSeasonImgAlpha((1 - ratio), currentSeason);
        SetSeasonImgAlpha(ratio, GetNextSeason(currentSeason));
        

        // Adjust parameters according to user input
        temperature *= (1 + TemperatureModifier + SunlightModifier * SunlightStepSize);
        rain *= (1 + (RainModifier * RainStepSize));
        sunlight *= (1 + (SunlightModifier * SunlightStepSize)); 
    }

    private void InterpolateParameters(float ratio, Season targetSeason)
    {
        seasonColors.InterpolateColors(ratio, targetSeason);

        var targetTemp = GetSeasonTemperature(targetSeason);
        var targetRain = GetSeasonRain(targetSeason);
        var targetSun = GetSeasonSunlight(targetSeason);

        var prevSeason = GetPrevSeason(targetSeason);
        var prevTemp = GetSeasonTemperature(prevSeason);
        var prevRain = GetSeasonRain(prevSeason);
        var prevSun = GetSeasonSunlight(prevSeason);

        temperature = prevTemp + ratio * (targetTemp - prevTemp);
        rain = prevRain + ratio * (targetRain - prevRain);
        sunlight = prevSun + ratio * (targetSun - prevSun);
    }

    private void ChangeSeason(Season current, Season target)
    {
        // Invokes growth on the tree
        // Spends energy

        var tree = GameObject.Find("Tree").GetComponent<TreeScript>();
		tree.SeasonalGrowth(current, target, SeasonLength); // Growth is affected by the previous season

        // And finally set the current season
        currentSeason = target;
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
		s--;
		if (s < 0) 
		{
			s = 3;
		}

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

    private GameObject[] GetSeasonImages(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return new GameObject[] { SpringBG, SpringFront };
            case Season.Summer:
                return new GameObject[] { SummerBG, SummerFront };
            case Season.Autumn:
                return new GameObject[] { AutumnBG, AutumnFront };
            case Season.Winter:
                return new GameObject[] { WinterBG, WinterFront };
            default:
                Debug.LogError("Unknown season!" + season);
                return new GameObject[] { SpringBG, SpringFront };
        }
    }

    private void SetSeasonImgAlpha(float alpha, Season season)
    {
        var imgObjs = GetSeasonImages(season);
        foreach (var obj in imgObjs)
        {
            if (obj)
            {
                Color c = obj.GetComponent<Image>().color;
                c.a = alpha;
                obj.GetComponent<Image>().color = c;
            }
        }
    }

}
