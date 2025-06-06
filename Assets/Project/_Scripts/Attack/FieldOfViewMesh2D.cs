using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewMesh2D : MonoBehaviour
{
    private Mesh mesh; // 부채꼴 Mesh
    private Vector3[] vertices; // 정점 배열
    private int[] triangles; // 삼각형 인덱스 배열

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "FOV Mesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    /// <summary>
    /// 부채꼴을 그리는 함수
    /// </summary>
    /// <param name="radius">반지름</param>
    /// <param name="angle">각도</param>
    /// <param name="resolution">삼각형 수</param>
    public void DrawFOV(float radius, float angle, int resolution)
    {
        if (resolution < 1) return;

        vertices = new Vector3[resolution + 2]; // 중심점 + 외곽 정점들
        triangles = new int[resolution * 3];    // 삼각형 개수 * 3 (정점 인덱스)

        vertices[0] = Vector3.zero; // 중심점

        float angleStep = angle / resolution;
        float startAngle = -angle / 2f;

        for (int i = 0; i <= resolution; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            float rad = Mathf.Deg2Rad * currentAngle;

            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            vertices[i + 1] = dir * radius;
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

    /// <summary>
    /// 메쉬 초기화
    /// </summary>
    public void ClearMesh()
    {
        if (mesh != null)
        {
            mesh.Clear();
        }
    }
}
