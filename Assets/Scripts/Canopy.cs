using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canopy : MonoBehaviour
{
    private Vector3 originalScale;

    private Vector3 startScale;

    private Vector3 targetScale;
    private float leaves;

    [HideInInspector]
    public bool SkipExpansion;

    public float ExpandTime;
    public float ExpandRandomness;

    private float expandTime;
    private float expandTimer;

    private MeshFilter meshFilter;

    private float meshTransitionTime;
    private float meshTransitionTimer;

    private Vector3[] startVerts;
    private Vector3[] targetVerts;

    [HideInInspector]
    public Branch OwnerBranch;

    private float canopyMoveTimer;
    private float canopyMoveTime;
    private Vector3 startCanopyPosition;
    private Vector3 targetCanopyPosition;

    private ColorChanger colors;
    private MeshRenderer meshRenderer;

    private Color startColor;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        colors = GameObject.Find("ClimateController").GetComponent<ColorChanger>();

        startColor = meshRenderer.material.color;
        float startH;
        float startV;
        float startS;
        Color.RGBToHSV(startColor, out startH, out startV, out startS);

        startColor = Random.ColorHSV(startH + -0.1f, startH + 0.1f, 0.5f, 0.5f);

        originalScale = new Vector3(0.1f, 0.1f, 0.1f);

        expandTime = ExpandTime + Random.Range(-ExpandRandomness, ExpandRandomness);
        expandTimer = 0f;

        meshTransitionTime = expandTime;
        meshTransitionTimer = meshTransitionTime;

        canopyMoveTime = expandTime;
        canopyMoveTimer = canopyMoveTime;

        meshFilter = GetComponent<MeshFilter>();
    }

    void Start() 
    {
        if (SkipExpansion)
        {
            expandTimer = expandTime;
            transform.localScale = targetScale;
        }
        else
        {
            startScale = originalScale;
        }

        startCanopyPosition = transform.localPosition;
        targetCanopyPosition = transform.localPosition;
    }
    
    void Update()
    {
        SetColor();

        var dt = Time.deltaTime;
        expandTimer += dt;

        var rate = Mathf.Clamp(expandTimer / expandTime, 0f, 1f);

        transform.localScale = Vector3.Lerp(startScale, targetScale * leaves * 2f, rate);

        // Mesh transition
        meshTransitionTimer += dt;
        if (meshTransitionTimer < meshTransitionTime)
        {
            var meshRate = meshTransitionTimer / meshTransitionTime;

            var verts = meshFilter.mesh.vertices;

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = Vector3.Lerp(startVerts[i], targetVerts[i], meshRate);
            }

            meshFilter.mesh.vertices = verts;
        }

        // Move to current owner's canopy position
        canopyMoveTimer += dt;
        var canopyMoveRate = Mathf.Clamp(canopyMoveTimer / canopyMoveTime, 0f, 1f);
        transform.localPosition = Vector3.Lerp(startCanopyPosition, targetCanopyPosition, canopyMoveRate);
    }

    public void SetNewCanopyPosition(bool instant)
    {
        if (instant)
        {
            transform.localPosition = OwnerBranch.CanopyPosition;
            return;
        }

        startCanopyPosition = transform.localPosition;
        targetCanopyPosition = OwnerBranch.CanopyPosition;

        canopyMoveTimer = 0f;
    }

    public void SetTargetMesh(Mesh mesh, bool instant)
    {
        if (instant)
        {
            meshFilter.mesh = mesh;
            return;
        }

        meshTransitionTimer = 0f;
        startVerts = meshFilter.mesh.vertices;
        targetVerts = mesh.vertices;
    }

    public void SetScale(Vector3 newScale)
    {
        targetScale = newScale;
    }

    public void SetLeaves(float newLeaves, bool vanish)
    {
        // Scale the canopy based on the given value, and whether to complete vanish it
        if (vanish)
        {
            leaves = 0f;
        }
        else
        {
            leaves = newLeaves;
        }

        startScale = transform.localScale;

        expandTimer = 0f;
    }

    private void SetColor()
    {
        meshRenderer.material.color = (startColor + colors.CurrentLightColor) / 2f;
    }
}
