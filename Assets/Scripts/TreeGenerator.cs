using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    private Mesh treeMesh;

    private MeshFilter meshFilter;

    public Vector3[] trunkShapeVertices;
    public Vector2[] trunkShapeUVs;

    public int TrunkLength;
    public float TrunkInterval;
    public float IntervalVariance;

    public float TrunkStartThickness;
    public float TrunkShrinkingFactor;

    // Randomness
    public float RandomRingThicknessVariance;
    public float RandomVertexThicknessVariance;

    public float RingBaseDrift;

    private List<List<float>> trunkThickness;
    private List<float> intervalVariances;
    private List<Vector3> ringBasePositions;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        // Initialize the trunk thickness
        trunkThickness = new List<List<float>>();
        for (var i = 0; i < TrunkLength; i++)
        {
            trunkThickness.Add(new List<float>());
        }

        // Generate randomness into the trunk's thickness
        for (var i = 0; i < TrunkLength; i++)
        {
            var ringThickness = 1 + Random.Range(-RandomRingThicknessVariance, RandomRingThicknessVariance);

            for (var j = 0; j < trunkShapeVertices.Length; j++)
            {
                var vertexThickness = Random.Range(0f, RandomVertexThicknessVariance);
                trunkThickness[i].Add(ringThickness + vertexThickness);
            }
        }

        // Generate random trunk intervals
        intervalVariances = new List<float>();
        for (var i = 0; i < TrunkLength; i++)
        {
            intervalVariances.Add(TrunkInterval + Random.Range(-IntervalVariance, IntervalVariance));
        }

        // Base position drift
        ringBasePositions = new List<Vector3>();
        Vector3 basePosition = Vector3.zero;
        for (var i = 0; i < TrunkLength; i++)
        {
            ringBasePositions.Add(basePosition);

            var accX = Random.Range(-RingBaseDrift, RingBaseDrift);
            var accZ = Random.Range(-RingBaseDrift, RingBaseDrift);

            basePosition.x += accX;
            basePosition.z += accZ;
        }

        GenerateTree();
    }

    void Start() 
    {
        
    }
    
    void Update() 
    {
        
    }

    private void GenerateTree()
    {
        // Clear, initialize, assign mesh
        treeMesh = new Mesh();

        // Use lists becasue we dont know the final size, convert to arrays when assigning to the mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        meshFilter.mesh = treeMesh;

        // Generate the vertices and triangles
        Vector3 direction = Vector3.up;

        Vector3 accumulatedBasePosition = Vector3.zero;

        for (int i = 0; i < TrunkLength - 1; i++)
        {
            var ringThickness = TrunkStartThickness * Mathf.Pow(TrunkShrinkingFactor, i);
            var nextRingThickness = TrunkStartThickness * Mathf.Pow(TrunkShrinkingFactor, i + 1);

            var nextRingBasePosition = accumulatedBasePosition + ringBasePositions[i] +
                                       direction * intervalVariances[i];

            float twist = 20f * i / (float)TrunkLength;

            float nextTwist = 20f * (i + 1) / (float) TrunkLength;

            for (int shapeIndex = 0; shapeIndex < trunkShapeVertices.Length; shapeIndex++)
            {
                var nextShapeIndex = (shapeIndex + 1) % trunkShapeVertices.Length;
                // Get from the shape, apply thickness and extrude along direction
                // Bottom left
                var vertexPos1 = trunkShapeVertices[shapeIndex];
                vertexPos1 = Quaternion.AngleAxis(twist, Vector3.up) * vertexPos1;
                vertexPos1 += accumulatedBasePosition;
                vertexPos1 *= ringThickness * trunkThickness[i][shapeIndex];
                vertexPos1 += direction * i * TrunkInterval;

                // Top left
                var vertexPos2 = trunkShapeVertices[shapeIndex];
                vertexPos2 = Quaternion.AngleAxis(nextTwist, Vector3.up) * vertexPos2;
                vertexPos2 += nextRingBasePosition;
                vertexPos2 *= nextRingThickness * trunkThickness[i + 1][shapeIndex];
                vertexPos2 += direction * (i + 1) * TrunkInterval;

                // Bottom right
                var vertexPos3 = trunkShapeVertices[nextShapeIndex];
                vertexPos3 = Quaternion.AngleAxis(twist, Vector3.up) * vertexPos3;
                vertexPos3 += accumulatedBasePosition;
                vertexPos3 *= ringThickness * trunkThickness[i][nextShapeIndex];
                vertexPos3 += direction * i * TrunkInterval;

                // Top right
                var vertexPos4 = trunkShapeVertices[nextShapeIndex];
                vertexPos4 = Quaternion.AngleAxis(nextTwist, Vector3.up) * vertexPos4;
                vertexPos4 += nextRingBasePosition;
                vertexPos4 *= nextRingThickness * trunkThickness[i + 1][nextShapeIndex];
                vertexPos4 += direction * (i + 1) * TrunkInterval;

                vertices.Add(vertexPos1);
                vertices.Add(vertexPos2);
                vertices.Add(vertexPos3);
                vertices.Add(vertexPos4);

                // Create the first triangle A B C, in clockwise order
                triangles.Add(vertices.Count - 1 - 3);
                triangles.Add(vertices.Count - 1 - 1);
                triangles.Add(vertices.Count - 1 - 2);

                // Create the second triangle, B D C, in clockwise order
                triangles.Add(vertices.Count - 1 - 2);
                triangles.Add(vertices.Count - 1 - 1);
                triangles.Add(vertices.Count - 1);
            }
            accumulatedBasePosition += ringBasePositions[i] + direction * intervalVariances[i];
        }

        treeMesh.vertices = vertices.ToArray();
        treeMesh.triangles = triangles.ToArray();
        treeMesh.RecalculateBounds();
        treeMesh.RecalculateNormals();
    }

    private void GenerateBranch(Vector3 origin, Vector3 direction)
    {
        
    }
}
