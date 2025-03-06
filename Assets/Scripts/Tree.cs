using System;
using System.Collections.Generic;
using UnityEngine;

public class Tree
{
    // Public properties

    // Branch structure

    // Stores the positions each segment in the trunk starts from.
    public List<Vector3> SegmentPositions { get; private set; }
    
    // Stores the vectors for each segment in the trunk; each vector is meant to start at the end of the last segment and point
    // to the base of the next segment.
    public List<Vector3> SegmentVectors { get; private set; }

    // Stores the radii for the bases of each segment. The end radius of one segment is the same as the base radius for the next
    // segment, except for the last segment, which doesn't have a "next segment".
    // This is why there needs to be one extra entry to store the last end radius.
    public List<float> SegmentRadii { get; private set; }

    // Holds all child branch objects.
    public List<Tree> Branches { get; private set; }


    // Leaves

    // Stores the positions for the base of each leaf
    public Vector3 LeafBasePosition { get; private set; }

    // Stores the direction each leaf is pointing
    public List<Vector3> LeafDirections { get; private set; }

    // Stores the Bezier curve points used as the blueprint for each leaf
    // First point should always be Vector3.zero to serve as the base of the leaf so it appears to start to the end of the branch
    public List<Vector3> LeafBezierPoints { get; private set; }


    // Customizable functions

    // Branch structure
    private Func<int, float> GetTerminateRadius;
    private Func<int, float> GetSegmentLength;
    private Func<int, float> GetSegmentRadius;
    private Func<int, float> GetSegmentDivergenceAngle;
    private Func<int, float> GetSegmentRotationAngle;
    private Func<int, bool> ShouldStartNewBranch;
    private Func<int, float> GetBranchDivergenceAngle;
    private Func<int, float> GetBranchRotationAngle;
    private Func<int, int> GetBranchSegmentNumber;

    // Leaf spawning
    private Func<int, float> GetLeafDivergenceAngle;
    private Func<int, float> GetLeafRotationAngle;
    private Func<int, int> GetLeavesToSpawn;



    // Private variables

    // The number the current branch segment is in the list of all segments generated so far in the tree; starts at 0.
    private int segmentNumber;

    // The orthogonal vector used to create diverging segments and branches
    private Vector3 orthogonalVector;

    // The threshold the branch's radius must reach before it stops growing
    private readonly float terminateRadius;


    // Constructors

    public Tree(Vector3 startPosition,
                Vector3 startDirection = default,
                Func<int, float> TerminateRadiusFunc = null,
                Func<int, float> SegmentLengthFunc = null,
                Func<int, float> SegmentRadiusFunc = null,
                Func<int, float> SegmentDivergenceFunc = null,
                Func<int, float> SegmentRotationFunc = null,
                Func<int, bool> ShouldBranchFunc = null,
                Func<int, float> BranchDivergenceFunc = null,
                Func<int, float> BranchRotationFunc = null,
                Func<int, int> SegmentNumberFunc = null,
                Func<int, float> LeafDivergenceFunc = null,
                Func<int, float> LeafRotationFunc = null,
                Func<int, int> LeafSpawnFunc = null,
                List<Vector3> leafBezierPoints = null)
                : this(startPosition,
                       startDirection,
                       TerminateRadiusFunc,
                       SegmentLengthFunc,
                       SegmentRadiusFunc,
                       SegmentDivergenceFunc,
                       SegmentRotationFunc,
                       ShouldBranchFunc,
                       BranchDivergenceFunc,
                       BranchRotationFunc,
                       SegmentNumberFunc,
                       LeafDivergenceFunc,
                       LeafRotationFunc,
                       LeafSpawnFunc,
                       leafBezierPoints,
                       0,
                       default) { }

    private Tree(Vector3 startPosition,
                 Vector3 startDirection,
                 Func<int, float> TerminateRadiusFunc,
                 Func<int, float> SegmentLengthFunc,
                 Func<int, float> SegmentRadiusFunc,
                 Func<int, float> SegmentDivergenceFunc,
                 Func<int, float> SegmentRotationFunc,
                 Func<int, bool> ShouldBranchFunc,
                 Func<int, float> BranchDivergenceFunc,
                 Func<int, float> BranchRotationFunc,
                 Func<int, int> SegmentNumberFunc,
                 Func<int, float> LeafDivergenceFunc,
                 Func<int, float> LeafRotationFunc,
                 Func<int, int> LeafSpawnFunc,
                 List<Vector3> leafBezierPoints,
                 int segmentNumber,
                 Vector3 orthogonalVector)
    {
        // Check for default startDirection
        if (startDirection == default) startDirection = Vector3.up;
        if (orthogonalVector == default)
        {
            orthogonalVector = Vector3.Cross(startDirection, Vector3.right).normalized;
            if (orthogonalVector.magnitude == 0) orthogonalVector = Vector3.forward;
        }

        // Initialize segment arrays

        // Branch structure
        SegmentPositions = new();
        SegmentVectors = new();
        SegmentRadii = new();
        Branches = new();

        // Leaf spawning
        LeafDirections = new();
        LeafBezierPoints = leafBezierPoints;


        // Initialize customizeable functions

        // Branch structure
        GetTerminateRadius = TerminateRadiusFunc;
        GetSegmentLength = SegmentLengthFunc;
        GetSegmentRadius = SegmentRadiusFunc;
        GetSegmentDivergenceAngle = SegmentDivergenceFunc;
        GetSegmentRotationAngle = SegmentRotationFunc;
        ShouldStartNewBranch = ShouldBranchFunc;
        GetBranchDivergenceAngle = BranchDivergenceFunc;
        GetBranchRotationAngle = BranchRotationFunc;
        GetBranchSegmentNumber = SegmentNumberFunc;

        // Leaf spawning
        GetLeafDivergenceAngle = LeafDivergenceFunc;
        GetLeafRotationAngle = LeafRotationFunc;
        GetLeavesToSpawn = LeafSpawnFunc;

        CheckDefaultFuncs();

        // Initialize private variables
        this.segmentNumber = segmentNumber;
        this.orthogonalVector = orthogonalVector;
        terminateRadius = GetTerminateRadius.Invoke(segmentNumber);


        // Start tree

        SegmentPositions.Add(startPosition);
        SegmentVectors.Add(startDirection.normalized * GetSegmentLength.Invoke(segmentNumber));
        SegmentRadii.Add(GetSegmentRadius.Invoke(segmentNumber));


        Grow();
    }


