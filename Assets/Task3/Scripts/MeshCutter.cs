using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCutter : MonoBehaviour
{

    bool cutComplete = false;
    // Update is called once per frame
    public void InitiateCut(LineScript lineScript)                      // Called From LineScript on LineComplete
    {
        if (!cutComplete)                                               // Checking if Already cut in this session
        {
            
            Transform cutplane;
            Vector3 pointA, pointB, pointC;
            Plane myplane;

            pointA = lineScript.lineRenderer.GetPosition(0);
            pointB = lineScript.lineRenderer.GetPosition(1);
            pointC = pointA + (Camera.main.transform.forward * -10);    // Third point to get plane from line drawn
            myplane = new Plane(pointA, pointB, pointC);                // Creating plane from lin points plus 3rd point

            Vector3 dir = (pointB - pointA).normalized;
            Vector3 point = pointA + (0.5f * dir);

            cutComplete = MeshCut.Cut(gameObject, point, myplane.normal);   // Calling MeshCut , will return true if line/plane intersects mesh
        }

    }

}
