using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeScript : MonoBehaviour {
    public float rain { get; set; }
    public float sunlight { get; set;}
    public float temperature = 20;
    public float growth_rate = 0.01f;

    public float energy = 10; // Tree needs energy to grow (gain from photosynthesis)
    public float water = 500; // Tree needs water to survive (gain from rain)
    
    public float age = 0;

    float max_energy = 1000;
    float max_water = 1000;

    float season_timer = 0;
    float season_change_time = 10;

    int temp_spring = 13;
    int temp_summer = 20;
    int temp_autumn = 5;
    int temp_winter = 1;

    public int leafs = 1; // Scales the amount of energy from sunlight (Too complex?)

    public enum Seasons
    {
        Spring, Summer, Autumn, Winter
    }
    public Seasons season = Seasons.Spring;


	// Use this for initialization
	void Start () {
       // gameObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        season = Seasons.Spring;
        temperature = temp_spring;

    }
	
	// Update is called once per frame
	void Update () {

        check_season();
        photosynthesis();
        handleWater();
        grow();
        //changeColor();

        // Death stuff or whatever
        if (water >= max_water)
        {
            Debug.Log("The tree is rotten!");
        }
        else if (water <= 0)
        {
            Debug.Log("The tree dried out!");
        }


    }
    
    void check_season()
    {
        // Changes the season and the temperature after x seconds
        season_timer += Time.deltaTime;
        bool change_season = season_timer > season_change_time;
        if (change_season)
        {
            season_timer = 0;
            switch (season)
            {
                case Seasons.Spring:
                    season = Seasons.Summer;
                    temperature = temp_summer;
                    break;
                case Seasons.Summer:
                    season = Seasons.Autumn;
                    temperature = temp_autumn;
                    break;
                case Seasons.Autumn:
                    season = Seasons.Winter;
                    temperature = temp_winter;
                    break;
                case Seasons.Winter:
                    season = Seasons.Spring;
                    temperature = temp_spring;
                    age++;
                    break;
            }
        }
        
    }

    void photosynthesis()
    {
        // Generates energy from sunlight and leaves
        energy += leafs * sunlight * Time.deltaTime;

        energy = Mathf.Min(max_energy, energy);
    }

    void handleWater()
    {
        // Collect water from rain
        water += rain * Time.deltaTime;
        water = Mathf.Min(max_water, water);

        // Evaporate water because of the temperature (And sunlight?)
        water -= Mathf.Max(0, temperature * (sunlight + 1) * Time.deltaTime * 0.1f);
        water = Mathf.Max(0, water);
    }

    void grow()
    {
        // Growth is affected by the current temperature and it needs water/energy
        float growth = Time.deltaTime * temperature * growth_rate;
        growth = Mathf.Min(growth, energy, water);
        growth = Mathf.Max(0, growth);
        if (growth > 0)
        {
            // Growing consumes energy and water
            energy -= Time.deltaTime * temperature;
            water -= Time.deltaTime * temperature;
            transform.localScale += new Vector3(growth, growth);
        }
        
    }

    void changeColor()
    {
        // For prototyping (displaying water level)
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.5f, water/max_water, 1);
    }

    public float getEnergy() { return energy; }
    public float getWater() { return water; } 


}
