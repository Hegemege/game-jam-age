using System.Collections;
using System.Collections.Generic;
using System.Net.Cache;
using UnityEngine;

public class Branch : MonoBehaviour 
{
    [HideInInspector]
    public List<Vector3> BranchPositions;

    [HideInInspector]
    public List<Vector3> BranchDirections;

    [HideInInspector]
    public List<float> BranchThicknesses;

    [HideInInspector]
    public int Depth;

    [HideInInspector]
    public float InitialThickness;

    [HideInInspector]
    public float BranchLength;

    [HideInInspector]
    public float BranchInterval;

    [HideInInspector]
    public Vector3 CanopyPosition;

    public float ExpandTime;
    public float ExpandRandomness;

    private MeshFilter meshFilter;

    private float meshTransitionTime;
    private float meshTransitionTimer;

    private Vector3[] startVerts;
    private Vector3[] targetVerts;

    private bool recalculated;

    [HideInInspector]
    public Branch ParentBranch;
    [HideInInspector]
    public int ParentBranchIndex;

    private float branchMoveTimer;
    private float branchMoveTime;
    private Vector3 startBranchPosition;
    private Vector3 targetBranchPosition;

    void Awake() 
    {
        BranchPositions = new List<Vector3>();
        BranchDirections = new List<Vector3>();

        meshFilter = GetComponent<MeshFilter>();

        meshTransitionTime = ExpandTime + Random.Range(-ExpandRandomness, ExpandRandomness);
        meshTransitionTimer = meshTransitionTime;

        branchMoveTime = meshTransitionTime;
        branchMoveTimer = branchMoveTime;
    }

    void Start()
    {
        startBranchPosition = transform.localPosition;
        targetBranchPosition = transform.localPosition;
    }
    
    void Update()
    {
        float dt = Time.deltaTime;

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

        if (!recalculated)
        {
            recalculated = true;
            meshFilter.mesh.RecalculateBounds();
            meshFilter.mesh.RecalculateNormals();
        }

        if (ParentBranch != null)
        {
            // Move to current owner's canopy position
            branchMoveTimer += dt;
            var branchMoveRate = Mathf.Clamp(branchMoveTimer / branchMoveTime, 0f, 1f);
            transform.localPosition = Vector3.Lerp(startBranchPosition, targetBranchPosition, branchMoveRate);
        }
    }

    public void SetNewBranchPosition(bool instant)
    {
        if (ParentBranch == null) return;

        if (instant)
        {
            // Hack, sometimes breaks
            if (ParentBranchIndex > ParentBranch.BranchPositions.Count - 1)
            {
                ParentBranchIndex = ParentBranch.BranchPositions.Count - 1;
            }

            transform.localPosition = ParentBranch.BranchPositions[ParentBranchIndex];
            return;
        }

        startBranchPosition = transform.localPosition;
        targetBranchPosition = ParentBranch.BranchPositions[ParentBranchIndex];

        branchMoveTimer = 0f;
    }

    public void SetTargetMesh(Mesh mesh, bool instant)
    {
        recalculated = false;

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
