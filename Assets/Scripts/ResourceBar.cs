using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Events;

public class ResourceBar : MonoBehaviour {

    public TreeScript Tree;
    public bool ShowEnergy; // A stupid way to select between showing water or energy
    private Slider slider;


    void Awake()
    {
        slider = gameObject.GetComponent<Slider>();
		Tree = GameObject.Find ("Tree").GetComponent<TreeScript> ();
        
        if (ShowEnergy)
        {
            slider.maxValue = Tree.MaxEnergy;
        }
        else
        {
            slider.maxValue = Tree.MaxWater;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (ShowEnergy)
        {
            slider.value = Tree.Energy;
        }
        else
        {
            slider.value = Tree.Water;
        }
    }
}
