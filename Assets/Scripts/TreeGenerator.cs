using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeGenerator : MonoBehaviour
{
    public GameObject BranchPrefab;
    public GameObject CanopyPrefab;

    public float Scale;

    public float size;
    public float leaves;

    public Vector3[] trunkShapeVertices;

    public int MaxDepth;

    // Number of branches generated for every depth of hierarchy. Random value between the low and high is used.
    public int[] BranchesPerDepthLow;
    public int[] BranchesPerDepthHigh;

    // Positions for new branches along any branch at the given depth
    public float[] BranchingFromStemLow;
    public float[] BranchingFromStemHigh;

    public int TrunkLength;
    public float TrunkInterval;
    public float IntervalVariance;
    public float BranchLengthFalloff;
    public float BranchIntervalFalloff;
    public float BranchThicknessFalloff;

    public float TrunkStartThickness;
    public float TrunkShrinkingFactor;

    // Randomness
    public float RandomRingThicknessVariance;
    public float RandomVertexThicknessVariance;
    public float RandomLengthVariance;

    public float RingBaseDrift;

    private List<List<float>> trunkThickness;
    private List<float> intervalVariances;
    private List<Vector3> ringBasePositions;

    private List<GameObject> branches;
    private List<GameObject> canopies;

    private Random.State initialRandomState;

    void Awake()
    {
        // Initialize and store the random seed
        initialRandomState = Random.state;

        branches = new List<GameObject>();
        canopies = new List<GameObject>();

        RegenerateTree(false, true, false);
        ExpandCanopies(true);
    }

    void Start() 
    {
        initialRandomState = Random.state;
        RegenerateTree(false, true, true);
    }
    
    void Update() 
    {
        if (Input.GetKeyDown("r"))
        {
            RegenerateTree(true, false, true);
        }
    }

    /// <summary>
    /// newSize is relative to a base value (1f). newLeaves is the amount of leaves (0f to 1f).
    /// </summary>
    /// <param name="newSize"></param>
    /// <param name="newLeaves"></param>
    public void SeasonalGrowth(float newSize, float newLeaves, Season nextSeason)
    {
        //newSize = Mathf.Log(1 + size + newSize);

        size = newSize;
        RegenerateTree(false, false, true);

        // If leaves have changed, shrink or expand the canopies
        if (nextSeason != Season.Winter)
        {
            if (newLeaves >= leaves)
            {
                leaves = Mathf.Clamp(newLeaves, 0f, 1f);
                // Expand canopies
                ExpandCanopies(false);
            }
            else
            {
                leaves = Mathf.Clamp(newLeaves, 0f, 1f);
                // Shrink canopies - shrink everything and make first X disappear
                ShrinkCanopies();
            }
        }
        else
        {
            leaves = 0f;
            ShrinkCanopies();
        }

        // If both have increased, grow a new branch if possible
        // TODO
    }

    private void ExpandCanopies(bool all)
    {
        Debug.Log("Expand canopies");
        Debug.Log(leaves);
        // Force set the canopy size for the first X canopies
        var highVanishIndex = (int)((canopies.Count - 1) * leaves);

        if (all) highVanishIndex = canopies.Count - 1;

        for (var i = 0; i <= highVanishIndex; i++)
        {
            var canopy = canopies[i];

            canopy.GetComponent<Canopy>().SetLeaves(leaves, false);
        }
    }

    private void ShrinkCanopies()
    {
        Debug.Log("Shrink canopies");
        // Diminish last X canopies based on 1/leaves
        var highVanishIndex = (int)((canopies.Count - 1) * leaves);

        Debug.Log(highVanishIndex + " " + leaves);

        for (var i = 0; i < canopies.Count; i++)
        {
            var canopy = canopies[i];

            var vanish = i >= highVanishIndex;

            canopy.GetComponent<Canopy>().SetLeaves(leaves, vanish);
        }
    }

    private void RegenerateTree(bool canopyAnimation, bool instant, bool updateOld)
    {
        //size = Random.Range(0.5f, 1.5f);

        transform.localScale = Vector3.one;

        // Destroy all children
        /*
        foreach (var child in GetComponentsInChildren<Transform>())
        {
            if (child.gameObject != gameObject)
            {
                Destroy(child.gameObject);
            }
        }
        */

        Random.state = initialRandomState;

        // Generate initial tree params for mesh generation
        NewTreeParameters(TrunkLength);

        //branches = new List<GameObject>();
        //canopies = new List<GameObject>();

        if (updateOld)
        {
            // Update all branches
            for (var i = 0; i < branches.Count; i++)
            {
                var branch = branches[i];

                GenerateBranchMesh(branch, instant);

                branch.GetComponent<Branch>().SetNewBranchPosition(instant);
            }

            // Update all canopies
            for (var i = 0; i < canopies.Count; i++)
            {
                var canopy = canopies[i];

                GenerateCanopyMesh(canopy, instant);

                // Move canopies
                canopy.GetComponent<Canopy>().SetNewCanopyPosition(instant);
            }
        }
        else
        {
            GenerateTree(canopyAnimation, instant);
        }

        transform.localScale = Vector3.one * Scale * 2f * (Screen.height / 1080f);
    }

    private void NewTreeParameters(int length)
    {
        // Initialize the trunk thickness
        trunkThickness = new List<List<float>>();
        for (var i = 0; i < length; i++)
        {
            trunkThickness.Add(new List<float>());
        }

        // Generate randomness into the trunk's thickness
        for (var i = 0; i < length; i++)
        {
            var ringThickness = 1 + Random.Range(-RandomRingThicknessVariance, RandomRingThicknessVariance);

            for (var j = 0; j < trunkShapeVertices.Length; j++)
            {
                var vertexThickness = Random.Range(0f, RandomVertexThicknessVariance);

                vertexThickness *= 1 - i / (float)length;

                trunkThickness[i].Add((ringThickness + vertexThickness) * Mathf.Sqrt(size));
            }
        }

        // Generate random trunk intervals
        intervalVariances = new List<float>();
        for (var i = 0; i < length; i++)
        {
            intervalVariances.Add(TrunkInterval + Random.Range(-IntervalVariance, IntervalVariance));
        }

        // Base position drift
        ringBasePositions = new List<Vector3>();
        Vector3 basePosition = Vector3.zero;
        for (var i = 0; i < length; i++)
        {
            ringBasePositions.Add(basePosition);

            var accX = Random.Range(-RingBaseDrift, RingBaseDrift) * size;
            var accZ = Random.Range(-RingBaseDrift, RingBaseDrift) * size;

            accX *= 1 - i / (float)length;
            accZ *= 1 - i / (float)length;

            basePosition.x += accX;
            basePosition.z += accZ;
        }
    }

    private void GenerateTree(bool canopyAnimation, bool instant)
    {
        var trunk = GenerateBranch(gameObject, transform.position, Vector3.up, 0, TrunkStartThickness, TrunkInterval, TrunkLength, instant, -1);

        GenerateTreeRecursive(trunk, instant);

        GenerateCanopy(canopyAnimation, instant);
    }

    private void GenerateTreeRecursive(GameObject parent, bool instant)
    {
        Branch branch = parent.GetComponent<Branch>();

        // Do not generate more branches
        if (branch.Depth == MaxDepth)
        {
            return;
        }

        // Go through all branch positions and generate new branch there
        for (var i = 0; i < branch.BranchPositions.Count; i++)
        {
            var position = branch.BranchPositions[i];
            var direction = branch.BranchDirections[i];
            var thickness = branch.BranchThicknesses[i] * BranchThicknessFalloff;
            var depth = branch.Depth + 1;
            var interval = branch.BranchInterval * BranchIntervalFalloff;
            var length = (int)(branch.BranchLength * 
                BranchLengthFalloff * 
                (1 + Random.Range(-RandomLengthVariance, 0)));

            NewTreeParameters(length);

            var newBranch = GenerateBranch(parent, position, direction, depth, thickness, interval, length, instant, i);
            
            GenerateTreeRecursive(newBranch, instant);
        }
    }

    private void GenerateCanopy(bool canopyAnimation, bool instant)
    {
        for (var i = 0; i < branches.Count; i++)
        {
            var branch = branches[i];

            var canopyOrigin = branch.GetComponent<Branch>().CanopyPosition;

            var canopy = Instantiate(CanopyPrefab);
            canopy.transform.parent = branch.transform;
            canopy.transform.localPosition = canopyOrigin;

            // Scale the object
            var branchThickness = branch.GetComponent<Branch>().InitialThickness;


            canopy.transform.rotation = Random.rotation;

            // Generate the canopy mesh
            GenerateCanopyMesh(canopy, instant);

            // Give scaling instructions
            canopy.GetComponent<Canopy>().SetScale(Vector3.one * Mathf.Log(Random.Range(10f, 30f) * branchThickness * size));
            canopy.GetComponent<Canopy>().SkipExpansion = !canopyAnimation;

            canopies.Add(canopy);
        }
    }

    private void GenerateCanopyMesh(GameObject owner, bool instant)
    {
        // Generate an icosahedron, then form it
        var canopyMesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Generate vertices
        var t = (1f + Mathf.Sqrt(5f)) / 2f;
        vertices.Add(new Vector3(-1, t, 0));
        vertices.Add(new Vector3(1, t, 0));
        vertices.Add(new Vector3(-1, -t, 0));
        vertices.Add(new Vector3(1, -t, 0));

        vertices.Add(new Vector3(0, -1, t));
        vertices.Add(new Vector3(0, 1, t));
        vertices.Add(new Vector3(0, -1, -t));
        vertices.Add(new Vector3(0, 1, -t));

        vertices.Add(new Vector3(t, 0, -1));
        vertices.Add(new Vector3(t, 0, 1));
        vertices.Add(new Vector3(-t, 0, -1));
        vertices.Add(new Vector3(-t, 0, 1));

        // Generate triangles
        triangles.AddRange(new []{0, 11, 5});

        // 5 faces around point 0
        triangles.AddRange(new []{ 0, 11, 5 });
        triangles.AddRange(new []{ 0, 5, 1 });
        triangles.AddRange(new []{ 0, 1, 7 });
        triangles.AddRange(new []{ 0, 7, 10 });
        triangles.AddRange(new []{ 0, 10, 11 });

        // 5 adjacent faces
        triangles.AddRange(new[] { 1, 5, 9 });
        triangles.AddRange(new[] { 5, 11, 4 });
        triangles.AddRange(new[] { 11, 10, 2 });
        triangles.AddRange(new[] { 10, 7, 6 });
        triangles.AddRange(new[] { 7, 1, 8 });

        // 5 faces around point 3
        triangles.AddRange(new[] { 3, 9, 4 });
        triangles.AddRange(new[] { 3, 4, 2 });
        triangles.AddRange(new[] { 3, 2, 6 });
        triangles.AddRange(new[] { 3, 6, 8 });
        triangles.AddRange(new[] { 3, 8, 9 });

        // 5 adjacent faces
        triangles.AddRange(new[] { 4, 9, 5 });
        triangles.AddRange(new[] { 2, 4, 11 });
        triangles.AddRange(new[] { 6, 2, 10 });
        triangles.AddRange(new[] { 8, 6, 7 });
        triangles.AddRange(new[] { 9, 8, 1 });

        // Deform the vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Random.Range(0f, 1f) < 2f)
            {
                var randomDistance = Random.Range(1f, 1.5f);
                vertices[i] *= randomDistance;
            }
        }

        // Split vertices - makes a new vertex for every triangle
        // From http://answers.unity3d.com/questions/798510/flat-shading.html
        Vector3[] oldVertices = vertices.ToArray();
        int[] newTriangles = triangles.ToArray();
        Vector3[] newVertices = new Vector3[newTriangles.Length];

        for (int i = 0; i < newTriangles.Length; i++)
        {
            newVertices[i] = oldVertices[newTriangles[i]];
            newTriangles[i] = i;
        }

        // Apply verts and tris to the mesh, and apply the mesh to the owner's meshfilter
        canopyMesh.vertices = newVertices;
        canopyMesh.triangles = newTriangles;

        canopyMesh.RecalculateBounds();
        canopyMesh.RecalculateNormals();

        owner.GetComponent<Canopy>().SetTargetMesh(canopyMesh, instant);
        owner.GetComponent<Canopy>().OwnerBranch = owner.transform.parent.GetComponent<Branch>();
    }

    private GameObject GenerateBranch(GameObject parent, Vector3 origin, Vector3 direction, int depth, float thickness, float interval, float length, bool instant, int branchIndex)
    {
        var branch = Instantiate(BranchPrefab);
        branch.transform.parent = parent.transform;
        branch.transform.localPosition = origin;
        branch.transform.localRotation = Quaternion.LookRotation(Vector3.forward, direction);

        branch.GetComponent<Branch>().Depth = depth;
        branch.GetComponent<Branch>().InitialThickness = thickness;
        branch.GetComponent<Branch>().BranchLength = length;
        branch.GetComponent<Branch>().BranchInterval = interval;
        branch.GetComponent<Branch>().ParentBranchIndex = branchIndex;

        if (branchIndex >= 0)
        {
            branch.GetComponent<Branch>().ParentBranch = parent.GetComponent<Branch>();
        }

        GenerateBranchMesh(branch, instant);

        branches.Add(branch);

        return branch;
    }

    private void GenerateBranchMesh(GameObject owner, bool instant)
    {
        // Get parameters from the owner branch object
        var depth = owner.GetComponent<Branch>().Depth;
        var initialThickness = owner.GetComponent<Branch>().InitialThickness;
        var subBranches = Random.Range(BranchesPerDepthHigh[depth], BranchesPerDepthHigh[depth]); // HACK: high not used because of random state breaking

        var branchLength = owner.GetComponent<Branch>().BranchLength;
        var interval = owner.GetComponent<Branch>().BranchInterval;

        // Clear, initialize, assign mesh
        var treeMesh = new Mesh();

        // Use lists because we dont know the final size, convert to arrays when assigning to the mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Generate the vertices and triangles
        Vector3 direction = Vector3.up;

        Vector3 accumulatedBasePosition = Vector3.zero;

        List<Vector3> childPositions = new List<Vector3>();
        List<float> childThicknesses = new List<float>();
        List<Vector3> childDirections = new List<Vector3>();

        // Figure out the ring indices for which we generate a point for child branches
        var childRingIndices = new List<int>();
        for (int i = 0; i < subBranches; i++)
        {
            float index = Random.Range(BranchingFromStemLow[depth], BranchingFromStemHigh[depth]);
            childRingIndices.Add((int) Mathf.Floor(branchLength * index));
        }

        childRingIndices.Sort();

        var canopyPosition = Vector3.zero;

        // Generate the geometry
        for (int i = 0; i < branchLength - 1; i++)
        {
            var ringThickness = initialThickness * Mathf.Pow(TrunkShrinkingFactor, i);
            var nextRingThickness = initialThickness * Mathf.Pow(TrunkShrinkingFactor, i + 1);

            var ringPosition = ringBasePositions[i] * depth;

            var nextRingBasePosition = accumulatedBasePosition + ringPosition +
                                       direction * intervalVariances[i];

            float twist = 20f * i / (float)branchLength;

            float nextTwist = 20f * (i + 1) / (float)branchLength;

            // Sub branch generation
            var branchesToGenerate = childRingIndices.Count(item => item == i);

            branchesToGenerate = Mathf.Min(2, branchesToGenerate);

            List<int> branchVertexIndices = new List<int>();
            List<Vector3> branchVertexPositions = new List<Vector3>();
            List<Vector3> branchVertexDirections = new List<Vector3>();
            List<float> branchVertexThicknesses = new List<float>();

            for (var j = 0; j < branchesToGenerate; j++)
            {
                // Take random points on the ring, unique
                var randomIndex = Random.Range(0, trunkShapeVertices.Length);
                while (branchVertexIndices.Contains(randomIndex))
                {
                    randomIndex = Random.Range(0, trunkShapeVertices.Length);
                }

                branchVertexIndices.Add(randomIndex);
            }

            for (int shapeIndex = 0; shapeIndex < trunkShapeVertices.Length; shapeIndex++)
            {
                var nextShapeIndex = (shapeIndex + 1) % trunkShapeVertices.Length;
                // Get from the shape, apply thickness and extrude along direction
                // Bottom left
                var vertexPos1 = trunkShapeVertices[shapeIndex];
                vertexPos1 = Quaternion.AngleAxis(twist, Vector3.up) * vertexPos1;
                vertexPos1 *= ringThickness * trunkThickness[i][shapeIndex];
                vertexPos1 += accumulatedBasePosition;
                vertexPos1 += direction * i * interval;

                canopyPosition = vertexPos1;

                // Top left
                var vertexPos2 = trunkShapeVertices[shapeIndex];
                vertexPos2 = Quaternion.AngleAxis(nextTwist, Vector3.up) * vertexPos2;
                vertexPos2 *= nextRingThickness * trunkThickness[i + 1][shapeIndex];
                vertexPos2 += nextRingBasePosition;
                vertexPos2 += direction * (i + 1) * interval;

                // Bottom right
                var vertexPos3 = trunkShapeVertices[nextShapeIndex];
                vertexPos3 = Quaternion.AngleAxis(twist, Vector3.up) * vertexPos3;
                vertexPos3 *= ringThickness * trunkThickness[i][nextShapeIndex];
                vertexPos3 += accumulatedBasePosition;
                vertexPos3 += direction * i * interval;

                // Top right
                var vertexPos4 = trunkShapeVertices[nextShapeIndex];
                vertexPos4 = Quaternion.AngleAxis(nextTwist, Vector3.up) * vertexPos4;
                vertexPos4 *= nextRingThickness * trunkThickness[i + 1][nextShapeIndex];
                vertexPos4 += nextRingBasePosition;
                vertexPos4 += direction * (i + 1) * interval;

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

                // sub branch position / normal
                for (int branchIndex = 0; branchIndex < branchesToGenerate; branchIndex++)
                {
                    if (shapeIndex == branchVertexIndices[branchIndex])
                    {
                        // Places any new branches at the center of the parent branch
                        branchVertexPositions.Add(accumulatedBasePosition + direction * i * interval);

                        // Save direction to grow the tree in. Not sharply downwards, or directly along the old direction
                        var newDirection = direction + Random.onUnitSphere;
                        while (Vector3.Dot(newDirection.normalized, Vector3.up) < -0.2f &&
                            Vector3.Dot(newDirection.normalized, direction) > 0.2f)
                        {
                            newDirection = direction + Random.onUnitSphere;
                        }
                        branchVertexDirections.Add(newDirection.normalized);

                        // Store thickness at current point
                        branchVertexThicknesses.Add(ringThickness * trunkThickness[i][shapeIndex]);
                    }
                }

            }
            accumulatedBasePosition += ringPosition + direction * intervalVariances[i];

            // Generate sub branches, amount based on matching index
            childPositions.AddRange(branchVertexPositions);
            childDirections.AddRange(branchVertexDirections);
            childThicknesses.AddRange(branchVertexThicknesses);
        }

        treeMesh.vertices = vertices.ToArray();
        treeMesh.triangles = triangles.ToArray();
        treeMesh.RecalculateBounds();
        treeMesh.RecalculateNormals();

        owner.GetComponent<Branch>().SetTargetMesh(treeMesh, instant);

        owner.GetComponent<Branch>().BranchPositions = childPositions;
        owner.GetComponent<Branch>().BranchDirections = childDirections;
        owner.GetComponent<Branch>().BranchThicknesses = childThicknesses;
        owner.GetComponent<Branch>().CanopyPosition = canopyPosition;
    }
}
