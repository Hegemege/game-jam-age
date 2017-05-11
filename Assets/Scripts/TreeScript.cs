using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class TreeScript : MonoBehaviour
{
    private ClimateController climate;
    private Text energyText;
    private Text waterText;

    public float EnergyPerSecond;
    public float WaterPerSecond;
    public float WaterPerTemperature;
    //public float WaterPerSunlight;

    public float WaterGain;
    public float EnergyGain;
    public float WinterWaterGain;
    public float WinterEnergyGain;
    public Vector2 SpringWaterTarget;
    public Vector2 SummerWaterTarget;
    public Vector2 AutumnWaterTarget;
    public Vector2 WinterWaterTarget;

    public Vector2 SpringEnergyTarget;
    public Vector2 SummerEnergyTarget;
    public Vector2 AutumnEnergyTarget;
    public Vector2 WinterEnergyTarget;

    private float size;
    private float growthRate; // Current growth rate

    public float GrowthMultiplier; // For fine tuning the gameplay
    public float InitialEnergy;
    public float InitialWater;

    [HideInInspector]
    public float Energy; // Tree needs energy to grow (gain from photosynthesis)
    [HideInInspector]
    public float Water; // Tree needs water to survive (gain from rain + passively from ground)

    public float MaxEnergy; // How much energy can be held in storage
    public float MaxWater; // How much water can be held in storage

    public int InitialLeaves;
    [HideInInspector]
    public int Leaves;

    void Awake()
    {
        waterText = GameObject.Find("WaterText").GetComponent<Text>();
        energyText = GameObject.Find("EnergyText").GetComponent<Text>();
        climate = GameObject.Find("ClimateController").GetComponent<ClimateController>();
        Leaves = InitialLeaves;
        Water = InitialWater;
        Energy = InitialEnergy;

        size = 1;
    }
    
	void Start ()
    {
        
    }
	
	void Update ()
    {
        energyText.text = "Energy: " + Energy.ToString("F1");
        waterText.text = "Water: " + Water.ToString("F1");


        var season = climate.GetSeason();
        CollectResources(season);
        UseResources(season);
        Grow(season);

        // Check status (Dead/Alive)
        if (Water >= MaxWater)
        {
            Debug.Log("The tree is rotten!");
        }
        else if (Water <= 0)
        {
            Debug.Log("The tree dried out!");
        }
    }

    private void CollectResources(Season season)
    {
        float energyGain = EnergyGain;
        float waterGain = WaterGain;
        if (season == Season.Winter)
        {
            energyGain = WinterEnergyGain;
            waterGain = WinterWaterGain;
        }

        // Collect water from rain
        Water += climate.GetRain() * Time.deltaTime * waterGain;
        Water = Mathf.Clamp(Water, 0, MaxWater);

        // Generates energy from sunlight and leaves
        Energy += Leaves * climate.GetSunlight() * Time.deltaTime * energyGain;
        Energy = Mathf.Clamp(Energy, 0, MaxEnergy);
    }

    private void UseResources(Season season)
    {
        // Staying alive requires water and energy
        Energy -= EnergyPerSecond * Time.deltaTime;
        Energy = Mathf.Clamp(Energy, 0, MaxEnergy);

        // Temperature (and sunlight) also evaporates water
        Water -= WaterPerSecond * Time.deltaTime;
        if (climate.GetTemperature() > 0)
        {
            Water -= WaterPerTemperature * climate.GetTemperature() * Time.deltaTime;
        }

        //Water -= climate.GetSunlight() * WaterPerSunlight * Time.deltaTime;
        Water = Mathf.Clamp(Water, 0, MaxWater);
    }

    private void Grow(Season season)
    {
        var seasonParams = GetSeasonParamenters(season);

        // Growth is affected by the current temperature
        growthRate = climate.GetTemperature() * GrowthMultiplier;

        // Growing needs water and energy
        growthRate = Mathf.Min(Water, Energy, growthRate);
        growthRate = Mathf.Max(0, growthRate);
        if (growthRate > 0)
        {
            // Growing consumes energy and water
            Energy -= Time.deltaTime * growthRate;
            Water -= Time.deltaTime * growthRate;
            size += Time.deltaTime * growthRate;
        }
    }

    public void SeasonalGrowth(Season nextSeason)
    {
        // TODO
    }

    private Vector2[] GetSeasonParamenters(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return new Vector2[] { SpringWaterTarget, SpringEnergyTarget};
            case Season.Summer:
                return new Vector2[] { SummerWaterTarget, SummerEnergyTarget };
            case Season.Autumn:
                return new Vector2[] { AutumnWaterTarget, AutumnEnergyTarget };
            case Season.Winter:
                return new Vector2[] { WinterWaterTarget, WinterEnergyTarget };
            default:
                Debug.LogError("Unknown season!" + season);
                return new Vector2[] { SummerWaterTarget, SummerEnergyTarget };
        }
    }


}
