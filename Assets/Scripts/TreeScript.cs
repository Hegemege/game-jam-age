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

    private float seasonalSizeGain; // Size change for the next season
    private float seasonalLeavesGain; // Change in leaves for the next season
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

    public float InitialLeaves;
    [HideInInspector]
    public float Leaves;

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

    private float CalculateOptimality(float value, float min, float max)
    {
        // Expects values from 0f to 1f

		// Returns 1 if value is within range, and less than 1 otherwise
		var distance = 0f;
		if (value < min) 
		{
			distance = min - value;
		} 
		else if (value > max) 
		{
			distance = value - max;
		}
		var optimality = 1 - distance;
        return optimality;
    }

    private void Grow(Season season)
    {
        var seasonParams = GetSeasonParamenters(season);
        var waterParams = seasonParams[0];
        var energyParams = seasonParams[1];
        var waterOptimality = CalculateOptimality(Water / MaxWater, waterParams[0], waterParams[1]);
        var energyOptimality = CalculateOptimality(Energy / MaxEnergy, energyParams[0], energyParams[1]);
		var temperatureOptimality = CalculateOptimality(climate.GetTemperature() / 100f, 0.20f, 0.40f);  // Optimal temperature is between 20 and 40 degrees

		waterOptimality = Mathf.Clamp(waterOptimality, 0, 1);
		energyOptimality = Mathf.Clamp(energyOptimality, 0, 1);
		temperatureOptimality = Mathf.Clamp(temperatureOptimality, 0, 1);

        // Growth is affected by the current temperature
		var sizeGain = temperatureOptimality * waterOptimality * energyOptimality; // Always between 0 and 1
		seasonalSizeGain += sizeGain * Time.deltaTime;

		var sunlightOptimality = CalculateOptimality(climate.GetSunlight(), 0.60f, 1f);
		seasonalLeavesGain += sunlightOptimality * Time.deltaTime;


    }

	public void SeasonalGrowth(Season nextSeason, float seasonLength)
    {
		// Scale growth variables by seasonLenght (to range of [-1, 1])
		var sizeIncrease = Mathf.Clamp(seasonalSizeGain / seasonLength, 0, 1);  // Size cannot decrease
		Leaves = Mathf.Clamp(seasonalLeavesGain / seasonLength, 0, 1); // Leaves cannot be negative
		size += sizeIncrease;
		seasonalSizeGain = 0;
		seasonalLeavesGain = 0;


		// Update parameters for the TreeGenerator
        Debug.Log(size);
		var generator = gameObject.GetComponent<TreeGenerator>();
        Leaves = Mathf.Clamp(seasonalLeavesGain / seasonLength, 0, 1);
        generator.SeasonalGrowth(size, Leaves);
    }

    private Vector2[] GetSeasonParamenters(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return new Vector2[] { SpringWaterTarget, SpringEnergyTarget };
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
