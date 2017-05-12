using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private ClimateController climate;

    public Image WaterOptimalImage;
    public Image EnergyOptimalImage;

    public Image SunlightFade;
    public float SunlightLow;
    public float SunlightHigh;

    private float parentWidth;

    void Awake()
    {
        climate = GameObject.Find("ClimateController").GetComponent<ClimateController>();
        parentWidth = ((RectTransform) WaterOptimalImage.transform.parent).sizeDelta.x;
    }

    void Start() 
    {
        
    }
    
    void Update() 
    {
        
    }

    public void UpdateOptimalUI(float waterLow, float waterHigh, float energyLow, float energyHigh)
    {
        var y = WaterOptimalImage.transform.localPosition.y;
        var z = WaterOptimalImage.transform.localPosition.z;
        WaterOptimalImage.transform.localPosition = new Vector3(parentWidth * waterLow, y, z);
        var height = ((RectTransform) WaterOptimalImage.transform).sizeDelta.y;
        ((RectTransform) WaterOptimalImage.transform).sizeDelta = new Vector2((waterHigh - waterLow) * parentWidth, height);

        y = EnergyOptimalImage.transform.localPosition.y;
        z = EnergyOptimalImage.transform.localPosition.z;
        EnergyOptimalImage.transform.localPosition = new Vector3(parentWidth * energyLow, y, z);
        height = ((RectTransform)EnergyOptimalImage.transform).sizeDelta.y;
        ((RectTransform)EnergyOptimalImage.transform).sizeDelta = new Vector2((energyHigh - energyLow) * parentWidth, height);

        var newSunlightColor = SunlightFade.color;
        newSunlightColor.a = Mathf.Lerp(SunlightLow, SunlightHigh, 1 - (climate.SunlightModifier + 2) / 4f);
        SunlightFade.color = newSunlightColor;
    }
}
