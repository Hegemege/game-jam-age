using System.Collections;
using System.Collections.Generic;
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

    void Awake() 
    {
        BranchPositions = new List<Vector3>();
        BranchDirections = new List<Vector3>();
    }

    void Start() 
    {
        
    }
    
    void Update() 
    {
        
    }
}
