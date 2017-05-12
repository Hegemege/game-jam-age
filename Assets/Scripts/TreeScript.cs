using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class TreeScript : MonoBehaviour
{
    private ClimateController climate;
    private UIController climateUI;
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

	public Vector2 OptimalTemperature; // Affects growth

	public Vector2 SpringWaterTarget; // Affects leaves and growth
    public Vector2 SummerWaterTarget;
    public Vector2 AutumnWaterTarget;
    public Vector2 WinterWaterTarget;

	public Vector2 SpringEnergyTarget; // Affects growth
    public Vector2 SummerEnergyTarget;
    public Vector2 AutumnEnergyTarget;
    public Vector2 WinterEnergyTarget;

	public Vector2 SpringSunTarget; // Affects leaves
	public Vector2 SummerSunTarget;
	public Vector2 AutumnSunTarget;
	public Vector2 WinterSunTarget;

	public float SpringLeavesModifier;
	public float SummerLeavesModifier;
	public float AutumnLeavesModifier;
	public float WinterLeavesModifier;
	public float WinterLeavesMultiplier;

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
        climateUI = GameObject.Find("UI").GetComponent<UIController>();
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
		var energyDrain = EnergyPerSecond * Time.deltaTime;
		if (energyDrain > 0) 
		{
			Debug.LogError("Energy drain is positive! (Tree gains energy instead of consuming it!)");
		}
		Energy += energyDrain;
        Energy = Mathf.Clamp(Energy, 0, MaxEnergy);

        // Temperature (and sunlight) also evaporates water
		var waterDrain = WaterPerSecond * Time.deltaTime;
        Water += WaterPerSecond * Time.deltaTime;
        if (climate.GetTemperature() > 0)
        {
			waterDrain += WaterPerTemperature * climate.GetTemperature() * Time.deltaTime;
        }
		if (waterDrain > 0) 
		{
			Debug.LogError("Water drain is positive! (Tree gains water instead of consuming it!)");
		}
		Water += waterDrain;
        //Water -= climate.GetSunlight() * WaterPerSunlight * Time.deltaTime;
        Water = Mathf.Clamp(Water, 0, MaxWater);
    }

    private float CalculateOptimality(float value, float min, float max)
    {
        // Expects values from 0f to 1f
		if (min > max) 
		{
			Debug.LogError ("Minimum optimal value must be higher than maximum value! min=" + min.ToString () + " max=" + max.ToString ());
		}
		// Returns 1 if value is within range, and less than 1 otherwise
		var distance = 0f;
		if (min == 0 && max == 0) 
		{
			return 0;
		}
		else if (value < min) 
		{
			distance = min - value;
		} 
		else if (value > max) 
		{
			distance = value - max;
		}
		var optimality = 1 - distance;
		return Mathf.Clamp(optimality, 0, 1);;
    }

    private void Grow(Season season)
    {
		var waterParams = GetSeasonWaterTarget (season);
		var energyParams = GetSeasonEnergyTarget (season);
        var waterOptimality = CalculateOptimality(Water / MaxWater, waterParams[0], waterParams[1]);
        var energyOptimality = CalculateOptimality(Energy / MaxEnergy, energyParams[0], energyParams[1]);
		var temperatureOptimality = CalculateOptimality(climate.GetTemperature() / 100f, OptimalTemperature[0], OptimalTemperature[1]);

		waterOptimality = Mathf.Clamp(waterOptimality, 0, 1);
		energyOptimality = Mathf.Clamp(energyOptimality, 0, 1);
		temperatureOptimality = Mathf.Clamp(temperatureOptimality, 0, 1);

        // Growth is affected by temperature, energy and water
		var sizeGain = temperatureOptimality * waterOptimality * energyOptimality; // Always between 0 and 1
		seasonalSizeGain += sizeGain * Time.deltaTime;


		// Leaves are affected by water and sunlight
		var sunlightRange = GetSeasonSunlightTarget (season);
		var sunlightOptimality = CalculateOptimality(climate.GetSunlight(), sunlightRange[0], sunlightRange[1]);
		seasonalLeavesGain += waterOptimality * sunlightOptimality * Time.deltaTime;

        climateUI.UpdateOptimalUI(waterParams[0], waterParams[1], energyParams[0], energyParams[1]);
    }

	public void SeasonalGrowth(Season currentSeason, Season nextSeason, float seasonLength)
    {
		// Scale growth variables by seasonLenght (to range of [-1, 1])
		var sizeIncrease = Mathf.Clamp(seasonalSizeGain / seasonLength, 0, 1);  // Size cannot decrease
		var leavesMultiplier = seasonalLeavesGain / seasonLength; // Multiplier from parameter optimality [0, 1]
		leavesMultiplier = Mathf.Clamp (leavesMultiplier * 2, 0.5f, 1.5f);
		leavesMultiplier += GetSeasonLeavesModifier(currentSeason);

		if (currentSeason == Season.Winter) 
		{
			// Force a constant change for winter (Preferably 1, for no changes at all)
			leavesMultiplier = WinterLeavesMultiplier; 
		}

		Leaves = Mathf.Clamp(Leaves * leavesMultiplier, 0, 1);
		size += sizeIncrease;
		seasonalSizeGain = 0;
		seasonalLeavesGain = 0;


		// Update parameters for the TreeGenerator
		Debug.Log("Current Leaves: " + Leaves.ToString("F2") + " (previous multiplier: " + leavesMultiplier.ToString("F2") + ")");
		var generator = gameObject.GetComponent<TreeGenerator>();
        //Leaves = Mathf.Clamp(seasonalLeavesGain / seasonLength, 0, 1);
		generator.SeasonalGrowth(size, Leaves, nextSeason);
    }

    private Vector2[] GetSeasonParameters(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return new Vector2[] { SpringWaterTarget, SpringEnergyTarget, SpringSunTarget};
            case Season.Summer:
				return new Vector2[] { SummerWaterTarget, SummerEnergyTarget, SummerSunTarget};
            case Season.Autumn:
				return new Vector2[] { AutumnWaterTarget, AutumnEnergyTarget, AutumnSunTarget};
            case Season.Winter:
                return new Vector2[] { WinterWaterTarget, WinterEnergyTarget, WinterSunTarget};
            default:
                Debug.LogError("Unknown season!" + season);
                return new Vector2[] { SummerWaterTarget, SummerEnergyTarget, SummerSunTarget};
        }
    }

	private Vector2 GetSeasonWaterTarget(Season season) 
	{
		var parameters = GetSeasonParameters(season);
		return parameters [0];
	}
		

	private Vector2 GetSeasonEnergyTarget(Season season) 
	{
		var parameters = GetSeasonParameters(season);
		return parameters [1];
	}

	private Vector2 GetSeasonSunlightTarget(Season season) 
	{
		var parameters = GetSeasonParameters(season);
		return parameters [2];
	}

	private float GetSeasonLeavesModifier(Season season) 
	{
		switch (season)
		{
			case Season.Spring:
				return SpringLeavesModifier;
			case Season.Summer:
				return SummerLeavesModifier;
			case Season.Autumn:
				return AutumnLeavesModifier;
			case Season.Winter:
				return WinterLeavesModifier;
			default:
				Debug.LogError("Unknown season!" + season);
				return SpringLeavesModifier;
		}
	}


}
