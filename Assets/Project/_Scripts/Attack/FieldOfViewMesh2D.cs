using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewMesh2D : MonoBehaviour
{
    [Header("FOV Settings")]
    [Range(1f, 360f)] public float viewAngle = 90f;
    public float viewRadius = 5f;
    public int resolution = 30;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "FOV Mesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            DrawFOV();
        }
        else
        {
            mesh.Clear();
        }
    }

    public void DrawFOV()
    {
        vertices = new Vector3[resolution + 2];
        triangles = new int[resolution * 3];

        vertices[0] = Vector3.zero;

        float angleStep = viewAngle / resolution;
        float startAngle = -viewAngle / 2f;

        for (int i = 0; i <= resolution; i++)
        {
            float angle = startAngle + i * angleStep;
            float rad = Mathf.Deg2Rad * angle;

            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            vertices[i + 1] = dir * viewRadius;
        }

        for (int i = 0; i < resolution; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    public void ClearMesh()
    {
        if (mesh != null)
        {
            mesh.Clear();
        }
    }
}
