using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineScript : MonoBehaviour
{
    public List<Vector3> linePoints = new List<Vector3>();
    public LineRenderer lineRenderer;
    public float startWidth = 1.0f;
    public float endWidth = 1.0f;
    Camera thisCamera;
    int lineCount = 0;

    Vector3 lastPos = Vector3.one * float.MaxValue;

    public MeshCutter cuttableObject;

    void Awake()
    {
        thisCamera = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5;//thisCamera.nearClipPlane;
        Vector3 mouseWorld = thisCamera.ScreenToWorldPoint(mousePos);

    
        if(Input.GetMouseButtonDown(0))                                     // Start line draw on mouse down
        {
            linePoints = new List<Vector3>();
            linePoints.Add(mouseWorld);
        }
        if (Input.GetMouseButton(0))                                       // Update Line on Mouse
        {
            if(linePoints.Count == 1)
                linePoints.Add(mouseWorld);
            else if(linePoints.Count == 2)
                linePoints[1] = (mouseWorld);
        }
        if(Input.GetMouseButtonUp(0))                                       // End Line draw and Call Splitting of object on mouse up
        {
            if(linePoints.Count>=2)
                cuttableObject.InitiateCut(this);
            
        }
            UpdateLine();
    }


    void UpdateLine()                                                       // Update Line Renderer
    {
        lineRenderer.SetWidth(startWidth, endWidth);
        lineRenderer.SetVertexCount(linePoints.Count);
        lineRenderer.SetPositions(linePoints.ToArray());

        //lineCount = linePoints.Count;
    }

}