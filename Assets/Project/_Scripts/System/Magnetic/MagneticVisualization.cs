using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MagneticVisualization : MonoBehaviour
{
    [Header("FOV Settings")]
    [SerializeField]
    private FOVType m_fovType = FOVType.Circle; // Start에서 사용되므로 중간에 변경 불가
    [SerializeField]
    private float m_range = 8f;
    [SerializeField]
    private float m_coneAngle = 90f;
    [SerializeField]
    private int m_rayCount = 128;

    [Header("Visualization")]
    [SerializeField]
    private bool m_fovEdgeEnabled = false; // Start에서 사용되므로 중간에 변경해도 적용 안됨
    [SerializeField]
    private Material m_fovMainMaterial;
    [SerializeField]
    private Material m_fovEdgeMaterial;
    public LayerMask ObstacleLayer = -1;

    [Header("Tilemap Support")]
    public Tilemap[] ObstacleTilemaps;
    public float TileSize = 1f;
    [SerializeField]
    private float m_tilemapSearchPadding = 2f; // 타일맵 검사시 추가 범위 (m_range + a)

    // Compute Shader 관련
    [Header("Compute Shader")]
    [SerializeField]
    private ComputeShader m_fovComputeShader;
    private ComputeBuffer m_rayBuffer;
    private ComputeBuffer m_obstacleBuffer;

    // 시각화 관련
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;
    private Mesh m_fovMesh;

    // 성능 최적화를 위한 캐싱
    private List<ObstacleData> m_cachedObstacles = new List<ObstacleData>();
    private Vector3 m_lastPlayerPosition;
    private const float m_POSITION_THRESHOLD = 0.1f; // 위치 변화 임계값

    public enum FOVType { Circle, Cone }

    // 데이터 구조체
    struct RayData
    {
        public Vector2 Direction;
        public float MaxDistance;
        public float HitDistance;
    }

    struct ObstacleData
    {
        public enum ShapeType
        {
            Box = 0,
            Circle = 1,
            Polygon = 2
        }
        public Vector2 Position;
        public Vector2 Size;
        public Vector2[] Vertice;
        public float[] Indice;
        public ShapeType Type;
    }

    void Start()
    {
        InitializeFOV();
        SetupVisualization();
        m_lastPlayerPosition = transform.position;
    }

    void Update()
    {
        UpdateFOV();
        UpdateVisualization();
    }

    void InitializeFOV()
    {
        // 버퍼 생성 (더 많은 장애물을 처리할 수 있도록 확장)
        m_rayBuffer = new ComputeBuffer(m_rayCount, sizeof(float) * 4);
        m_obstacleBuffer = new ComputeBuffer(200, sizeof(float) * 5); // 타일맵 포함해서 더 많은 장애물 처리

        // 레이 데이터 초기화
        InitializeRays();
    }

    void InitializeRays()
    {
        RayData[] rays = new RayData[m_rayCount];

        for (int i = 0; i < m_rayCount; ++i)
        {
            float angle;

            if (m_fovType == FOVType.Circle)
            {
                // 원형 - 360도
                angle = (360f / m_rayCount) * i;
            }
            else
            {
                // 원뿔형 - 지정된 각도 범위
                float startAngle = -m_coneAngle * 0.5f;
                angle = startAngle + (m_coneAngle / (m_rayCount - 1)) * i;

                // 플레이어 방향 추가
                angle += transform.eulerAngles.z;
            }

            float radians = angle * Mathf.Deg2Rad;
            rays[i] = new RayData
            {
                Direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)),
                MaxDistance = m_range,
                HitDistance = m_range
            };
        }

        m_rayBuffer.SetData(rays);
    }

    void UpdateFOV()
    {
        if (m_fovComputeShader == null) return;

        // 방향이 바뀌면 레이 재계산 (Cone 모드에서)
        if (m_fovType == FOVType.Cone)
        {
            InitializeRays();
        }

        // 플레이어 위치가 크게 변했을 때만 장애물 재수집 (성능 최적화)
        // 아래와 같이 최적화하려고 하였으나 플랫폼이 움직이는 등 물체가 움직이면 반영 안됨
        //if (Vector3.Distance(transform.position, lastPlayerPosition) > POSITION_THRESHOLD)
        //{
        //    CollectAllObstacles();
        //    lastPlayerPosition = transform.position;
        //}
        CollectAllObstacles();

        // Compute Shader 실행
        int kernel = m_fovComputeShader.FindKernel("MagneticFOV");

        m_fovComputeShader.SetVector("_PlayerPos", transform.position);
        m_fovComputeShader.SetFloat("_Range", m_range);
        m_fovComputeShader.SetInt("_RayCount", m_rayCount);
        m_fovComputeShader.SetInt("_ObstacleCount", m_cachedObstacles.Count);

        m_fovComputeShader.SetBuffer(kernel, "_RayBuffer", m_rayBuffer);
        m_fovComputeShader.SetBuffer(kernel, "_ObstacleBuffer", m_obstacleBuffer);

        // 장애물 데이터 업데이트
        if (m_cachedObstacles.Count > 0)
        {
            m_obstacleBuffer.SetData(m_cachedObstacles.ToArray());
        }

        int threadGroups = Mathf.CeilToInt(m_rayCount / 64f);
        m_fovComputeShader.Dispatch(kernel, threadGroups, 1, 1);
    }

    void CollectAllObstacles()
    {
        m_cachedObstacles.Clear();

        // 1. 일반 콜라이더 수집
        CollectRegularObstacles();

        // 2. 타일맵을 Box로 변환해서 수집
        CollectTilemapAsBoxes();

        //Debug.Log($"Total obstacles collected: {cachedObstacles.Count}");
    }

    void CollectRegularObstacles()
    {
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(transform.position, m_range * 1.5f, ObstacleLayer);

        foreach (var obstacle in obstacles)
        {
            // TilemapCollider2D는 별도 처리하므로 건너뛰기
            if (obstacle is TilemapCollider2D) continue;

            ObstacleData data = new ObstacleData
            {
                Position = obstacle.transform.position,
                Size = obstacle.bounds.size,
                Type = obstacle is BoxCollider2D ? ObstacleData.ShapeType.Box : ObstacleData.ShapeType.Circle
            };

            m_cachedObstacles.Add(data);
        }
    }

    void CollectTilemapAsBoxes()
    {
        if (ObstacleTilemaps == null || ObstacleTilemaps.Length == 0) return;

        Vector3 playerPos = transform.position;
        float searchRange = m_range + m_tilemapSearchPadding;

        foreach (Tilemap tilemap in ObstacleTilemaps)
        {
            if (tilemap == null) continue;

            // 플레이어 주변의 타일 영역만 검사
            Vector3Int playerCellPos = tilemap.WorldToCell(playerPos);
            int cellRange = Mathf.CeilToInt(searchRange / TileSize);

            for (int y = playerCellPos.y - cellRange; y <= playerCellPos.y + cellRange; y++)
            {
                for (int x = playerCellPos.x - cellRange; x <= playerCellPos.x + cellRange; x++)
                {
                    Vector3Int cellPosition = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(cellPosition);

                    if (tile != null)
                    {
                        // 타일의 월드 좌표 계산
                        Vector3 tileWorldPos = tilemap.CellToWorld(cellPosition);

                        // 타일 중심점으로 조정 (CellToWorld는 셀의 왼쪽 아래 모서리를 반환)
                        Vector3 tileCenterPos = tileWorldPos + new Vector3(TileSize * 0.5f, TileSize * 0.5f, 0);

                        // 플레이어와의 거리 체크 (성능 최적화)
                        if (Vector2.Distance(playerPos, tileCenterPos) <= searchRange)
                        {
                            ObstacleData tileObstacle = new ObstacleData
                            {
                                Position = tileCenterPos,
                                Size = new Vector2(TileSize, TileSize),
                                Type = ObstacleData.ShapeType.Box // Box로 처리
                            };

                            m_cachedObstacles.Add(tileObstacle);
                        }
                    }
                }
            }
        }
    }

    void SetupVisualization()
    {
        m_meshFilter = gameObject.AddComponent<MeshFilter>();
        m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
        m_meshRenderer.materials = m_fovEdgeEnabled ? 
            new Material[2] { m_fovMainMaterial, m_fovEdgeMaterial } :
            new Material[1] { m_fovMainMaterial };

        CreateFOVMesh();
    }

    void CreateFOVMesh()
    {
        m_fovMesh = new Mesh();

        Vector3[] vertices = new Vector3[m_rayCount + 1];
        Vector2[] uvs = new Vector2[m_rayCount + 1]; // UV 좌표 추가
        int[] triangles = new int[m_rayCount * 3];

        vertices[0] = Vector3.zero; // 중심점
        uvs[0] = new Vector2(0.5f, 0.5f); // 중심점 UV (0.5, 0.5)

        for (int i = 0; i < m_rayCount; ++i)
        {
            float angle;
            if (m_fovType == FOVType.Circle)
            {
                angle = (360f / m_rayCount) * i * Mathf.Deg2Rad;
            }
            else
            {
                float startAngle = (-m_coneAngle * 0.5f + transform.eulerAngles.z) * Mathf.Deg2Rad;
                angle = startAngle + (m_coneAngle * Mathf.Deg2Rad / (m_rayCount - 1)) * i;
            }

            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * m_range, Mathf.Sin(angle) * m_range, 0);

            // UV 좌표 계산: 중심(0.5, 0.5)에서 가장자리(0~1)로
            float uvX = 0.5f + (Mathf.Cos(angle) * 0.5f);
            float uvY = 0.5f + (Mathf.Sin(angle) * 0.5f);
            uvs[i + 1] = new Vector2(uvX, uvY);

            // 삼각형 생성
            if (i < m_rayCount - 1)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            else if (m_fovType == FOVType.Circle) // 원형일 때만 마지막 삼각형 연결
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = 1;
            }
        }

        m_fovMesh.vertices = vertices;
        m_fovMesh.uv = uvs;
        m_fovMesh.triangles = triangles;
        m_fovMesh.RecalculateNormals();

        m_meshFilter.mesh = m_fovMesh;
    }

    void UpdateVisualization()
    {
        if (m_fovMesh == null) return;

        // Compute Shader 결과 가져오기
        RayData[] rayResults = new RayData[m_rayCount];
        m_rayBuffer.GetData(rayResults);

        // 메시 업데이트 (vertices만 업데이트, UV는 고정)
        Vector3[] vertices = new Vector3[m_rayCount + 1];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < m_rayCount; ++i)
        {
            Vector3 direction = new Vector3(rayResults[i].Direction.x, rayResults[i].Direction.y, 0);
            vertices[i + 1] = direction * rayResults[i].HitDistance;
        }

        m_fovMesh.vertices = vertices;
        m_fovMesh.RecalculateBounds();
    }

    // 범위 내 체크 (다른 스크립트에서 사용)
    public bool IsInRange(Vector3 targetPos)
    {
        Vector2 dirToTarget = (targetPos - transform.position);
        float distance = dirToTarget.magnitude;

        if (distance > m_range) return false;

        if (m_fovType == FOVType.Cone)
        {
            Vector2 forward = new Vector2(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad),
                                         Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad));
            float angle = Vector2.Angle(forward, dirToTarget.normalized);
            if (angle > m_coneAngle * 0.5f) return false;
        }

        // 캐싱된 장애물로 체크
        return !IsBlocked(transform.position, targetPos);
    }

    // 두 점 사이가 장애물에 막혀있는지 확인
    private bool IsBlocked(Vector3 start, Vector3 end)
    {
        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);

        foreach (var obstacle in m_cachedObstacles)
        {
            if (RayBoxIntersect(start, direction, obstacle.Position, obstacle.Size, distance))
            {
                return true;
            }
        }

        return false;
    }

    // Box와 레이의 교차점 검사 (간단한 버전)
    private bool RayBoxIntersect(Vector3 rayOrigin, Vector2 rayDir, Vector2 boxCenter, Vector2 boxSize, float maxDistance)
    {
        Vector2 rayOrigin2D = new Vector2(rayOrigin.x, rayOrigin.y);
        Vector2 boxMin = boxCenter - boxSize * 0.5f;
        Vector2 boxMax = boxCenter + boxSize * 0.5f;

        // AABB와 레이의 교차 검사
        Vector2 invDir = new Vector2(
            Mathf.Abs(rayDir.x) > 0.0001f ? 1f / rayDir.x : float.MaxValue,
            Mathf.Abs(rayDir.y) > 0.0001f ? 1f / rayDir.y : float.MaxValue
        );

        Vector2 t1 = Vector2.Scale((boxMin - rayOrigin2D), invDir);
        Vector2 t2 = Vector2.Scale((boxMax - rayOrigin2D), invDir);

        Vector2 tMin = Vector2.Min(t1, t2);
        Vector2 tMax = Vector2.Max(t1, t2);

        float tNear = Mathf.Max(tMin.x, tMin.y);
        float tFar = Mathf.Min(tMax.x, tMax.y);

        return tNear <= tFar && tFar >= 0.0f && tNear <= maxDistance;
    }

    // 수동으로 장애물 재수집 (디버깅용)
    [ContextMenu("Force Refresh Obstacles")]
    public void ForceRefreshObstacles()
    {
        CollectAllObstacles();
    }

    void OnDestroy()
    {
        m_rayBuffer?.Release();
        m_obstacleBuffer?.Release();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_range);

        if (m_fovType == FOVType.Cone)
        {
            float leftAngle = (transform.eulerAngles.z - m_coneAngle * 0.5f) * Mathf.Deg2Rad;
            float rightAngle = (transform.eulerAngles.z + m_coneAngle * 0.5f) * Mathf.Deg2Rad;

            Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle), 0) * m_range;
            Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle), 0) * m_range;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, leftDir);
            Gizmos.DrawRay(transform.position, rightDir);
        }

        // 수집된 장애물들 표시 (디버깅용)
        if (Application.isPlaying && m_cachedObstacles != null)
        {
            Gizmos.color = Color.red;
            foreach (var obstacle in m_cachedObstacles)
            {
                switch (obstacle.Type)
                {
                    case ObstacleData.ShapeType.Box:
                        Gizmos.DrawWireCube(obstacle.Position, obstacle.Size);
                        break;
                    case ObstacleData.ShapeType.Circle:
                        Gizmos.DrawWireSphere(obstacle.Position, obstacle.Size.x * 0.5f);
                        break;
                    case ObstacleData.ShapeType.Polygon:
                        break;
                }
            }
        }
    }
}