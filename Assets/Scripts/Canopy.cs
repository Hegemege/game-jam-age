using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canopy : MonoBehaviour
{
    private Vector3 originalScale;

    private Vector3 startScale;

    [HideInInspector]
    public Vector3 TargetScale;

    [HideInInspector]
    public bool SkipExpansion;

    public float ExpandTime;
    public float ExpandRandomness;

    private float expandTime;
    private float expandTimer;

    void Awake()
    {
        originalScale = new Vector3(0.1f, 0.1f, 0.1f);

        expandTime = ExpandTime + Random.Range(-ExpandRandomness, ExpandRandomness);
        expandTimer = 0f;
    }

    void Start() 
    {
        if (SkipExpansion)
        {
            expandTimer = expandTime;
            transform.localScale = TargetScale;
        }
        else
        {
            startScale = originalScale;
        }
    }
    
    void Update()
    {
        var dt = Time.deltaTime;
        expandTimer += dt;

        var rate = Mathf.Clamp(expandTimer / expandTime, 0f, 1f);

        transform.localScale = Vector3.Lerp(startScale, TargetScale, rate);
    }
}
