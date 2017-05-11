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

    void Awake()
    {
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
            transform.localScale = TargetScale;
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
        var dt = Time.deltaTime;
        expandTimer += dt;

        var rate = Mathf.Clamp(expandTimer / expandTime, 0f, 1f);

        transform.localScale = Vector3.Lerp(startScale, TargetScale, rate);

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
}
