using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MagneticVisualization : MonoBehaviour
{
    [Header("FOV Settings")]
    public FOVType fovType = FOVType.Circle;
    public float range = 8f;
    public float coneAngle = 90f;
    public int rayCount = 128;

    [Header("Visualization")]
    public bool FOVEdgeEnabled = false;
    public Material FOVMainMaterial;
    public Material FOVEdgeMaterial;
    public LayerMask obstacleLayer = -1;

    [Header("Tilemap Support")]
    public Tilemap[] obstacleTilemaps;
    public float tileSize = 1f;
    [Tooltip("타일맵 검사할 추가 범위 (성능 최적화)")]
    public float tilemapSearchPadding = 2f;

    // Compute Shader 관련
    [Header("Compute Shader")]
    [SerializeField]
    private ComputeShader fovComputeShader;
    private ComputeBuffer rayBuffer;
    private ComputeBuffer obstacleBuffer;

    // 시각화 관련
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh fovMesh;

    // 성능 최적화를 위한 캐싱
    private List<ObstacleData> cachedObstacles = new List<ObstacleData>();
    private Vector3 lastPlayerPosition;
    private const float POSITION_THRESHOLD = 0.1f; // 위치 변화 임계값

    public enum FOVType { Circle, Cone }

    // 데이터 구조체
    struct RayData
    {
        public Vector2 direction;
        public float maxDistance;
        public float hitDistance;
    }

    struct ObstacleData
    {
        public Vector2 position;
        public Vector2 size;
        public int shapeType; // 0: Box, 1: Circle
    }

    void Start()
    {
        InitializeFOV();
        SetupVisualization();
        lastPlayerPosition = transform.position;
    }

    void Update()
    {
        UpdateFOV();
        UpdateVisualization();
    }

    void InitializeFOV()
    {
        // 버퍼 생성 (더 많은 장애물을 처리할 수 있도록 확장)
        rayBuffer = new ComputeBuffer(rayCount, sizeof(float) * 4);
        obstacleBuffer = new ComputeBuffer(200, sizeof(float) * 5); // 타일맵 포함해서 더 많은 장애물 처리

        // 레이 데이터 초기화
        InitializeRays();
    }

    void InitializeRays()
    {
        RayData[] rays = new RayData[rayCount];

        for (int i = 0; i < rayCount; ++i)
        {
            float angle;

            if (fovType == FOVType.Circle)
            {
                // 원형 - 360도
                angle = (360f / rayCount) * i;
            }
            else
            {
                // 원뿔형 - 지정된 각도 범위
                float startAngle = -coneAngle * 0.5f;
                angle = startAngle + (coneAngle / (rayCount - 1)) * i;

                // 플레이어 방향 추가
                angle += transform.eulerAngles.z;
            }

            float radians = angle * Mathf.Deg2Rad;
            rays[i] = new RayData
            {
                direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)),
                maxDistance = range,
                hitDistance = range
            };
        }

        rayBuffer.SetData(rays);
    }

    void UpdateFOV()
    {
        if (fovComputeShader == null) return;

        // 방향이 바뀌면 레이 재계산 (Cone 모드에서)
        if (fovType == FOVType.Cone)
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
        int kernel = fovComputeShader.FindKernel("MagneticFOV");

        fovComputeShader.SetVector("_PlayerPos", transform.position);
        fovComputeShader.SetFloat("_Range", range);
        fovComputeShader.SetInt("_RayCount", rayCount);
        fovComputeShader.SetInt("_ObstacleCount", cachedObstacles.Count);

        fovComputeShader.SetBuffer(kernel, "_RayBuffer", rayBuffer);
        fovComputeShader.SetBuffer(kernel, "_ObstacleBuffer", obstacleBuffer);

        // 장애물 데이터 업데이트
        if (cachedObstacles.Count > 0)
        {
            obstacleBuffer.SetData(cachedObstacles.ToArray());
        }

        int threadGroups = Mathf.CeilToInt(rayCount / 64f);
        fovComputeShader.Dispatch(kernel, threadGroups, 1, 1);
    }

    void CollectAllObstacles()
    {
        cachedObstacles.Clear();

        // 1. 일반 콜라이더 수집
        CollectRegularObstacles();

        // 2. 타일맵을 Box로 변환해서 수집
        CollectTilemapAsBoxes();

        //Debug.Log($"Total obstacles collected: {cachedObstacles.Count}");
    }

    void CollectRegularObstacles()
    {
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(transform.position, range * 1.5f, obstacleLayer);

        foreach (var obstacle in obstacles)
        {
            // TilemapCollider2D는 별도 처리하므로 건너뛰기
            if (obstacle is TilemapCollider2D) continue;

            ObstacleData data = new ObstacleData
            {
                position = obstacle.transform.position,
                size = obstacle.bounds.size,
                shapeType = obstacle is BoxCollider2D ? 0 : 1
            };

            cachedObstacles.Add(data);
        }
    }

    void CollectTilemapAsBoxes()
    {
        if (obstacleTilemaps == null || obstacleTilemaps.Length == 0) return;

        Vector3 playerPos = transform.position;
        float searchRange = range + tilemapSearchPadding;

        foreach (Tilemap tilemap in obstacleTilemaps)
        {
            if (tilemap == null) continue;

            // 플레이어 주변의 타일 영역만 검사
            Vector3Int playerCellPos = tilemap.WorldToCell(playerPos);
            int cellRange = Mathf.CeilToInt(searchRange / tileSize);

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
                        Vector3 tileCenterPos = tileWorldPos + new Vector3(tileSize * 0.5f, tileSize * 0.5f, 0);

                        // 플레이어와의 거리 체크 (성능 최적화)
                        if (Vector2.Distance(playerPos, tileCenterPos) <= searchRange)
                        {
                            ObstacleData tileObstacle = new ObstacleData
                            {
                                position = tileCenterPos,
                                size = new Vector2(tileSize, tileSize),
                                shapeType = 0 // Box로 처리
                            };

                            cachedObstacles.Add(tileObstacle);
                        }
                    }
                }
            }
        }
    }

    void SetupVisualization()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.materials = FOVEdgeEnabled ? 
            new Material[2] { FOVMainMaterial, FOVEdgeMaterial } :
            new Material[1] { FOVMainMaterial };

        CreateFOVMesh();
    }

    void CreateFOVMesh()
    {
        fovMesh = new Mesh();

        Vector3[] vertices = new Vector3[rayCount + 1];
        Vector2[] uvs = new Vector2[rayCount + 1]; // UV 좌표 추가
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // 중심점
        uvs[0] = new Vector2(0.5f, 0.5f); // 중심점 UV (0.5, 0.5)

        for (int i = 0; i < rayCount; ++i)
        {
            float angle;
            if (fovType == FOVType.Circle)
            {
                angle = (360f / rayCount) * i * Mathf.Deg2Rad;
            }
            else
            {
                float startAngle = (-coneAngle * 0.5f + transform.eulerAngles.z) * Mathf.Deg2Rad;
                angle = startAngle + (coneAngle * Mathf.Deg2Rad / (rayCount - 1)) * i;
            }

            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * range, Mathf.Sin(angle) * range, 0);

            // UV 좌표 계산: 중심(0.5, 0.5)에서 가장자리(0~1)로
            float uvX = 0.5f + (Mathf.Cos(angle) * 0.5f);
            float uvY = 0.5f + (Mathf.Sin(angle) * 0.5f);
            uvs[i + 1] = new Vector2(uvX, uvY);

            // 삼각형 생성
            if (i < rayCount - 1)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            else if (fovType == FOVType.Circle) // 원형일 때만 마지막 삼각형 연결
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = 1;
            }
        }

        fovMesh.vertices = vertices;
        fovMesh.uv = uvs;
        fovMesh.triangles = triangles;
        fovMesh.RecalculateNormals();

        meshFilter.mesh = fovMesh;
    }

    void UpdateVisualization()
    {
        if (fovMesh == null) return;

        // Compute Shader 결과 가져오기
        RayData[] rayResults = new RayData[rayCount];
        rayBuffer.GetData(rayResults);

        // 메시 업데이트 (vertices만 업데이트, UV는 고정)
        Vector3[] vertices = new Vector3[rayCount + 1];
        vertices[0] = Vector3.zero;

        for (int i = 0; i < rayCount; ++i)
        {
            Vector3 direction = new Vector3(rayResults[i].direction.x, rayResults[i].direction.y, 0);
            vertices[i + 1] = direction * rayResults[i].hitDistance;
        }

        fovMesh.vertices = vertices;
        fovMesh.RecalculateBounds();
    }

    // 범위 내 체크 (다른 스크립트에서 사용)
    public bool IsInRange(Vector3 targetPos)
    {
        Vector2 dirToTarget = (targetPos - transform.position);
        float distance = dirToTarget.magnitude;

        if (distance > range) return false;

        if (fovType == FOVType.Cone)
        {
            Vector2 forward = new Vector2(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad),
                                         Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad));
            float angle = Vector2.Angle(forward, dirToTarget.normalized);
            if (angle > coneAngle * 0.5f) return false;
        }

        // 캐싱된 장애물로 체크
        return !IsBlocked(transform.position, targetPos);
    }

    // 두 점 사이가 장애물에 막혀있는지 확인
    private bool IsBlocked(Vector3 start, Vector3 end)
    {
        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);

        foreach (var obstacle in cachedObstacles)
        {
            if (RayBoxIntersect(start, direction, obstacle.position, obstacle.size, distance))
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
        rayBuffer?.Release();
        obstacleBuffer?.Release();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);

        if (fovType == FOVType.Cone)
        {
            float leftAngle = (transform.eulerAngles.z - coneAngle * 0.5f) * Mathf.Deg2Rad;
            float rightAngle = (transform.eulerAngles.z + coneAngle * 0.5f) * Mathf.Deg2Rad;

            Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle), 0) * range;
            Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle), 0) * range;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, leftDir);
            Gizmos.DrawRay(transform.position, rightDir);
        }

        // 수집된 장애물들 표시 (디버깅용)
        if (Application.isPlaying && cachedObstacles != null)
        {
            Gizmos.color = Color.red;
            foreach (var obstacle in cachedObstacles)
            {
                if (obstacle.shapeType == 0) // Box
                {
                    Gizmos.DrawWireCube(obstacle.position, obstacle.size);
                }
                else // Circle
                {
                    Gizmos.DrawWireSphere(obstacle.position, obstacle.size.x * 0.5f);
                }
            }
        }
    }
}