using UnityEngine;
using System.Collections;

public class ColorChanger : MonoBehaviour
{
    private Light directionalLight;

    public float SpringLightIntensity;
    public float SummerLightIntensity;
    public float AutumnLightIntensity;
    public float WinterLightIntensity;

    public Color SpringLightColor;
    public Color SummerLightColor;
    public Color AutumnLightColor;
    public Color WinterLightColor;

    public Color SpringAmbientLightColor;
    public Color SummerAmbientLightColor;
    public Color AutumnAmbientLightColor;
    public Color WinterAmbientLightColor;

    private Color startLightColor;
    private Color targetLightColor;
    private Color startAmbientColor;
    private Color targetAmbientColor;
    private float startIntensity;
    private float targetIntensity;

    [HideInInspector]
    public Color CurrentLightColor;

    void Awake()
    {
        directionalLight = GetComponentInChildren<Light>();
    }

    void Start() 
    {
        
    }
    
    void Update() 
    {
        
    }

    public void InterpolateColors(float ratio, Season targetSeason)
    {
        // Copypasta because it's fast
        switch (targetSeason)
        {
            case Season.Spring:
                startLightColor = WinterLightColor;
                targetLightColor = SpringLightColor;
                startAmbientColor = WinterAmbientLightColor;
                targetAmbientColor = SpringAmbientLightColor;
                startIntensity = WinterLightIntensity;
                targetIntensity = SpringLightIntensity;
                break;
            case Season.Summer:
                startLightColor = SpringLightColor;
                targetLightColor = SummerLightColor;
                startAmbientColor = SpringAmbientLightColor;
                targetAmbientColor = SummerAmbientLightColor;
                startIntensity = SpringLightIntensity;
                targetIntensity = SummerLightIntensity;
                break;
            case Season.Autumn:
                startLightColor = SummerLightColor;
                targetLightColor = AutumnLightColor;
                startAmbientColor = SummerAmbientLightColor;
                targetAmbientColor = AutumnAmbientLightColor;
                startIntensity = SummerLightIntensity;
                targetIntensity = AutumnLightIntensity;
                break;
            case Season.Winter:
                startLightColor = AutumnLightColor;
                targetLightColor = WinterLightColor;
                startAmbientColor = AutumnAmbientLightColor;
                targetAmbientColor = WinterAmbientLightColor;
                startIntensity = AutumnLightIntensity;
                targetIntensity = WinterLightIntensity;
                break;
        }

        // Canopies use this
        CurrentLightColor = Color.Lerp(startLightColor, targetLightColor, ratio);

        directionalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, ratio);
        directionalLight.color = CurrentLightColor;

        RenderSettings.ambientLight = Color.Lerp(startAmbientColor, targetAmbientColor, ratio);
    }
}
