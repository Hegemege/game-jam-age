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

    public float TrunkStartThickness;
    public float TrunkShrinkingFactor;

    // Randomness
    public float RandomRingThicknessVariance;
    public float RandomVertexThicknessVariance;
    private List<List<float>> trunkThickness;

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
                var vertexThickness = Random.Range(-RandomVertexThicknessVariance, RandomVertexThicknessVariance);
                trunkThickness[i].Add(ringThickness + vertexThickness);
            }
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

        for (int i = 0; i < TrunkLength - 1; i++)
        {
            for (int shapeIndex = 0; shapeIndex < trunkShapeVertices.Length; shapeIndex++)
            {
                var nextShapeIndex = (shapeIndex + 1) % trunkShapeVertices.Length;
                var ringThickness = TrunkStartThickness * Mathf.Pow(TrunkShrinkingFactor, i);
                var nextRingThickness = TrunkStartThickness * Mathf.Pow(TrunkShrinkingFactor, i+1);

                // Get from the shape, apply thickness and extrude along direction
                // Bottom left
                var vertexPos1 = trunkShapeVertices[shapeIndex];
                vertexPos1 *= ringThickness * trunkThickness[i][shapeIndex];
                vertexPos1 += direction * i * TrunkInterval;

                // Top left
                var vertexPos2 = trunkShapeVertices[shapeIndex];
                vertexPos2 *= nextRingThickness * trunkThickness[i + 1][shapeIndex];
                vertexPos2 += direction * (i + 1) * TrunkInterval;

                // Bottom right
                var vertexPos3 = trunkShapeVertices[nextShapeIndex];
                vertexPos3 *= ringThickness * trunkThickness[i][nextShapeIndex];
                vertexPos3 += direction * i * TrunkInterval;

                // Top right
                var vertexPos4 = trunkShapeVertices[nextShapeIndex];
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
