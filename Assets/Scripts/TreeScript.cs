using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeScript : MonoBehaviour
{
    private ClimateController climate;

    public float GrowthRate;

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
        climate = GameObject.Find("ClimateController").GetComponent<ClimateController>();
        Leaves = InitialLeaves;
    }
    
	void Start ()
    {

    }
	
	void Update ()
    {
        Photosynthesis();
        HandleWater();
        Grow();

        // Death stuff or whatever
        if (Water >= MaxWater)
        {
            Debug.Log("The tree is rotten!");
        }
        else if (Water <= 0)
        {
            Debug.Log("The tree dried out!");
        }


    }

    void Photosynthesis()
    {
        // Generates energy from sunlight and leaves
        Energy += Leaves * climate.Sunlight * Time.deltaTime;

        //energy = Mathf.Min(max_energy, energy);
    }

    void HandleWater()
    {
        // Collect water from rain
        Water += climate.Rain * Time.deltaTime;

        // Evaporate water because of the temperature (And sunlight?)
        Water -= Mathf.Max(0, climate.Temperature * (climate.Sunlight + 1) * Time.deltaTime * 0.1f);
    }

    void Grow()
    {
        // Growth is affected by the current temperature and it needs water/energy
        float growth = Time.deltaTime * climate.Temperature * GrowthRate;
        growth = Mathf.Min(growth, Energy, Water);
        growth = Mathf.Max(0, growth);
        if (growth > 0)
        {
            // Growing consumes energy and water
            Energy -= Time.deltaTime * climate.Temperature;
            Water -= Time.deltaTime * climate.Temperature;
        }
    }

}
