using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewMesh2D : MonoBehaviour
{
    private Mesh mesh;
    private bool isActivating = false;
    public bool IsActivating => isActivating;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "FOV Mesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void DrawFOV(float radius, float angle, int resolution, Vector2 direction)
    {
        direction.Normalize();

        Vector3[] vertices = new Vector3[resolution + 2];
        int[] triangles = new int[resolution * 3];

        vertices[0] = Vector3.zero;

        float halfAngle = angle / 2f;
        float angleStep = angle / resolution;

        Vector3 baseDir = Vector3.right;
        float baseAngle = Vector2.SignedAngle(Vector2.right, direction); // 기준 방향 회전량

        for (int i = 0; i <= resolution; i++)
        {
            float currentAngle = -halfAngle + i * angleStep;
            float totalAngle = baseAngle + currentAngle;

            Vector3 rotatedDir = Quaternion.Euler(0, 0, totalAngle) * baseDir;
            vertices[i + 1] = rotatedDir.normalized * radius;
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

        isActivating = true;
    }



    public void ClearMesh()
    {
        mesh.Clear();

        isActivating = false;
    }
}