    // Methods

    private void CheckDefaultFuncs()
    {
        // Branch structure
        GetTerminateRadius ??= (segmentNumber) => 0.01f;
        GetSegmentLength ??= (segmentNumber) => Mathf.Clamp((-1f / 30) * segmentNumber + 1f, 0.1f, Mathf.Infinity);
        GetSegmentRadius ??= (segmentNumber) => Mathf.Clamp((-1f / 20) * segmentNumber + 1f, terminateRadius, Mathf.Infinity);
        GetSegmentDivergenceAngle ??= (segmentNumber) => UnityEngine.Random.value * 10f;
        GetSegmentRotationAngle ??= (segmentNumber) => UnityEngine.Random.value * 360f;
        ShouldStartNewBranch ??= (segmentNumber) => segmentNumber > 3 && UnityEngine.Random.value <= 0.5f;
        GetBranchDivergenceAngle ??= (segmentNumber) => 33f;
        GetBranchRotationAngle ??= (segmentNumber) => UnityEngine.Random.value * 360;
        GetBranchSegmentNumber ??= (segmentNumber) => segmentNumber + 1;

        // Leaf spawning
        GetLeafDivergenceAngle ??= (segmentNumber) => UnityEngine.Random.value * 270;
        GetLeafRotationAngle ??= (segmentNumber) => UnityEngine.Random.value * 360f;
        GetLeavesToSpawn ??= (segmentNumber) => UnityEngine.Random.Range(3, 7);
        LeafBezierPoints ??= new List<Vector3> { Vector3.zero, new Vector3(0.08f, 0.12f, 0.25f), new Vector3(0.4f, 0, 0.2f) };
    }

    private void Grow()
    {
        // Increment segment
        segmentNumber++;

        // Add segment to current branch

        SegmentPositions.Add(SegmentPositions[^1] + SegmentVectors[^1]);
        Vector3 newVector = Diverge(SegmentVectors[^1], GetSegmentDivergenceAngle.Invoke(segmentNumber),
                                    GetSegmentRotationAngle.Invoke(segmentNumber));
        SegmentVectors.Add(newVector.normalized * GetSegmentLength.Invoke(segmentNumber));

        float segmentRadius = GetSegmentRadius.Invoke(segmentNumber);
        SegmentRadii.Add(segmentRadius);


        if (segmentRadius > terminateRadius)
        {
            TrySplit();
            Grow();
        }
        else
        {
            SegmentRadii.Add(segmentRadius);
            SpawnLeaves();
        }
    }

    // Calculates new vector for diverging branch
    private Vector3 Diverge(Vector3 originalVector, float divergenceAngle, float rotationAngle)
    {
        originalVector.Normalize();

        // Diverge originalVector
        Vector3 newVector = VectorRotation.RotateVector(orthogonalVector, originalVector, divergenceAngle);
        newVector = VectorRotation.RotateVector(originalVector, newVector, rotationAngle);

        // Rotate orthogonalVector to stay aligned with rotated segment
        orthogonalVector = VectorRotation.RotateVector(originalVector, orthogonalVector, rotationAngle);

        return newVector;
    }

    // Possibly starts new branch based on chance
    private void TrySplit()
    {
        // Only start a new branch if the dice land right
        if (!ShouldStartNewBranch.Invoke(segmentNumber)) return;

        // Start the new branch at the base of the current segment
        Vector3 startPosition = SegmentPositions[^1];

        // Save a copy of orthogonalVector before calculating the branch's direction so that it can be restored after
        Vector3 orthogonalVectorTemp = orthogonalVector;
        Vector3 startDirection = Diverge(SegmentVectors[^1], GetBranchDivergenceAngle.Invoke(segmentNumber),
                                         GetBranchRotationAngle.Invoke(segmentNumber));

        // Instantiate the branch as a new Tree
        Tree newBranch = new(startPosition,
                             startDirection,
                             GetTerminateRadius,
                             GetSegmentLength,
                             GetSegmentRadius,
                             GetSegmentDivergenceAngle,
                             GetSegmentRotationAngle,
                             ShouldStartNewBranch,
                             GetBranchDivergenceAngle,
                             GetBranchRotationAngle,
                             GetBranchSegmentNumber,
                             GetLeafDivergenceAngle,
                             GetLeafRotationAngle,
                             GetLeavesToSpawn,
                             LeafBezierPoints,
                             GetBranchSegmentNumber.Invoke(segmentNumber),
                             orthogonalVector);
        Branches.Add(newBranch);

        // Restore orthogonalVector to its original state
        orthogonalVector = orthogonalVectorTemp;
    }

    // Spawn leaves at the end of the current branch
    private void SpawnLeaves()
    {
        LeafBasePosition = SegmentPositions[^1] + SegmentVectors[^1];

        int leafNum = GetLeavesToSpawn(segmentNumber);

        for (int i = 0; i < leafNum; i++) LeafDirections.Add(Diverge(SegmentVectors[^1], GetLeafDivergenceAngle(segmentNumber),
                                                                     GetLeafRotationAngle(segmentNumber)));
    }
}