using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshCut
{

    private static Plane blade;
    private static Transform victim_transform;
    private static Mesh victim_mesh;

    private static bool[] sides = new bool[3];

    // Gathering values
    private static List<int> left_Gather_subIndices = new List<int> ();
    private static List<int> right_Gather_subIndices = new List<int>();

    private static List<Vector3> left_Gather_added_Points = new List<Vector3> ();
        private static List<Vector2> left_Gather_added_uvs = new List<Vector2>() ;
    private static List<Vector3> left_Gather_added_normals = new List<Vector3> ();

    private static List<Vector3> right_Gather_added_Points = new List<Vector3>() ;
    private static List<Vector2> right_Gather_added_uvs = new List<Vector2> ();
    private static List<Vector3> right_Gather_added_normals = new List<Vector3> ();



    // face cutting temps
    private static Vector3 leftPoint1 = Vector3.zero;
    private static Vector3 leftPoint2 = Vector3.zero;
    private static Vector3 rightPoint1 = Vector3.zero;
    private static Vector3 rightPoint2 = Vector3.zero;

    private static Vector2 left_uv1 = Vector3.zero;
    private static Vector2 left_uv2 = Vector3.zero;
    private static Vector2 right_uv1 = Vector3.zero;
    private static Vector2 right_uv2 = Vector3.zero;

    private static Vector3 left_normal1 = Vector3.zero;
    private static Vector3 left_normal2 = Vector3.zero;
    private static Vector3 right_normal1 = Vector3.zero;
    private static Vector3 right_normal2 = Vector3.zero;


    // final arrays
    private static List<int> left_Final_subIndices = new List<int> ();

    private static List<Vector3> left_Final_vertices = new List<Vector3>();
    private static List<Vector3> left_Final_normals = new List<Vector3>();
    private static List<Vector2> left_Final_uvs = new List<Vector2>();

    private static List<int> right_Final_subIndices = new List<int>() ;

    private static List<Vector3> right_Final_vertices = new List<Vector3>();
    private static List<Vector3> right_Final_normals = new List<Vector3>();
    private static List<Vector2> right_Final_uvs = new List<Vector2>();

    // capping stuff
    private static List<Vector3> createdVertexPoints = new List<Vector3>();

    #region Public_CutFunction

    public static bool Cut(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection)
    {

        victim_transform = victim.transform;

        // Creating Plane To Split Sphere
        blade = new Plane(victim.transform.InverseTransformDirection(normalDirection),
                          victim.transform.InverseTransformPoint(anchorPoint));

        victim_mesh = victim.GetComponent<MeshFilter>().mesh;
        victim_mesh.subMeshCount = 2;

        // Resetting Local Variables
        ResetGatheringValues();

        // Initializing Triangle Variables
        int p1 = 0;
        int p2 = 0;
        int p3 = 0;

        sides = new bool[3];

        int[] indices = victim_mesh.triangles;

        // Loop to iterate through triangles of mesh
        for (int i = 0; i < indices.Length; i += 3)
        {
            p1 = indices[i];
            p2 = indices[i + 1];
            p3 = indices[i + 2];

            // Checking for each triangle point, on which side of plane it lies
            sides[0] = blade.GetSide(victim_mesh.vertices[p1]);
            sides[1] = blade.GetSide(victim_mesh.vertices[p2]);
            sides[2] = blade.GetSide(victim_mesh.vertices[p3]);



            //  If triangle is on either side of plane
            if (sides[0] == sides[1] && sides[0] == sides[2])
            {
                // Triangle on negative side of plane
                if (sides[0])
                { // left side
                    left_Gather_subIndices.Add(p1);
                    left_Gather_subIndices.Add(p2);
                    left_Gather_subIndices.Add(p3);

                }
                // Triangle on positive side of plane
                else
                {
                    right_Gather_subIndices.Add(p1);
                    right_Gather_subIndices.Add(p2);
                    right_Gather_subIndices.Add(p3);

                }

            }
            //  If plane passes through Triangle
            else
            { // cut face
                ResetFaceCuttingTemps();
                int[] indeces = { p1, p2, p3 };
                Cut_this_Face(indeces);
            }
        }


        // set final arrays
        ResetFinalArrays();
        SetFinalArrays_withOriginals();
        AddNewTriangles_toFinalArrays();
        MakeCaps();

        // Creating Split Mesh Left
        Mesh left_HalfMesh = new Mesh();
        left_HalfMesh.name = "LeftMesh";
        left_HalfMesh.vertices = left_Final_vertices.ToArray();

        left_HalfMesh.subMeshCount = 2;
        left_HalfMesh.SetIndices(left_Final_subIndices.ToArray(), MeshTopology.Triangles, 0);
        left_HalfMesh.SetIndices(left_Final_subIndices.ToArray(), MeshTopology.Triangles, 1);

        left_HalfMesh.normals = left_Final_normals.ToArray();
        left_HalfMesh.uv = left_Final_uvs.ToArray();

        // Creating Split Mesh Right
        Mesh right_HalfMesh = new Mesh();
        right_HalfMesh.name = "RightMesh";
        right_HalfMesh.vertices = right_Final_vertices.ToArray();

        right_HalfMesh.subMeshCount = 2;
        right_HalfMesh.SetIndices(right_Final_subIndices.ToArray(), MeshTopology.Triangles, 0);
        right_HalfMesh.SetIndices(right_Final_subIndices.ToArray(), MeshTopology.Triangles, 1);

        right_HalfMesh.normals = right_Final_normals.ToArray();
        right_HalfMesh.uv = right_Final_uvs.ToArray();


        // Instantiating New Object as Right Mesh if mesh not empty
        if (left_HalfMesh.vertexCount > 0 && right_HalfMesh.vertexCount > 0)
        {
            victim.name = "leftSide";
            victim.GetComponent<MeshFilter>().mesh = left_HalfMesh;

            // Setting Original Object as Left Mesh
            GameObject leftSideObj = victim;
            GameObject rightSideObj = new GameObject("rightSide", typeof(MeshFilter), typeof(MeshRenderer));
            // Adding Offset to position, so cut is visible
            rightSideObj.transform.position = victim_transform.position + (normalDirection * -0.25f);
            rightSideObj.transform.rotation = victim_transform.rotation;
            rightSideObj.GetComponent<MeshFilter>().mesh = right_HalfMesh;

            Material[] mats = new Material[] { victim.GetComponent<MeshRenderer>().material, victim.GetComponent<MeshRenderer>().material };
            leftSideObj.GetComponent<MeshRenderer>().materials = mats;
            rightSideObj.GetComponent<MeshRenderer>().materials = mats;

            return true;
        }


        return false;

    }
    #endregion


#region TrianglesCutAndAdd

    static void Cut_this_Face(int[] indeces)
    {

        int p = indeces[0];
        for (int side = 0; side < 3; side++)
        {
            p = indeces[side];

            if (sides[side]) //On Left Side of Plane
            {
                if (leftPoint1 == Vector3.zero)
                {

                    leftPoint1 = victim_mesh.vertices[p];
                    leftPoint2 = leftPoint1;
                    left_uv1 = victim_mesh.uv[p];
                    left_uv2 = left_uv1;
                    left_normal1 = victim_mesh.normals[p];
                    left_normal2 = left_normal1;

                }
                else
                {

                    leftPoint2 = victim_mesh.vertices[p];
                    left_uv2 = victim_mesh.uv[p];
                    left_normal2 = victim_mesh.normals[p];

                }
            }
            else  //On Right Side of Plane
            {
                if (rightPoint1 == Vector3.zero)
                {

                    rightPoint1 = victim_mesh.vertices[p];
                    rightPoint2 = rightPoint1;
                    right_uv1 = victim_mesh.uv[p];
                    right_uv2 = right_uv1;
                    right_normal1 = victim_mesh.normals[p];
                    right_normal2 = right_normal1;

                }
                else
                {

                    rightPoint2 = victim_mesh.vertices[p];
                    right_uv2 = victim_mesh.uv[p];
                    right_normal2 = victim_mesh.normals[p];

                }
            }
        }


        float normalizedDistance = 0.0f;
        float distance = 0;
        blade.Raycast(new Ray(leftPoint1, (rightPoint1 - leftPoint1).normalized), out distance);

        normalizedDistance = distance / (rightPoint1 - leftPoint1).magnitude;
        Vector3 newVertex1 = leftPoint1 + (rightPoint1-leftPoint1)*normalizedDistance;//Vector3.Lerp(leftPoint1, rightPoint1, normalizedDistance);
        Vector2 newUv1 = left_uv1 + (right_uv1-left_uv1)*normalizedDistance;//Vector2.Lerp(left_uv1, right_uv1, normalizedDistance);
        Vector3 newNormal1 = left_normal1+ (right_normal1-left_normal1)*normalizedDistance;//Vector3.Lerp(left_normal1, right_normal1, normalizedDistance);

        createdVertexPoints.Add(newVertex1);

        blade.Raycast(new Ray(leftPoint2, (rightPoint2 - leftPoint2).normalized), out distance);

        normalizedDistance = distance / (rightPoint2 - leftPoint2).magnitude;
        Vector3 newVertex2 = leftPoint2+ (rightPoint2 - leftPoint2) * normalizedDistance;//Vector3.Lerp(leftPoint1, rightPoint1, normalizedDistance);
        Vector2 newUv2 = left_uv2+(right_uv2 - left_uv2) * normalizedDistance;//Vector2.Lerp(left_uv1, right_uv1, normalizedDistance);
        Vector3 newNormal2 = left_normal2+(right_normal2 - left_normal2) * normalizedDistance;//Vector3.Lerp(left_normal1, right_normal1, normalizedDistance);

        createdVertexPoints.Add(newVertex2);


        // first triangle
        Add_Left_triangle (newNormal1, new Vector3[] { leftPoint1, newVertex1, newVertex2 },
        new Vector2[] { left_uv1, newUv1, newUv2 },
        new Vector3[] { left_normal1, newNormal1, newNormal2 });

        // second triangle
        Add_Left_triangle( newNormal2, new Vector3[] { leftPoint1, leftPoint2, newVertex2 },
        new Vector2[] { left_uv1, left_uv2, newUv2 },
        new Vector3[] { left_normal1, left_normal2, newNormal2 });

        // first triangle
        Add_Right_triangle( newNormal1, new Vector3[] { rightPoint1, newVertex1, newVertex2 },
        new Vector2[] { right_uv1, newUv1, newUv2 },
        new Vector3[] { right_normal1, newNormal1, newNormal2 });

        // second triangle
        Add_Right_triangle( newNormal2, new Vector3[] { rightPoint1, rightPoint2, newVertex2 },
        new Vector2[] { right_uv1, right_uv2, newUv2 },
        new Vector3[] { right_normal1, right_normal2, newNormal2 });

    }

    static void Add_Left_triangle( Vector3 faceNormal, Vector3[] points, Vector2[] uvs, Vector3[] normals)
    {

        int p1 = 0;
        int p2 = 1;
        int p3 = 2;

        Vector3 calculated_normal = Vector3.Cross((points[1] - points[0]).normalized, (points[2] - points[0]).normalized); 

        if (Vector3.Dot(calculated_normal, faceNormal) < 0) //inverting triangle if face is wrong side
        {

            p1 = 2;
            p2 = 1;
            p3 = 0;
        }

        left_Gather_added_Points.Add(points[p1]);
        left_Gather_added_Points.Add(points[p2]);
        left_Gather_added_Points.Add(points[p3]);

        left_Gather_added_uvs.Add(uvs[p1]);
        left_Gather_added_uvs.Add(uvs[p2]);
        left_Gather_added_uvs.Add(uvs[p3]);

        left_Gather_added_normals.Add(normals[p1]);
        left_Gather_added_normals.Add(normals[p2]);
        left_Gather_added_normals.Add(normals[p3]);

    }

    static void Add_Right_triangle(Vector3 faceNormal, Vector3[] points, Vector2[] uvs, Vector3[] normals)
    {


        int p1 = 0;
        int p2 = 1;
        int p3 = 2;

        Vector3 calculated_normal = Vector3.Cross((points[1] - points[0]).normalized, (points[2] - points[0]).normalized);

        if (Vector3.Dot(calculated_normal, faceNormal) < 0)
        {

            p1 = 2;
            p2 = 1;
            p3 = 0;
        }


        right_Gather_added_Points.Add(points[p1]);
        right_Gather_added_Points.Add(points[p2]);
        right_Gather_added_Points.Add(points[p3]);

        right_Gather_added_uvs.Add(uvs[p1]);
        right_Gather_added_uvs.Add(uvs[p2]);
        right_Gather_added_uvs.Add(uvs[p3]);

        right_Gather_added_normals.Add(normals[p1]);
        right_Gather_added_normals.Add(normals[p2]);
        right_Gather_added_normals.Add(normals[p3]);

    }
#endregion

    #region ResetFunctions
    static void ResetGatheringValues()
    {

        left_Gather_subIndices.Clear();
        left_Gather_subIndices.Clear();
        left_Gather_added_Points.Clear();
        left_Gather_added_Points.Clear();
        left_Gather_added_uvs.Clear();
        left_Gather_added_uvs.Clear();
        left_Gather_added_normals.Clear();
        left_Gather_added_normals.Clear();

        right_Gather_subIndices.Clear();
        right_Gather_subIndices.Clear();
        right_Gather_added_Points.Clear();
        right_Gather_added_Points.Clear();
        right_Gather_added_uvs.Clear();
        right_Gather_added_uvs.Clear();
        right_Gather_added_normals.Clear();
        right_Gather_added_normals.Clear();

        createdVertexPoints.Clear();

    }

    static void ResetFaceCuttingTemps()
    {

        leftPoint1 = Vector3.zero;
        leftPoint2 = Vector3.zero;
        rightPoint1 = Vector3.zero;
        rightPoint2 = Vector3.zero;

        left_uv1 = Vector3.zero;
        left_uv2 = Vector3.zero;
        right_uv1 = Vector3.zero;
        right_uv2 = Vector3.zero;

        left_normal1 = Vector3.zero;
        left_normal2 = Vector3.zero;
        right_normal1 = Vector3.zero;
        right_normal2 = Vector3.zero;

    }
    static void ResetFinalArrays()
    {

        left_Final_subIndices.Clear();
        left_Final_subIndices.Clear();
        left_Final_vertices.Clear();
        left_Final_normals.Clear();
        left_Final_uvs.Clear();

        right_Final_subIndices.Clear();
        right_Final_subIndices.Clear();
        right_Final_vertices.Clear();
        right_Final_normals.Clear();
        right_Final_uvs.Clear();

    }
    #endregion

    #region FinalArrays
    static void SetFinalArrays_withOriginals()
    {
           int p = 0;
           for (int i = 0; i < left_Gather_subIndices.Count; i++)
            {

                p = left_Gather_subIndices[i];

                left_Final_vertices.Add(victim_mesh.vertices[p]);
                left_Final_subIndices.Add(i);
                left_Final_normals.Add(victim_mesh.normals[p]);
                left_Final_uvs.Add(victim_mesh.uv[p]);

            }

            for (int i = 0; i < right_Gather_subIndices.Count; i++)
            {

                p = right_Gather_subIndices[i];

                right_Final_vertices.Add(victim_mesh.vertices[p]);
                right_Final_subIndices.Add(i);
                right_Final_normals.Add(victim_mesh.normals[p]);
                right_Final_uvs.Add(victim_mesh.uv[p]);

            }

    }

    static void AddNewTriangles_toFinalArrays()
    {

            int count = left_Final_vertices.Count;
            // add the new ones
            for (int i = 0; i < left_Gather_added_Points.Count; i++)
            {

                left_Final_vertices.Add(left_Gather_added_Points[i]);
                left_Final_subIndices.Add(i + count);
                left_Final_uvs.Add(left_Gather_added_uvs[i]);
                left_Final_normals.Add(left_Gather_added_normals[i]);

            }

            count = right_Final_vertices.Count;

            for (int i = 0; i < right_Gather_added_Points.Count; i++)
            {

                right_Final_vertices.Add(right_Gather_added_Points[i]);
                right_Final_subIndices.Add(i + count);
                right_Final_uvs.Add(right_Gather_added_uvs[i]);
                right_Final_normals.Add(right_Gather_added_normals[i]);

            }
      

    }
    #endregion

    #region CapStuff
    private static List<Vector3> capVertpolygon = new List<Vector3>();

    static void MakeCaps()
    {
        
        capVertpolygon.Clear();

        if (createdVertexPoints.Count > 1)
        {
            capVertpolygon.Add(createdVertexPoints[0]);
            capVertpolygon.Add(createdVertexPoints[1]);

            bool isDone = false;
            while (!isDone)
            {
                isDone = true;

                for (int k = 0; k < createdVertexPoints.Count; k += 2)
                { // go through the pairs

                    // for-all-created verteces, if is 2nd of capPolys and its pair  //(for-all-created not in capTracker)
                    if (createdVertexPoints[k] == capVertpolygon[capVertpolygon.Count - 1] && !capVertpolygon.Contains(createdVertexPoints[k + 1]))
                    { // if so add the other

                        isDone = false;
                        capVertpolygon.Add(createdVertexPoints[k + 1]);

                    }
                    else if (createdVertexPoints[k + 1] == capVertpolygon[capVertpolygon.Count - 1] && !capVertpolygon .Contains(createdVertexPoints[k]))
                    {// if so add the other

                        isDone = false;
                        capVertpolygon.Add(createdVertexPoints[k]);
                    }
                }
            }
            Debug.Log("COUNNT:" + capVertpolygon.Count);
            FillCap(capVertpolygon);

        }

    }

    static void FillCap(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        Vector3 center = Vector3.zero;
        foreach (Vector3 point in vertices)
            center += point;

        center = center / vertices.Count;


        Vector3 upward = Vector3.zero;
        // 90 degree turn
        upward.x = blade.normal.y;
        upward.y = -blade.normal.x;
        upward.z = blade.normal.z;
        Vector3 left = Vector3.Cross(blade.normal, upward);

        Vector3 displacement = Vector3.zero;
        Vector3 relativePosition = Vector3.zero;

        for (int i = 0; i < vertices.Count; i++)
        {

            displacement = vertices[i] - center;
            relativePosition = Vector3.zero;
            relativePosition.x = 0.5f + Vector3.Dot(displacement, left);
            relativePosition.y = 0.5f + Vector3.Dot(displacement, upward);
            //relativePosition.z = 0.5f + Vector3.Dot(displacement, blade.normal);

            uvs.Add(new Vector2(relativePosition.x, relativePosition.y));
            normals.Add(blade.normal);
        }


        vertices.Add(center);
        normals.Add(blade.normal);
        uvs.Add(new Vector2(0.5f, 0.5f));

        Vector3 calculated_normal = Vector3.zero;
        int otherIndex = 0;
        for (int i = 0; i < vertices.Count; i++)
        {

            otherIndex = (i + 1) % (vertices.Count - 1);

            calculated_normal = Vector3.Cross((vertices[otherIndex] - vertices[i]).normalized,
                                              (vertices[vertices.Count - 1] - vertices[i]).normalized);

            if (Vector3.Dot(calculated_normal, blade.normal) < 0)
            {

                triangles.Add(vertices.Count - 1);
                triangles.Add(otherIndex);
                triangles.Add(i);
            }
            else
            {

                triangles.Add(i);
                triangles.Add(otherIndex);
                triangles.Add(vertices.Count - 1);
            }

        }

        int index = 0;
        for (int i = 0; i < triangles.Count; i++)
        {

            index = triangles[i];
            right_Final_vertices.Add(vertices[index]);
            right_Final_subIndices.Add(right_Final_vertices.Count - 1);
            right_Final_normals.Add(normals[index]);
            right_Final_uvs.Add(uvs[index]);

        }

        //flipping normals for 2nd mesh
        for (int i = 0; i < normals.Count; i++)
        {
            normals[i] = -normals[i];
        }

        //flipping triangles for 2nd mesh
        int temp1, temp2;
        for (int i = 0; i < triangles.Count; i += 3)
        {

            temp1 = triangles[i + 2];
            temp2 = triangles[i];

            triangles[i] = temp1;
            triangles[i + 2] = temp2;
        }

        for (int i = 0; i < triangles.Count; i++)
        {

            index = triangles[i];
            left_Final_vertices.Add(vertices[index]);
            left_Final_subIndices.Add(left_Final_vertices.Count - 1);
            left_Final_normals.Add(normals[index]);
            left_Final_uvs.Add(uvs[index]);

        }

    }
#endregion

}