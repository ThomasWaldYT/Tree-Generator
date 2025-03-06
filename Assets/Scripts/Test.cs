using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    /*
    // Tunable function curves

    // Branch structure
    [SerializeField] private AnimationCurve terminateRadiusCurve;
    [SerializeField] private AnimationCurve segmentLengthCurve;
    [SerializeField] private AnimationCurve segmentRadiusCurve;
    [SerializeField] private AnimationCurve segmentDivergenceCurve;
    [SerializeField] private AnimationCurve segmentRotationCurve;
    [SerializeField] private AnimationCurve shouldBranchCurve;
    [SerializeField] private AnimationCurve branchDivergenceCurve;
    [SerializeField] private AnimationCurve branchRotationCurve;
    [SerializeField] private AnimationCurve segmentNumberCurve;

    // Leaf spawning
    [SerializeField] private AnimationCurve leafDivergenceCurve;
    [SerializeField] private AnimationCurve leafRotationCurve;
    [SerializeField] private AnimationCurve leafSpawnCurve;
    [SerializeField] private List<Vector3> leafBezierPoints;
    [SerializeField] private int leafSpawnMinimum;
    */


    [SerializeField] private int randomSeed;
    [SerializeField] private int leafResolution;


    [SerializeField] private Color leafColor;
    [SerializeField] private Color treeColor;
    [SerializeField] private Color groundColor;


    private delegate void TreeFunction(Vector3 position = default);


    [SerializeField] private float branchSpringConstant = 1f;
    [SerializeField] private float branchDampingCoefficient = 0.5f;
    [SerializeField] private float leafSpringConstant = 5f;
    [SerializeField] private float leafDampingCoefficient = 0.1f;
    [SerializeField] private float impulseMagnitude = 3f;

    [SerializeField] private float branchBendAcceleration;
    [SerializeField] private float branchBendVelocity;
    [SerializeField] private float branchBend;

    [SerializeField] private float leafBendAcceleration;
    [SerializeField] private float leafBendVelocity;
    [SerializeField] private float leafBend;

    [SerializeField] private List<Vector3> leafPoints;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //DrawGround(20);
        StartCoroutine(DrawTruffulaBall(20));
    }

    // Update is called once per frame
    void Update()
    {
        /*TreeFunction treeFunction = DrawSugarMaple;
        DrawSwayingTree(treeFunction);*/

        /*Tree testTree = new(Vector3.zero,
                            Vector3.up,
                            (segmentNumber) => 0.75f,
                            (segmentNumber) => 5,
                            (segmentNumber) => Mathf.Clamp((-1f / 40) * segmentNumber + 0.8f, 0.01f, Mathf.Infinity),
                            (segmentNumber) => branchBend,
                            (segmentNumber) => segmentNumber == 1 ? rotation : 0,
                            (segmentNumber) => false,
                            (segmentNumber) => 0,
                            (segmentNumber) => 0,
                            (segmentNumber) => segmentNumber + 1,
                            (segmentNumber) => 0,
                            (segmentNumber) => 0,
                            (segmentNumber) => 0);
        StartCoroutine(VisualizeTree(testTree, 0, 0.0001f));*/

        //DrawLeaf(Vector3.zero, Vector3.right, leafPoints, 0, 0.0001f, false, leafResolution, leafColor);
    }

    private IEnumerator DrawTruffulaBall(int truffulaNum)
    {
        for (int i = 0; i < truffulaNum; i++)
        {
            DrawTruffula();
            yield return new WaitForSeconds(1);
        }
    }

    private void DrawSwayingTree(TreeFunction treeFunction)
    {
        Random.InitState(Time.frameCount);
        float randomImpulse = Random.Range(-impulseMagnitude, impulseMagnitude);

        branchBendAcceleration = -branchSpringConstant * branchBend - branchDampingCoefficient * branchBendVelocity + randomImpulse;
        branchBendVelocity += branchBendAcceleration * Time.deltaTime;
        branchBend += branchBendVelocity * Time.deltaTime;

        leafBendAcceleration = -leafSpringConstant * leafBend - leafDampingCoefficient * leafBendVelocity + randomImpulse;
        leafBendVelocity += leafBendAcceleration * Time.deltaTime;
        leafBend += leafBendVelocity * Time.deltaTime;


        Random.InitState(randomSeed);
        treeFunction();
    }

    private IEnumerator VisualizeBezier(List<Vector3> points, float drawDuration, float lifetime, bool shouldWait,
                                        bool shouldDrawBezierPoints, int resolution, Color color)
    {
        float increment = 1f / Mathf.Max(resolution, 1);
        Vector3 currentPoint = points[0];
        Vector3 nextPoint;

        if (shouldDrawBezierPoints) foreach (Vector3 point in points) Visualizer.DrawNGon(point, 4, 0.1f, 0, 0.0001f, Color.green);

        for (float t = 0; t < 1; t += increment)
        {
            nextPoint = Bezier(points, t + increment);
            Visualizer.DrawLine(currentPoint, nextPoint, drawDuration, lifetime, 0.1f, color);
            currentPoint = nextPoint;

            if (shouldWait) yield return null;
        }
    }

    private Vector3 Bezier(List<Vector3> points, float t)
    {
        Vector3 sum = new();
        int n = points.Count - 1;

        for (int i = 0; i <= n; i++)
        {
            float binomialCoefficient = Factorial(n) / (Factorial(i) * Factorial(n - i));
            sum += binomialCoefficient * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i) * points[i];
        }

        return sum;
    }

    private int Factorial(int x)
    {
        return x <= 1 ? 1 : x * Factorial(x - 1); ;
    }

    private IEnumerator VisualizeQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float drawDuration, float lifetime, bool shouldWait,
                                                 bool shouldDrawBezierPoints, int resolution, Color color)
    {
        float increment = 1f / Mathf.Max(resolution, 1);
        Vector3 currentPoint = p0;
        Vector3 nextPoint;

        if (shouldDrawBezierPoints)
        {
            Visualizer.DrawNGon(p0, 4, 0.1f, 0, 0.0001f, Color.green);
            Visualizer.DrawNGon(p1, 4, 0.1f, 0, 0.0001f, Color.green);
            Visualizer.DrawNGon(p2, 4, 0.1f, 0, 0.0001f, Color.green);
        }

        for (float t = 0; t < 1; t += increment)
        {
            nextPoint = QuadraticBezier(p0, p1, p2, t + increment);
            Visualizer.DrawLine(currentPoint, nextPoint, drawDuration, lifetime, 0, color);
            currentPoint = nextPoint;

            if (shouldWait) yield return null;
        }
    }

    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return (1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2);
    }

    private IEnumerator VisualizeLinearBezier(Vector3 p0, Vector3 p1, float drawDuration, float lifetime, bool shouldWait, bool shouldDrawBezierPoints,
                                       int resolution, Color color)
    {
        float increment = 1f / Mathf.Max(resolution, 1);
        Vector3 currentPoint = p0;
        Vector3 nextPoint;

        if (shouldDrawBezierPoints)
        {
            Visualizer.DrawNGon(p0, 4, 0.1f, 0, 0.0001f, Color.green);
            Visualizer.DrawNGon(p1, 4, 0.1f, 0, 0.0001f, Color.green);
        }

        for (float t = 0; t < 1; t += increment)
        {
            nextPoint = LinearBezier(p0, p1, t + increment);
            Visualizer.DrawLine(currentPoint, nextPoint, drawDuration, lifetime, 0, color);
            currentPoint = nextPoint;

            if (shouldWait) yield return null;
        }
    }

    private Vector3 LinearBezier(Vector3 p0, Vector3 p1, float t)
    {
        return p0 + t * (p1 - p0);
    }


    private IEnumerator Intro()
    {
        for (int i = 0; i < 15; i++)
        {
            Visualizer.Draw2DGrid(new Vector3(-10, 0, i * 15), Vector3.right, Vector3.forward, 40, 30, 0.5f, groundColor, 1, 2);

            Tree tree1 = new(new Vector3(-8f, 0, i * 15 + 7.5f), Vector3.up);
            Tree tree2 = new(new Vector3(8f, 0, i * 15 + 7.5f), Vector3.up);

            VisualizeTree(tree1, 1, 2, false);
            VisualizeTree(tree2, 1, 2, false);

            yield return new WaitForSeconds(1);
        }

        DrawTreeOfLife(new Vector3(0, -100, 600));
    }

    private IEnumerator ShowcaseTrees()
    {
        TreeFunction treeFunction;

        for (int i = 0; i < 2; i++)
        {
            DrawAngelOak(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawAngelOak;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 2; i++)
        {
            DrawPine(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawPine;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);


        for (int i = 0; i < 2; i++)
        {
            DrawSugarMaple(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawSugarMaple;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 2; i++)
        {
            DrawBaobab(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawBaobab;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 2; i++)
        {
            DrawJoshua(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawJoshua;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 2; i++)
        {
            DrawAcacia(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawAcacia;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 2; i++)
        {
            DrawKapok(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawKapok;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 2; i++)
        {
            DrawMangrove(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawMangrove;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 2; i++)
        {
            DrawTentacles(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawTentacles;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 2; i++)
        {
            DrawSpiral(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawSpiral;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 2; i++)
        {
            DrawSnake(Vector3.zero);
            yield return new WaitForSeconds(7);
        }
        treeFunction = DrawSnake;
        DrawSmallForest(treeFunction);
        yield return new WaitForSeconds(7);

        DrawTreeOfLife(Vector3.zero);
    }

    private void DrawSmallForest(TreeFunction drawTree)
    {
        int positionModifier = 7;

        drawTree(Vector3.zero);
        drawTree(new Vector3(positionModifier, 0, positionModifier));
        drawTree(new Vector3(-positionModifier, 0, positionModifier));
        drawTree(new Vector3(positionModifier, 0, -positionModifier));
        drawTree(new Vector3(-positionModifier, 0, -positionModifier));
    }

    private void DrawTruffula(Vector3 position = default)
    {
        List<Vector3> truffulaLeafPoints = new() { Vector3.zero,
                                                new( 2.89f,  0.31f,  1.65f),
                                                new( 2.83f,  0.00f, -1.83f) };
        Tree truffula = new(position,
                            Random.onUnitSphere,
                            (segmentNumber) => 0.2f,
                            (segmentNumber) => Mathf.Clamp((-1f / 75) * segmentNumber + 1f, 0.2f, Mathf.Infinity),
                            (segmentNumber) => Mathf.Clamp((-1f / 75) * segmentNumber + 0.5f, 0.2f, Mathf.Infinity),
                            (segmentNumber) => Random.value * 5,
                            (segmentNumber) => Random.value * 360,
                            (segmentNumber) => false,
                            (segmentNumber) => 0,
                            (segmentNumber) => 0,
                            (segmentNumber) => 0,
                            (segmentNumber) => Random.value * 360,
                            (segmentNumber) => Random.value * 360,
                            (segmentNumber) => 500,
                            truffulaLeafPoints);

        Color truffulaLeafColor = Color.HSVToRGB(303 / 360f, 0.75f, 0.85f);

        // Must hack the DrawLeaf function to always point downwards in order to get the willow leaves to droop
        VisualizeTree(truffula, 3, Mathf.Infinity, true, 3, treeColor, truffulaLeafColor);
    }

    private void DrawTreeOfLife(Vector3 position = default)
    {
        float multiplier = 1;

        List<Vector3> treeOfLifeLeafPoints = new() { Vector3.zero,
                                                new(0.3f, 0.4f, 0.15f),
                                                new(0.9f, 0f, 0f) };

        Tree tree = new(new Vector3(position.x, position.y + 2, position.z),
                        Vector3.up,
                        (segmentNumber) => 0.01f * multiplier,
                        (segmentNumber) => Mathf.Clamp(((-1f / 55) * segmentNumber + 1f) * multiplier, 0.1f * multiplier, Mathf.Infinity),
                        (segmentNumber) => Mathf.Clamp(((-1f / 45) * segmentNumber + 1f) * multiplier, 0.01f * multiplier, Mathf.Infinity),
                        (segmentNumber) =>
                        {
                            if (segmentNumber < 8) return 0;
                            else return UnityEngine.Random.value * 25;
                        },
                        (segmentNumber) =>
                        {
                            if (segmentNumber < 8) return 0;
                            else return UnityEngine.Random.value * 360;
                        },
                        (segmentNumber) =>
                        {
                            if (segmentNumber <= 4) return false;
                            else if (segmentNumber > 4 && segmentNumber < 8) return true;
                            else return UnityEngine.Random.value < 0.33f + segmentNumber / 40;
                        },
                        (segmentNumber) => UnityEngine.Random.value * 45 + 20,
                        (segmentNumber) =>
                        {
                            if (segmentNumber < 8) return segmentNumber * 120;
                            else return UnityEngine.Random.value * 360;
                        },
                        (segmentNumber) => segmentNumber + 3,
                        (segmentNumber) => UnityEngine.Random.value * 100 + 10,
                        (segmentNumber) => UnityEngine.Random.value * 360,
                        (segmentNumber) => UnityEngine.Random.Range(3, 4),
                        treeOfLifeLeafPoints);

        Tree roots = new(new Vector3(position.x, position.y + 2, position.z),
                         Vector3.down,
                         (segmentNumber) => 0.01f * multiplier,
                         (segmentNumber) => Mathf.Clamp(((-1f / 55) * segmentNumber + 1f) * multiplier, 0.1f * multiplier, Mathf.Infinity),
                         (segmentNumber) => Mathf.Clamp(((-1f / 25) * segmentNumber + 1f) * multiplier, 0.01f * multiplier, Mathf.Infinity),
                         (segmentNumber) =>
                         {
                             if (segmentNumber < 5) return 0;
                             else return UnityEngine.Random.value * 25;
                         },
                         (segmentNumber) => UnityEngine.Random.value * 360,
                         (segmentNumber) =>
                         {
                             if (segmentNumber == 0) return false;
                             else if (segmentNumber > 0 && segmentNumber < 5) return true;
                             else return UnityEngine.Random.value < 0.4f;
                         },
                         (segmentNumber) => UnityEngine.Random.value * 45 + 30,
                         (segmentNumber) =>
                         {
                             if (segmentNumber < 5) return segmentNumber * 90;
                             else return UnityEngine.Random.value * 360;
                         },
                         (segmentNumber) => segmentNumber + 3,
                         (segmentNumber) => 0,
                         (segmentNumber) => 0,
                         (segmentNumber) => 0,
                         new List<Vector3> { Vector3.zero });

        Color treeOfLifeLeafColor = Color.HSVToRGB(123 / 360f, 0.58f, 0.69f);

        VisualizeTree(tree, 3, Mathf.Infinity, true, 5, treeColor, treeOfLifeLeafColor);
        VisualizeTree(roots, 3, Mathf.Infinity, true, 5, treeColor, treeOfLifeLeafColor);
    }

    private void DrawSnake(Vector3 position = default)
    {
        Tree snake = new(position,
                         Vector3.up,
                         (segmentNumber) => 0.01f,
                         (segmentNumber) => Mathf.Clamp((-1f / 30) * segmentNumber + 1.5f, 0.1f, Mathf.Infinity),
                         (segmentNumber) => Mathf.Clamp((-1f / 30) * segmentNumber + 0.75f, 0.01f, Mathf.Infinity),
                         (segmentNumber) =>
                         {
                             if (segmentNumber < 3) return -25;
                             else return Mathf.Sin(segmentNumber / 2) * 45;
                         },
                         (segmentNumber) => 0,
                         (segmentNumber) => segmentNumber > 3 && UnityEngine.Random.value <= 0.2f,
                         (segmentNumber) => UnityEngine.Random.value * 50 + 30,
                         (segmentNumber) => UnityEngine.Random.value * 360,
                         (segmentNumber) => segmentNumber + 1);

        VisualizeTree(snake, 3, 5, true);
    }

    private void DrawSpiral(Vector3 position = default)
    {
        List<Vector3> spiralLeafPoints = new() { Vector3.zero,
                                                 new(0.24f, 0.35f, -0.25f),
                                                 new(1.34f, 0.30f, 0.12f),
                                                 new(1.75f, 0.30f, 0.28f),
                                                 new(1.71f, 0.25f, 0.63f),
                                                 new(1.62f, 0.25f, 1.01f),
                                                 new(1.18f, 0.20f, 0.83f),
                                                 new(0.93f, 0.20f, 0.68f),
                                                 new(1.21f, 0.15f, 0.40f),
                                                 new(1.43f, 0.15f, 0.23f),
                                                 new(1.61f, 0.10f, 0.55f),
                                                 new(1.77f, 0.10f, 0.83f),
                                                 new(1.31f, 0.05f, 0.81f),
                                                 new(1.16f, 0.05f, 0.81f),
                                                 new(1.18f, 0.00f, 0.57f) };

        Tree spiral = new(position,
                          Vector3.up,
                          (segmentNumber) => 0.01f,
                          (segmentNumber) => Mathf.Clamp((-1f / 15) * segmentNumber + 1.5f, 0.1f, Mathf.Infinity),
                          (segmentNumber) => Mathf.Clamp((-1f / 30) * segmentNumber + 0.75f, 0.01f, Mathf.Infinity),
                          (segmentNumber) =>
                          {
                              if (segmentNumber < 10) return -10 + segmentNumber;
                              else return 20 + segmentNumber;
                          },
                          (segmentNumber) => 0,
                          (segmentNumber) => segmentNumber > 3 && segmentNumber < 20 && UnityEngine.Random.value <= 0.4f,
                          (segmentNumber) => UnityEngine.Random.value * 50 + 30,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => segmentNumber + 1,
                          (segmentNumber) => UnityEngine.Random.value * 90 + 10,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => UnityEngine.Random.Range(1, 3),
                          spiralLeafPoints);

        Color spiralLeafColor = Color.HSVToRGB(306 / 360f, 0.82f, 0.83f);

        VisualizeTree(spiral, 3, Mathf.Infinity, true, 5, treeColor, spiralLeafColor);
    }

    private void DrawTentacles(Vector3 position = default)
    {
        Tree tentacles = new(position,
                             Vector3.up,
                             (segmentNumber) => 0.01f,
                             (segmentNumber) => Mathf.Clamp((-1f / 50) * segmentNumber + 1f, 0.1f, Mathf.Infinity),
                             (segmentNumber) => Mathf.Clamp((-1f / 50) * segmentNumber + 1f, 0.01f, Mathf.Infinity),
                             (segmentNumber) => Mathf.Sin(segmentNumber / 2f) * 45,
                             (segmentNumber) => UnityEngine.Random.value * 360,
                             (segmentNumber) => UnityEngine.Random.value <= 0.1f,
                             (segmentNumber) => UnityEngine.Random.value * 30 + segmentNumber,
                             (segmentNumber) => UnityEngine.Random.value * 360,
                             (segmentNumber) => segmentNumber + 1);

        VisualizeTree(tentacles, 3, 5, true);
    }

    private void DrawWillow(Vector3 position = default)
    {
        List<Vector3> willowLeafPoints = new() { Vector3.zero,
                                                new( 0.06f, -0.14f,  0.00f),
                                                new( 0.47f, -0.30f,  0.00f),
                                                new( 0.07f,  0.01f,  0.00f),
                                                new( 0.05f,  0.00f,  0.00f),
                                                new( 0.10f,  0.01f,  0.00f),
                                                new( 0.20f,  0.00f,  0.00f),
                                                new( 0.35f,  0.18f,  0.00f),
                                                new( 0.68f,  0.30f,  0.00f),
                                                new( 0.45f,  0.11f,  0.00f),
                                                new( 0.28f,  0.00f,  0.00f),
                                                new( 0.30f,  0.01f,  0.00f),
                                                new( 0.46f,  0.00f,  0.00f),
                                                new( 0.58f,  0.16f,  0.00f),
                                                new( 0.89f,  0.29f,  0.00f),
                                                new( 0.62f,  0.04f,  0.00f),
                                                new( 0.54f,  0.00f,  0.00f),
                                                new( 0.58f,  0.01f,  0.00f),
                                                new( 0.73f,  0.00f,  0.00f),
                                                new( 0.82f,  0.17f,  0.00f),
                                                new( 1.20f,  0.32f,  0.00f),
                                                new( 0.94f,  0.10f,  0.00f),
                                                new( 0.81f,  0.00f,  0.00f),
                                                new( 0.85f,  0.01f,  0.00f),
                                                new( 1.03f,  0.00f,  0.00f),
                                                new( 1.18f,  0.20f,  0.00f),
                                                new( 1.54f,  0.32f,  0.00f),
                                                new( 1.31f,  0.11f,  0.00f),
                                                new( 1.13f,  0.00f,  0.00f),
                                                new( 1.17f,  0.01f,  0.00f),
                                                new( 1.35f,  0.00f,  0.00f),
                                                new( 1.45f,  0.16f,  0.00f),
                                                new( 1.81f,  0.33f,  0.00f),
                                                new( 1.47f,  0.01f,  0.00f),
                                                new( 1.44f,  0.00f,  0.00f),
                                                new( 1.52f, -0.01f,  0.00f),
                                                new( 1.68f,  0.00f,  0.00f),
                                                new( 1.81f,  0.17f,  0.00f),
                                                new( 2.14f,  0.30f,  0.00f),
                                                new( 1.83f,  0.01f,  0.00f),
                                                new( 1.77f,  0.00f,  0.00f),
                                                new( 1.99f,  0.05f,  0.00f),
                                                new( 2.43f,  0.00f,  0.00f) };
        Tree willow = new(position,
                          Vector3.up,
                          (segmentNumber) => 0.01f,
                          (segmentNumber) => Mathf.Clamp((-1f / 100) * segmentNumber + 1f, 0.2f, Mathf.Infinity),
                          (segmentNumber) => Mathf.Clamp((-1f / 50) * segmentNumber + 1f, 0.01f, Mathf.Infinity),
                          (segmentNumber) => UnityEngine.Random.value * 15,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => segmentNumber > 3 && Random.value < 0.2f +
                                             (1f / 400) * segmentNumber,
                          (segmentNumber) => UnityEngine.Random.value * 40 + 10,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => segmentNumber + 3,
                          (segmentNumber) => 0,
                          (segmentNumber) => 0,
                          (segmentNumber) => 1,
                          willowLeafPoints);

        Color willowLeafColor = Color.HSVToRGB(138 / 360f, 0.75f, 0.85f);

        // Must hack the DrawLeaf function to always point downwards in order to get the willow leaves to droop
        VisualizeTree(willow, 3, Mathf.Infinity, true, 3, treeColor, willowLeafColor);
    }

    private void DrawMangrove(Vector3 position = default)
    {
        List<Vector3> mangroveLeafPoints = new() { Vector3.zero,
                                                new(0.6f, 0.2f, 0.15f),
                                                new(0.9f, 0f, 0f) };

        Tree mangroveTree = new(new Vector3(position.x, position.y + 9, position.z),
                                Vector3.up,
                                (segmentNumber) => 0.05f,
                                (segmentNumber) => Mathf.Clamp((-1f / 45) * segmentNumber + 1f, 0.1f, Mathf.Infinity),
                                (segmentNumber) => Mathf.Clamp((-1f / 45) * segmentNumber + 0.5f, 0.05f, Mathf.Infinity),
                                (segmentNumber) => UnityEngine.Random.value * 15,
                                (segmentNumber) => UnityEngine.Random.value * 360,
                                (segmentNumber) => segmentNumber > 2 && UnityEngine.Random.value <= 0.4f,
                                (segmentNumber) => UnityEngine.Random.value * 50 + 30,
                                (segmentNumber) => UnityEngine.Random.value * 360,
                                (segmentNumber) => segmentNumber + 2,
                                (segmentNumber) => UnityEngine.Random.value * 90 + 15,
                                (segmentNumber) => UnityEngine.Random.value * 360,
                                (segmentNumber) => UnityEngine.Random.Range(3, 6),
                                mangroveLeafPoints);

        Tree mangroveRoots = new(new Vector3(position.x, position.y + 9, position.z),
                                 Vector3.down,
                                 (segmentNumber) => 0.2f,
                                 (segmentNumber) => Mathf.Clamp((1f / 35) * segmentNumber + 1f, 0.1f, Mathf.Infinity),
                                 (segmentNumber) => Mathf.Clamp((-1f / 45) * segmentNumber + 0.5f, 0.2f, Mathf.Infinity),
                                 (segmentNumber) => UnityEngine.Random.value * 20,
                                 (segmentNumber) => UnityEngine.Random.value * 360,
                                 (segmentNumber) => UnityEngine.Random.value <= 0.7f,
                                 (segmentNumber) => UnityEngine.Random.value * 33 + 5,
                                 (segmentNumber) => UnityEngine.Random.value * 360,
                                 (segmentNumber) => segmentNumber + 2,
                                 (segmentNumber) => 0,
                                 (segmentNumber) => 0,
                                 (segmentNumber) => 0,
                                 new List<Vector3> { Vector3.zero });

        Color mangroveLeafColor = Color.HSVToRGB(148 / 360f, 0.67f, 0.80f);

        VisualizeTree(mangroveTree, 3, Mathf.Infinity, true, 5, treeColor, mangroveLeafColor);
        VisualizeTree(mangroveRoots, 3, Mathf.Infinity, true, 5, treeColor);
    }

    private void DrawKapok(Vector3 position = default)
    {
        List<Vector3> kapokLeafPoints = new() { Vector3.zero,
                                                new(0.97f, -0.34f, 0.23f),
                                                new(1.3f, 0f, 0f) };

        Tree kapok = new(position,
                         Vector3.up,
                         (segmentNumber) => 0.01f,
                         (segmentNumber) =>
                         {
                             if (segmentNumber < 20) return 2;
                             else return Mathf.Clamp((-1f / 27) * (segmentNumber - 20) + 2f, 0.1f, Mathf.Infinity);
                         },
                         (segmentNumber) =>
                         {
                             if (segmentNumber < 4) return -segmentNumber + 5;
                             else if (segmentNumber >= 4 && segmentNumber < 20) return 1.5f;
                             else return Mathf.Clamp((-1f / 27) * (segmentNumber - 20) + 1.5f, 0.01f, Mathf.Infinity);
                         },
                         (segmentNumber) =>
                         {
                             if (segmentNumber < 20) return Random.value * 2;
                             else return Random.value * (segmentNumber - 20);
                         },
                         (segmentNumber) => UnityEngine.Random.value * 360,
                         (segmentNumber) => segmentNumber >= 20 && Random.value <= (0.12f + (segmentNumber - 20) / 200f),
                         (segmentNumber) => UnityEngine.Random.value * 45 + 15 - (segmentNumber - 20),
                         (segmentNumber) => UnityEngine.Random.value * 360,
                         (segmentNumber) => segmentNumber + 1,
                         (segmentNumber) => UnityEngine.Random.value * 100,
                         (segmentNumber) => UnityEngine.Random.value * 360,
                         (segmentNumber) => UnityEngine.Random.Range(5, 10),
                         kapokLeafPoints);

        Color kapokLeafColor = Color.HSVToRGB(127 / 360f, 0.80f, 0.84f);

        VisualizeTree(kapok, 3, Mathf.Infinity, true, 5, treeColor, kapokLeafColor);
    }

    private void DrawAcacia(Vector3 position = default)
    {
        List<Vector3> acaciaLeafPoints = new() { Vector3.zero,
                                                 new(0.075f, 0.15f, 0.15f),
                                                 new(0.05f, 0f, 0.015f),
                                                 new(0.125f, 0.2f, 0.21f),
                                                 new(0.1f, 0f, 0.025f),
                                                 new(0.175f, 0.225f, 0.26f),
                                                 new(0.15f, 0f, 0.03f),
                                                 new(0.225f, 0.25f, 0.29f),
                                                 new(0.2f, 0f, 0.035f),
                                                 new(0.275f, 0.275f, 0.31f),
                                                 new(0.25f, 0f, 0.04f),
                                                 new(0.325f, 0.3f, 0.33f),
                                                 new(0.3f, 0f, 0.045f),
                                                 new(0.375f, 0.325f, 0.35f),
                                                 new(0.35f, 0f, 0.05f),
                                                 new(0.425f, 0.325f, 0.36f),
                                                 new(0.4f, 0f, 0.05f),
                                                 new(0.475f, 0.3f, 0.32f),
                                                 new(0.45f, 0f, 0.04f),
                                                 new(0.525f, 0.275f, 0.3f),
                                                 new(0.5f, 0f, 0f) };

        Tree acacia = new(position,
                          Vector3.up,
                          (segmentNumber) => 0.01f,
                          (segmentNumber) => Mathf.Clamp((-1f / 18) * segmentNumber + 1.5f, 0.1f, Mathf.Infinity),
                          (segmentNumber) => Mathf.Clamp((-1f / 25) * segmentNumber + 0.8f, 0.01f, Mathf.Infinity),
                          (segmentNumber) => UnityEngine.Random.value * segmentNumber * 2,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => segmentNumber > 1 && UnityEngine.Random.value <= (0.33f + segmentNumber / 60f),
                          (segmentNumber) => UnityEngine.Random.value * 33 + 25 - segmentNumber,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) =>
                          {
                              if (segmentNumber < 15) return segmentNumber;
                              else return segmentNumber + 1;
                          },
                          (segmentNumber) => UnityEngine.Random.value * 100,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => UnityEngine.Random.Range(5, 10),
                          acaciaLeafPoints);

        Color acaciaLeafColor = Color.HSVToRGB(121 / 360f, 0.53f, 0.40f);

        VisualizeTree(acacia, 3, Mathf.Infinity, true, 5, treeColor, acaciaLeafColor);
    }

    private void DrawJoshua(Vector3 position = default)
    {
        List<Vector3> joshuaNeedlePoints = new() { Vector3.zero,
                                                new(-0.11f, 0.06f, 0.05f),
                                                new(1.5f, 0f, 0f) };

        Tree joshua = new(position,
                          Vector3.up,
                          (segmentNumber) => 0.3f,
                          (segmentNumber) => Mathf.Clamp((-1f / 35) * segmentNumber + 2f, 0.5f, Mathf.Infinity),
                          (segmentNumber) => Mathf.Clamp((-1f / 25) * segmentNumber + 0.65f, 0.3f, Mathf.Infinity),
                          (segmentNumber) => UnityEngine.Random.value * 40,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => segmentNumber > 2 && UnityEngine.Random.value <= 0.75f,
                          (segmentNumber) => UnityEngine.Random.value * 35 + 45,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => segmentNumber + 1,
                          (segmentNumber) => UnityEngine.Random.value * 150,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => UnityEngine.Random.Range(50, 70),
                          joshuaNeedlePoints);

        Color joshuaNeedleColor = Color.HSVToRGB(75 / 360f, 0.83f, 0.52f);

        VisualizeTree(joshua, 3, Mathf.Infinity, true, 3, treeColor, joshuaNeedleColor);
    }

    private void DrawBaobab(Vector3 position = default)
    {
        List<Vector3> baobabLeafPoints = new() { Vector3.zero,
                                                 new(0.34f, 0.23f, 0.17f),
                                                 new(0.5f, 0f, 0f) };

        Tree baobab = new(position,
                          Vector3.up,
                          (segmentNumber) =>
                          {
                              if (segmentNumber < 11) return 1.73f;
                              else return 0.01f;
                          },
                          (segmentNumber) =>
                          {
                              if (segmentNumber < 7) return 1.5f;
                              else if (segmentNumber < 11) return 0.1f;
                              else return 2 / Mathf.Sqrt(segmentNumber + 1);
                          },
                          (segmentNumber) => Mathf.Clamp((-1f / 35) * segmentNumber + 2f, 0.01f, Mathf.Infinity),
                          (segmentNumber) =>
                          {
                              if (segmentNumber < 11) return UnityEngine.Random.value * 1;
                              else return UnityEngine.Random.value * 15;
                          },
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) =>
                          {
                              if (segmentNumber < 7) return false;
                              else if (segmentNumber < 11) return true;
                              else return UnityEngine.Random.value <= 1 / 15000f * Mathf.Pow(segmentNumber, 2);
                          },
                          (segmentNumber) => UnityEngine.Random.value * 50 + 30,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) =>
                          {
                              if (segmentNumber < 11) return segmentNumber + 25;
                              else
                              {
                                  return segmentNumber + 1;
                              }
                          },
                          (segmentNumber) => UnityEngine.Random.value * 110,
                          (segmentNumber) => UnityEngine.Random.value * 360,
                          (segmentNumber) => UnityEngine.Random.Range(5, 10),
                          baobabLeafPoints);

        Color baobabLeafColor = Color.HSVToRGB(92 / 360f, 0.88f, 0.67f);

        VisualizeTree(baobab, 3, Mathf.Infinity, true, 5, treeColor, baobabLeafColor);
    }

    private void DrawSugarMaple(Vector3 position = default)
    {
        List<Vector3> mapleLeafPoints = new() { Vector3.zero,
                                                new(-0.19f, 0.27f, 0.05f),
                                                new(0f, 0.72f, 0.11f),
                                                new(-0.02f, 0.62f, 0.1f),
                                                new(0.08f, 0.47f, 0.08f),
                                                new(0.29f, 0.72f, 0.07f),
                                                new(0.42f, 0.76f, 0.11f),
                                                new(0.26f, 0.54f, 0f),
                                                new(0.84f, 0.79f, 0.07f),
                                                new(0.27f, -0.11f, 0.04f),
                                                new(1.1f, 0.35f, 0.04f),
                                                new(1.07f, 0.11f, 0f),
                                                new(1.28f, 0.17f, 0.06f),
                                                new(1.22f, 0.03f, 0),
                                                new(1.54f, 0f, -0.07f) };
        Tree sugarMaple = new(position,
                              Vector3.up,
                              (segmentNumber) => 0.01f,
                              (segmentNumber) => Mathf.Clamp((-1f / 40) * segmentNumber + 1f, 0.2f, Mathf.Infinity),
                              (segmentNumber) => Mathf.Clamp((-1f / 40) * segmentNumber + 0.8f, 0.01f, Mathf.Infinity),
                              (segmentNumber) => UnityEngine.Random.value * 10 + branchBend,
                              (segmentNumber) => UnityEngine.Random.value * 360,
                              (segmentNumber) =>
                              {
                                  if (segmentNumber > 4 && segmentNumber < 10) return true;
                                  else if (segmentNumber >= 10) return UnityEngine.Random.value <= Mathf.Clamp(
                                                                       (1f / 100) * segmentNumber + 0.15f, 0, 1);
                                  else return false;
                              },
                              (segmentNumber) => UnityEngine.Random.value * 40 + 40,
                              (segmentNumber) => UnityEngine.Random.value * 360,
                              (segmentNumber) => segmentNumber + 2,
                              (segmentNumber) => UnityEngine.Random.value * 100 + leafBend,
                              (segmentNumber) => UnityEngine.Random.value * 360,
                              (segmentNumber) => UnityEngine.Random.Range(3, 4),
                              mapleLeafPoints);

        Color mapleLeafColor = Color.HSVToRGB(0, 0.88f, 1f);

        VisualizeTree(sugarMaple, 3, Mathf.Infinity, true, 5, treeColor, mapleLeafColor);
    }

    private void DrawPine(Vector3 position = default)
    {
        List<Vector3> pineNeedlePoints = new() { Vector3.zero,
                                                new(-0.07f, 0.01f, 0f),
                                                new(0.3f, 0f, 0f) };

        Tree pine = new(position,
                        Vector3.up,
                        (segmentNumber) => 0.01f,
                        (segmentNumber) => Mathf.Clamp((-1f / 55) * segmentNumber + 0.8f, 0.1f, Mathf.Infinity),
                        (segmentNumber) => Mathf.Clamp((-1f / 55) * segmentNumber + 0.7f, 0.01f, Mathf.Infinity),
                        (segmentNumber) => UnityEngine.Random.value * segmentNumber / 4f,
                        (segmentNumber) => UnityEngine.Random.value * 360,
                        (segmentNumber) => 
                        {
                            if (segmentNumber > 5) return Random.value <= Mathf.Clamp((1f / 100) * segmentNumber + 0.7f, 0, 1);
                            else return false;
                        },
                        (segmentNumber) => UnityEngine.Random.value * 50 + 60,
                        (segmentNumber) => UnityEngine.Random.value * 360,
                        (segmentNumber) => segmentNumber + 5,
                        (segmentNumber) => UnityEngine.Random.value * 88 + 22,
                        (segmentNumber) => UnityEngine.Random.value * 360,
                        (segmentNumber) => UnityEngine.Random.Range(8, 15),
                        pineNeedlePoints);

        Color pineNeedleColor = Color.HSVToRGB(136 / 360f, 0.69f, 0.76f);

        VisualizeTree(pine, 3, Mathf.Infinity, true, 2, treeColor, pineNeedleColor);
    }

    private void DrawAngelOak(Vector3 position = default)
    {
        List<Vector3> angelOakLeafPoints = new() { Vector3.zero,
                                                   new(0.58f, 0.21f, 0.28f),
                                                   new(0.69f, 0f, 0) };

        Tree angelOak = new(position,
                            Vector3.up,
                            (segmentNumber) => 0.01f,
                            (segmentNumber) => Mathf.Clamp((-1f / 70) * segmentNumber + 1.5f, 0.2f, Mathf.Infinity),
                            (segmentNumber) => Mathf.Clamp((-1f / 40) * segmentNumber + 2f, 0.01f, Mathf.Infinity),
                            (segmentNumber) => segmentNumber > 3 ? Random.value * 33 : Random.value * 5,
                            (segmentNumber) => UnityEngine.Random.value * 360,
                            (segmentNumber) =>
                            {
                                if (segmentNumber > 3 && segmentNumber < 10) return Random.value < 0.5f;
                                else if (segmentNumber >= 10 && segmentNumber < 50) return Random.value < 0.05f;
                                else if (segmentNumber >= 50) return Random.value <= Mathf.Clamp((1f / 250) * segmentNumber + 0.05f, 0, 1);
                                else return false;
                            },
                            (segmentNumber) => UnityEngine.Random.value * 45 + 45,
                            (segmentNumber) => UnityEngine.Random.value * 360,
                            (segmentNumber) => segmentNumber + 3,
                            (segmentNumber) => UnityEngine.Random.value * 133,
                            (segmentNumber) => UnityEngine.Random.value * 360,
                            (segmentNumber) => UnityEngine.Random.Range(3, 5),
                            angelOakLeafPoints);

        Color angelOakLeafColor = Color.HSVToRGB(125/360f, 0.9f, 0.55f);

        VisualizeTree(angelOak, 3, Mathf.Infinity, true, 5, treeColor, angelOakLeafColor);
    }



    // Draws an entire tree
    private void VisualizeTree(Tree tree, float drawDuration = 1, float lifetime = Mathf.Infinity, bool shouldWait = false,
                               int leafResolution = 10, Color branchColor = default, Color leafColor = default)
    {
        if (branchColor == default) branchColor = Color.white;
        if (leafColor == default) leafColor = Color.white;

        List<Tree> branches = GetAllBranches(new List<Tree> { tree }, 0);

        foreach (Tree branch in branches)
        {
            StartCoroutine(DrawBranch(branch, drawDuration, lifetime, shouldWait, leafResolution, branchColor, leafColor));
        }
    }

    // Returns a list of all branches in a tree using breadth-first search
    private List<Tree> GetAllBranches(List<Tree> branches, int index)
    {
        int newBranches = 0;

        while (index < branches.Count)
        {
            foreach (Tree newBranch in branches[index].Branches)
            {
                branches.Add(newBranch);
                newBranches++;
            }

            index++;
        }

        if (newBranches == 0) return branches;
        else return GetAllBranches(branches, index);
    }

    // Draws all the segments of a single branch
    private IEnumerator DrawBranch(Tree branch, float drawDuration, float lifetime, bool shouldWait, int leafResolution,
                                   Color branchColor, Color leafColor)
    {
        float segmentDrawTime = drawDuration / (shouldWait ? branch.SegmentPositions.Count : 1);

        for (int i = 0; i < branch.SegmentPositions.Count; i++)
        {
            Vector3 startPosition = branch.SegmentPositions[i];
            Vector3 endPosition = startPosition + branch.SegmentVectors[i];
            Vector3 startNormal = i != 0 ? branch.SegmentVectors[i - 1] : branch.SegmentVectors[i];
            float startRadius = branch.SegmentRadii[i];
            float endRadius = branch.SegmentRadii[i + 1];
            Visualizer.DrawCylinder(startPosition, endPosition, startRadius, endRadius, 6, segmentDrawTime, lifetime,
                                    0.1f, startNormal, default, branchColor);
            if (shouldWait) yield return new WaitForSeconds(segmentDrawTime);
        }

        // Draw all leaves at the end of the branch
        foreach (Vector3 leafDirection in branch.LeafDirections)
        {
            DrawLeaf(branch.LeafBasePosition, leafDirection, branch.LeafBezierPoints, segmentDrawTime, lifetime, shouldWait,
                     leafResolution, leafColor);
            if (shouldWait) yield return null;
        }
    }

    // Draws a single leaf
    private void DrawLeaf(Vector3 basePosition, Vector3 direction, List<Vector3> points, float drawDuration, float lifetime,
                          bool shouldWait, int leafResolution, Color leafColor)
    {
        direction.Normalize();

        Vector3 orthogonalVector1 = Vector3.Cross(direction, Vector3.right).normalized;
        if (orthogonalVector1.magnitude == 0) orthogonalVector1 = Vector3.back;

        Vector3 orthogonalVector2 = Vector3.Cross(direction, orthogonalVector1).normalized;

        List<Vector3> topHalf = new();
        List<Vector3> bottomHalf = new();

        foreach (Vector3 point in points)
        {
            topHalf.Add(point.x * direction + point.y * orthogonalVector1 + point.z * orthogonalVector2 + basePosition);
            bottomHalf.Add(point.x * direction - point.y * orthogonalVector1 + point.z * orthogonalVector2 + basePosition);
        }

        for (int i = 1; i < topHalf.Count; i += 2)
        {
            if (i != topHalf.Count - 1)
            {
                StartCoroutine(VisualizeQuadraticBezier(topHalf[i - 1], topHalf[i], topHalf[i + 1], drawDuration, lifetime, shouldWait, false, leafResolution,
                                                        leafColor));
                StartCoroutine(VisualizeQuadraticBezier(bottomHalf[i - 1], bottomHalf[i], bottomHalf[i + 1], drawDuration, lifetime, shouldWait, false,
                                                        leafResolution, leafColor));
            }
            else
            {
                StartCoroutine(VisualizeLinearBezier(topHalf[i - 1], topHalf[i], drawDuration, lifetime, shouldWait, false, leafResolution, leafColor));
                StartCoroutine(VisualizeLinearBezier(bottomHalf[i - 1], bottomHalf[i], drawDuration, lifetime, shouldWait, false, leafResolution, leafColor));
            }
        }

    }

    private void DrawGround(int width)
    {
        Visualizer.Draw2DGrid(Vector3.zero, Vector3.right, Vector3.forward, width, width, 0.5f, groundColor, 1, Mathf.Infinity);
        Visualizer.Draw2DGrid(Vector3.zero, Vector3.right, Vector3.back, width, width, 0.5f, groundColor, 1, Mathf.Infinity);
        Visualizer.Draw2DGrid(Vector3.zero, Vector3.left, Vector3.forward, width, width, 0.5f, groundColor, 1, Mathf.Infinity);
        Visualizer.Draw2DGrid(Vector3.zero, Vector3.left, Vector3.back, width, width, 0.5f, groundColor, 1, Mathf.Infinity);
    }



    private void CylinderDrawingVisualizations()
    {
        Visualizer.DrawCylinder(Vector3.zero, new Vector3(5, 5, 0), 1, 0.5f, 36, 1, Mathf.Infinity, 0.02f,
                                    Vector3.up);
        Visualizer.DrawCylinder(Vector3.zero, new Vector3(0, 5, 5), 1, 0.5f, 36, 1, Mathf.Infinity, 0.02f,
                                Vector3.up);
        Visualizer.DrawCylinder(Vector3.zero, new Vector3(-5, 5, 0), 1, 0.5f, 36, 1, Mathf.Infinity, 0.02f,
                                Vector3.up);
        Visualizer.DrawCylinder(Vector3.zero, new Vector3(0, 5, -5), 1, 0.5f, 36, 1, Mathf.Infinity, 0.02f,
                                Vector3.up);

        Visualizer.DrawCylinder(Vector3.zero, new Vector3(5, -5, 0), 1, 0.5f, 36, 1, Mathf.Infinity, 0.02f,
                                Vector3.down);
        Visualizer.DrawCylinder(Vector3.zero, new Vector3(0, -5, 5), 1, 0.5f, 36, 1, Mathf.Infinity, 0.02f,
                                Vector3.down);
        Visualizer.DrawCylinder(Vector3.zero, new Vector3(-5, -5, 0), 1, 0.5f, 36, 1, Mathf.Infinity, 0.02f,
                                Vector3.down);
        Visualizer.DrawCylinder(Vector3.zero, new Vector3(0, -5, -5), 1, 0.5f, 36, 1, Mathf.Infinity, 0.02f,
                                Vector3.down);
    }
    
    private void VectorRotationVisualizations()
    {
        /*Visualizer.DrawArrow(Vector3.zero, new Vector3(1, 0, 0), 1, 100, 0.2f, Color.red);
            Visualizer.DrawArrow(Vector3.zero, new Vector3(0, 1, 0), 1, 100, 0.2f, Color.green);
            Visualizer.DrawArrow(Vector3.zero, new Vector3(0, 0, 1), 1, 100, 0.2f, Color.blue);*/

        /*Visualizer.DrawArrow(Vector3.zero, new Vector3(1, 0, 0), 1, 100, Color.red);
        Visualizer.DrawArrow(Vector3.zero, new Vector3(0, 1, 0), 1, 100, Color.green);
        Visualizer.DrawLine(Vector3.zero, new Vector3(0, 0, 100), 20, 20, 0.2f, Color.blue);*/

        /*List<Vector3> positions = new() { Vector3.zero, Vector3.left, Vector3.right, Vector3.up, Vector3.down };
        Visualizer.DrawLineContinuous(positions, 1, 100, 0.2f, Color.blue);*/

        //SpiralVisualizationY();

        /*SpiralVisualizationY();
        SpiralVisualizationX();*/

        //SpiralVisualizationXAndY();

        /*Vector3 stationaryVector = Vector3.one * 4;
        Vector3 rotatingVector = new(1, 2, 3);
        Visualizer.DrawArrow(Vector3.zero, stationaryVector, 1, 100, 0.2f, Color.black);
        Visualizer.DrawArrow(Vector3.zero, rotatingVector, 1, 100, 0.2f, Color.magenta);

        Vector3 cross1 = Vector3.Cross(stationaryVector, rotatingVector).normalized;
        Vector3 cross2 = Vector3.Cross(cross1, stationaryVector).normalized;
        Visualizer.DrawArrow(Vector3.zero, cross1, 1, 100, 0.2f, Color.white);
        Visualizer.DrawArrow(Vector3.zero, cross2, 1, 100, 0.2f, Color.white);

        Visualizer.Draw2DGrid(Vector3.zero, cross1, cross2, 10, 10, 1, 100, Color.white);
        Visualizer.Draw2DGrid(Vector3.zero, cross1, -cross2, 10, 10, 1, 100, Color.white);
        Visualizer.Draw2DGrid(Vector3.zero, -cross1, cross2, 10, 10, 1, 100, Color.white);
        Visualizer.Draw2DGrid(Vector3.zero, -cross1, -cross2, 10, 10, 1, 100, Color.white);

        Vector3 rotatedVector = VectorRotation.RotateVector(stationaryVector, rotatingVector, 45);
        Visualizer.DrawArrow(Vector3.zero, rotatedVector, 1, 100, 0.2f, Color.cyan);*/
    }

    private void SpiralVisualizationY()
    {
        float length = 0.1f;

        float angle = 0;

        List<Vector3> positions = new();
        positions.Add(Vector3.zero);

        while (length < 100)
        {
            float x = Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = Mathf.Sin(angle * Mathf.Deg2Rad);

            positions.Add(new Vector3(x, 0, z) * length);

            length += 0.01f;
            angle += 1f % 360;
        }

        //Visualizer.DrawLineContinuous(positions, 0.0001f, 100, 0.2f, Color.blue);
    }

    private void SpiralVisualizationX()
    {
        float length = 0.1f;

        float angle = 0;

        List<Vector3> positions = new();
        positions.Add(Vector3.zero);

        while (length < 100)
        {
            float y = Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = Mathf.Sin(angle * Mathf.Deg2Rad);

            positions.Add(new Vector3(0, y, z) * length);

            length += 0.01f;
            angle += 1f % 360;
        }

        //Visualizer.DrawLineContinuous(positions, 0.0001f, 100, 0.2f, Color.blue);
    }

    private void SpiralVisualizationXAndY()
    {
        float length = 0.1f;

        float angle = 0;

        List<Vector3> positions = new();
        positions.Add(Vector3.zero);

        while (length < 100)
        {
            float x = Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = Mathf.Sin(angle * Mathf.Deg2Rad);

            positions.Add(new Vector3(x, y, z) * length);

            length += 0.01f;
            angle += 1f % 360;
        }

        //Visualizer.DrawLineContinuous(positions, 0.0001f, 100, 0.2f, Color.blue);
    }
}
