using NUnit;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    /*
     * IMPORTANT!!!
     * This class is a singleton class.
     * Due to the way unity works, it needs to have one object in the scene with this script on it.
     * It spawns objects with trail renderers attached to them and assigns itself as the parent.
     * Don't try to use it without that singleton object!!!
    */

    [System.Serializable]
    struct LineData
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public float startTime;
        public float drawDuration;
        public float lifetime;
        public Vector4 color;
    }


    // Compute shader script used for exporting the line-drawing process to the GPU
    [SerializeField] private ComputeShader lineDrawComputeShader;

    // Material that inherits from a custom shader (LineDrawShader) to visualize lines
    [SerializeField] private Material lineDrawMaterial;

    // Maximum number of lines allowed in the scene at once; too many and the compute buffers will overflow
    [SerializeField] private int maxLines;


    // Singleton of this class
    private static Visualizer instance;

    // Used to send data about the lines to the GPU for updating drawing positions
    private ComputeBuffer linesDataBuffer;

    // Stores drawing positiong calculated by GPU for use by the shader
    private ComputeBuffer linePositionsBuffer;

    // Stores line colors for use by the shader
    private ComputeBuffer lineColorsBuffer;

    // Stores one LineData per line drawn
    private static LineData[] linesDataCPU;


    // How many lines are still active in the scene
    private static int linesActive = 0;



    private void Awake()
    {
        // Set the singleton instance to the current game object; should only be one in the scene!
        instance = this;

        // Allocate the CPU arrays
        linesDataCPU = new LineData[maxLines];


        // Allocate the GPU buffers

        // linesDataBuffer: one LineData per line
        int lineDataStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(LineData));
        linesDataBuffer = new ComputeBuffer(maxLines, lineDataStride, ComputeBufferType.Default);

        // linePositionsBuffer: 2 positions (float3) per line => maxLines * 2 float3 elements
        linePositionsBuffer = new ComputeBuffer(maxLines * 2, sizeof(float) * 3, ComputeBufferType.Default);

        // For each line, we have 2 vertices => 2 colors per line
        lineColorsBuffer = new ComputeBuffer(maxLines * 2, sizeof(float) * 4, ComputeBufferType.Default);
    }

    private void Update()
    {
        // Don't send data to GPU if no lines are currently drawing to save processing time
        if (linesActive == 0) return;

        SyncLineDataToGPU();
    }


    private void OnRenderObject()
    {
        // Don't send data to GPU if no lines are currently active to save processing time
        if (linesActive == 0) return;

        // Send active line data to GPU for rendering
        lineDrawMaterial.SetBuffer("linePositions", linePositionsBuffer);
        lineDrawMaterial.SetBuffer("lineColors", lineColorsBuffer);
        lineDrawMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Lines, linesActive * 2);


        // Remove inactive lines and sync line data to the GPU if any lines were removed
        bool removedLines = false;
        int linesActiveTemp = linesActive;
        for (int i = 0; i < linesActiveTemp; i++)
        {
            if (Time.time - linesDataCPU[i].startTime < linesDataCPU[i].lifetime) continue;

            linesDataCPU[i] = linesDataCPU[linesActiveTemp - 1];
            linesDataCPU[linesActiveTemp - 1] = new();

            removedLines = true;
            i--;
            linesActiveTemp--;
        }
        if (removedLines)
        {
            SyncLineDataToGPU();
            linesActive = linesActiveTemp;
        }
    }

    private void OnDestroy()
    {
        if (linesDataBuffer != null)
        {
            linesDataBuffer.Release();
            linesDataBuffer = null;
        }

        if (linePositionsBuffer != null)
        {
            linePositionsBuffer.Release();
            linePositionsBuffer = null;
        }

        if (lineColorsBuffer != null)
        {
            lineColorsBuffer.Release();
            lineColorsBuffer = null;
        }
    }

    private void SyncLineDataToGPU()
    {
        // Copy line data to the compute buffer
        linesDataBuffer.SetData(linesDataCPU, 0, 0, linesActive);

        // Set parameters on the compute shader
        lineDrawComputeShader.SetBuffer(0, "linesData", linesDataBuffer);
        lineDrawComputeShader.SetBuffer(0, "linePositions", linePositionsBuffer);
        lineDrawComputeShader.SetBuffer(0, "lineColors", lineColorsBuffer);
        lineDrawComputeShader.SetInt("LineCount", linesActive);
        lineDrawComputeShader.SetFloat("GlobalTime", Time.time);

        // Dispatch compute buffers to GPU for processing
        int threadGroups = Mathf.CeilToInt(linesActive / 64.0f);
        lineDrawComputeShader.Dispatch(0, threadGroups, 1, 1);
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------- Public Methods

    /// <summary>
    /// Draws a line from startPosition to endPosition. <br/>
    /// See <see cref="DrawLineCoroutine"/> <br/>
    /// </summary>
    public static void DrawLine(Vector3 startPosition,
                                Vector3 endPosition,
                                float drawDuration = 1,
                                float lifetime = Mathf.Infinity,
                                float width = 0.1f,
                                Color color = default,
                                int orderInLayer = 0)
    {
        instance.StartCoroutine(DrawLineCoroutine(startPosition, endPosition, drawDuration, lifetime, width, color,
                                                  orderInLayer));
    }

    /// <summary>
    /// Draws an arrow from startPosition to endPosition. <br/>
    /// See <see cref="DrawArrowCoroutine"/>
    /// </summary>
    public static void DrawArrow(Vector3 startPosition,
                                 Vector3 endPosition,
                                 float duration,
                                 float lifetime,
                                 float width,
                                 Color color)
    {
        instance.StartCoroutine(DrawArrowCoroutine(startPosition, endPosition, duration, lifetime, width, color));
    }

    /// <summary>
    /// Draws a 2D X-Y grid in 3D space. Can be used to draw skewed grids as well as orthogonal ones. <br/>
    /// See <see cref="Draw2DGridCoroutine"/>
    /// </summary>
    public static void Draw2DGrid(Vector3 bottomLeftCorner,
                                  Vector3 xVector,
                                  Vector3 yVector,
                                  int xCells,
                                  int yCells,
                                  float gridCellSize = 1,
                                  Color color = default,
                                  float drawDuration = 1,
                                  float lifetime = Mathf.Infinity)
    {
        instance.StartCoroutine(Draw2DGridCoroutine(bottomLeftCorner, xVector, yVector, xCells, yCells, gridCellSize, color,
                                                    drawDuration, lifetime));
    }

    /// <summary>
    /// Draws a cylinder in 3D space. <br/>
    /// </summary>
    public static void DrawCylinder(Vector3 startPosition,
                                    Vector3 endPosition,
                                    float startRadius,
                                    float endRadius = default,
                                    int sides = 36,
                                    float drawDuration = 1,
                                    float lifetime = Mathf.Infinity,
                                    float lineWidth = 0.1f,
                                    Vector3 startNormal = default,
                                    Vector3 endNormal = default,
                                    Color color = default,
                                    int orderInLayer = 0)
    {
        instance.StartCoroutine(DrawCylinderCoroutine(startPosition, endPosition, startRadius, endRadius, sides, drawDuration,
                                                      lifetime, lineWidth, startNormal, endNormal, color, orderInLayer));
    }

    /// <summary>
    /// Draws a polygon with a given number of sides.
    /// </summary>
    /// <param name="position">The position of the center of the polygon.</param>
    /// <param name="sides">How many sides the polygon has.</param>
    /// <param name="radius">The distance to each corner of the polygon.</param>
    /// <param name="drawDuration">How long the polygon takes to draw</param>
    /// <param name="lifetime">How long the polygon lasts in the scene.</param>
    /// <param name="color">The color of the polygon.</param>
    /// <param name="surfaceNormal">Which direction the face of the polygon is pointing towards.</param>
    public static void DrawNGon(Vector3 position,
                                int sides,
                                float radius,
                                float drawDuration = 1,
                                float lifetime = Mathf.Infinity,
                                Color color = default,
                                Vector3 surfaceNormal = default)
    {
        instance.StartCoroutine(DrawNGonCoroutine(position, sides, radius, drawDuration, lifetime, color, surfaceNormal));
    }




    // ------------------------------------------------------------------------------------------------------------------------------------- Private Coroutines




    /// <summary>
    /// Draws a line from startPosition to endPosition.
    /// </summary>
    /// <param name="startPosition">The point where the line starts.</param>
    /// <param name="endPosition">The point where the line ends.</param>
    /// <param name="drawDuration">How long the line should take to draw.</param>
    /// <param name="lifetime">How long the line should last.</param>
    /// <param name="width">The width of the line.</param>
    /// <param name="color">What color the line should be.</param>
    /// <param name="orderInLayer">The rendering order for the line, as dictated by Unity's rendering rules. The standard order for this library is:
    /// <br/> 1: Arrows
    /// <br/> 0: Other lines (default)
    /// </param>
    private static IEnumerator DrawLineCoroutine(Vector3 startPosition,
                                                 Vector3 endPosition,
                                                 float drawDuration,
                                                 float lifetime,
                                                 float width,
                                                 Color color,
                                                 int orderInLayer)
    {
        if (color == default) color = Color.white;

        if (linesActive >= instance.maxLines)
        {
            Debug.LogWarning("Reached maxLines limit!");
            yield break;
        }

        int index = linesActive++;

        linesDataCPU[index].startPos = startPosition;
        linesDataCPU[index].endPos = endPosition;
        linesDataCPU[index].startTime = Time.time;
        linesDataCPU[index].drawDuration = (drawDuration <= 0) ? 0.0001f : drawDuration;
        linesDataCPU[index].lifetime = lifetime;
        linesDataCPU[index].color = new Vector4(color.r, color.g, color.b, color.a);
    }

    /// <summary>
    /// Draws an arrow from startPosition to endPosition.
    /// </summary>
    /// <param name="startPosition">The point where the arrow starts.</param>
    /// <param name="endPosition">The point where the arrow ends and draws the head.</param>
    /// <param name="duration">How long the arrow should take to draw.</param>
    /// <param name="lifetime">How long the arrow should last.</param>
    /// <param name="color">What color the arrow should be.</param>
    /// <returns></returns>
    private static IEnumerator DrawArrowCoroutine(Vector3 startPosition,
                                                  Vector3 endPosition,
                                                  float duration,
                                                  float lifetime,
                                                  float width,
                                                  Color color)
    {
        float bodyTime = 0.7f * duration;
        float headTime = duration - bodyTime;


        // Draw arrow body
        instance.StartCoroutine(DrawLineCoroutine(startPosition, endPosition, bodyTime, lifetime + headTime, width, color, 1));
        yield return new WaitForSeconds(bodyTime);

        // Draw arrow head
        Vector3 arrowHeadEnd = endPosition + Quaternion.AngleAxis(33, Camera.main.transform.forward) *
                               (startPosition - endPosition) * 0.2f;
        instance.StartCoroutine(DrawLineCoroutine(endPosition, arrowHeadEnd, headTime, lifetime, width, color, 1));
        
        arrowHeadEnd = endPosition + Quaternion.AngleAxis(-33, Camera.main.transform.forward) * (startPosition - endPosition) *
                       0.2f;
        instance.StartCoroutine(DrawLineCoroutine(endPosition, arrowHeadEnd, headTime, lifetime, width, color, 1));
    }

    /// <summary>
    /// Draws a 2D X-Y grid in 3D space. Can be used to draw skewed grids as well as orthogonal ones.
    /// </summary>
    /// <param name="bottomLeftCorner">The position where the bottom left corner of the grid should be; used as a reference.</param>
    /// <param name="xVector">The direction in which horizontal lines will be drawn.</param>
    /// <param name="yVector">The direction in which vertical lines will be drawn.</param>
    /// <param name="xCells">The number of cells in the X direction.</param>
    /// <param name="yCells">The number of cells in the Y direction.</param>
    /// <param name="gridCellSize">The height and length of each grid cell.</param>
    /// <param name="lifetime">How long the grid should last.</param>
    /// <param name="color">What color the grid should be.</param>
    private static IEnumerator Draw2DGridCoroutine(Vector3 bottomLeftCorner,
                                                   Vector3 xVector,
                                                   Vector3 yVector,
                                                   int xCells,
                                                   int yCells,
                                                   float gridCellSize,
                                                   Color color,
                                                   float drawDuration,
                                                   float lifetime)
    {
        if (color == default) color = Color.white;

        Vector3 horizontalLineStartPos = bottomLeftCorner;
        Vector3 verticalLineStartPos = bottomLeftCorner;
        Vector3 horizontalLineEndPos = bottomLeftCorner + xVector.normalized * (gridCellSize * xCells);
        Vector3 verticalLineEndPos = bottomLeftCorner + yVector.normalized * (gridCellSize * yCells);

        float lineDuration = drawDuration / (Mathf.Max(xCells, yCells) + 1);

        for (int xIndex = 0, yIndex = 0; xIndex < yCells + 1 || yIndex < xCells + 1; xIndex++, yIndex++)
        {
            if (xIndex < yCells + 1)
            {
                DrawLine(horizontalLineStartPos, horizontalLineEndPos, lineDuration, lifetime, 0.1f, color);
                horizontalLineStartPos += yVector.normalized * gridCellSize;
                horizontalLineEndPos += yVector.normalized * gridCellSize;
            }
            if (yIndex < xCells + 1)
            {
                DrawLine(verticalLineStartPos, verticalLineEndPos, lineDuration, lifetime, 0.1f, color);
                verticalLineStartPos += xVector.normalized * gridCellSize;
                verticalLineEndPos += xVector.normalized * gridCellSize;
            }

            yield return new WaitForSeconds(lineDuration);
        }
    }

    private static IEnumerator DrawCylinderCoroutine(Vector3 startPosition,
                                                     Vector3 endPosition,
                                                     float startRadius,
                                                     float endRadius,
                                                     int sides,
                                                     float drawDuration,
                                                     float lifetime,
                                                     float lineWidth,
                                                     Vector3 startNormal,
                                                     Vector3 endNormal,
                                                     Color color,
                                                     int orderInLayer)
    {
        // Setup variables
        if (endRadius == default) endRadius = startRadius;
        if (color == default) color = Color.white;

        Vector3 centerVector = endPosition - startPosition;
        if (startNormal == default) startNormal = centerVector;
        if (endNormal == default) endNormal = centerVector;

        startNormal.Normalize();
        endNormal.Normalize();

        if (Vector3.Dot(startNormal, endNormal) < 0) startNormal *= -1;


        Vector3 startOrthogonalVector = Vector3.Cross(startNormal, Vector3.right).normalized;
        if (startOrthogonalVector.magnitude == 0) startOrthogonalVector = Vector3.back;
        startOrthogonalVector *= startRadius;

        Vector3 endOrthogonalVector = Vector3.Cross(endNormal, Vector3.right).normalized;
        if (endOrthogonalVector.magnitude == 0) endOrthogonalVector = Vector3.back;
        endOrthogonalVector *= endRadius;

        float rotationDegrees = 360 / sides;

        List<Vector3> startEdgePoints = new();
        List<Vector3> endEdgePoints = new();

        // Create lists of vectors to use for drawing edges
        for (float angle = 0; angle < 360; angle += rotationDegrees)
        {
            startEdgePoints.Add(startOrthogonalVector);
            startOrthogonalVector = VectorRotation.RotateVector(startNormal, startOrthogonalVector, rotationDegrees);

            endEdgePoints.Add(endOrthogonalVector);
            endOrthogonalVector = VectorRotation.RotateVector(endNormal, endOrthogonalVector, rotationDegrees);
        }

        // Move the points relative to the start and end positions
        for (int i = 0; i < startEdgePoints.Count; i++)
        {
            startEdgePoints[i] += startPosition;
            endEdgePoints[i] += endPosition;
        }

        // Find the closest pair of points and convert it to an index modifier to make sure final cylinder is untwisted
        float smallestDistance = Mathf.Infinity;
        int edgeIndexModifier = 0;
        for (int i = 0; i < startEdgePoints.Count; i++)
        {
            for (int j = 0; j < startEdgePoints.Count; j++)
            {
                float currentDistance = Vector3.Distance(startEdgePoints[i], endEdgePoints[j]);
                if (currentDistance < smallestDistance)
                {
                    smallestDistance = currentDistance;
                    edgeIndexModifier = (int)Mathf.Repeat(j - i, startEdgePoints.Count);
                }
            }
        }

        // Draw edges
        float edgeDrawDuration = drawDuration / sides;
        for (int i = 0; i < startEdgePoints.Count; i++)
        {
            DrawLine(startEdgePoints[i], endEdgePoints[(i + edgeIndexModifier) % startEdgePoints.Count], edgeDrawDuration,
                     lifetime, lineWidth, color, orderInLayer);
            DrawLine(startEdgePoints[i], startEdgePoints[(i + 1) % startEdgePoints.Count], edgeDrawDuration, lifetime, lineWidth,
                     color, orderInLayer);
            DrawLine(endEdgePoints[i], endEdgePoints[(i + 1) % startEdgePoints.Count], edgeDrawDuration, lifetime, lineWidth,
                     color, orderInLayer);
            if (drawDuration > 0) yield return new WaitForSeconds(edgeDrawDuration);
        }
    }

    private static IEnumerator DrawNGonCoroutine(Vector3 position,
                                                 int sides,
                                                 float radius,
                                                 float drawDuration,
                                                 float lifetime,
                                                 Color color,
                                                 Vector3 surfaceNormal)
    {
        //Check for defaults
        if (color == default) color = Color.white;
        if (surfaceNormal == default) surfaceNormal = Camera.main.transform.forward;

        // Set up variables
        Vector3 orthogonalVector = Vector3.Cross(surfaceNormal, Vector3.right).normalized;
        if (orthogonalVector.magnitude == 0) orthogonalVector = Vector3.back;
        orthogonalVector *= radius;

        float rotationDegrees = 360 / sides;

        List<Vector3> edgePoints = new();

        // Create lists of vectors to use for drawing edges
        for (float angle = 0; angle < 360; angle += rotationDegrees)
        {
            edgePoints.Add(orthogonalVector + position);
            orthogonalVector = VectorRotation.RotateVector(surfaceNormal, orthogonalVector, rotationDegrees);
        }

        // Draw edges
        float edgeDrawDuration = drawDuration / sides;
        bool shouldWait = drawDuration > 0;
        for (int i = 0; i < edgePoints.Count; i++)
        {
            DrawLine(edgePoints[i], edgePoints[(i + 1) % edgePoints.Count], edgeDrawDuration, lifetime, 0,
                     color, 0);
            if (shouldWait) yield return new WaitForSeconds(edgeDrawDuration);
        }
    }
}