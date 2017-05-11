using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class TreeScript : MonoBehaviour
{
    private ClimateController climate;
    private Text energyText;
    private Text waterText;

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

        Photosynthesis();
        HandleWater();
        Grow();

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

    private void Photosynthesis()
    {
        // Generates energy from sunlight and leaves
        Energy += Leaves * climate.GetSunlight() * Time.deltaTime;
        Energy = Mathf.Clamp(Energy, 0, MaxEnergy);
    }

    private void HandleWater()
    {
        // Collect water from rain
        Water += climate.GetRain() * Time.deltaTime;

        // Evaporate water because of the temperature (And sunlight?)
        Water -= Mathf.Max(0, climate.GetTemperature() * (climate.GetSunlight() + 1) * Time.deltaTime * 0.1f);

        Water = Mathf.Clamp(Water, 0, MaxWater);
    }

    private void Grow()
    {
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
}
