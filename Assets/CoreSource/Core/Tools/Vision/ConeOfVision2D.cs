using UnityEngine;
using System.Collections.Generic;
using System;



[Serializable]
public class ConeOfVision2D : MonoBehaviour
{
    public struct RaycastData
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public RaycastData(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }

    public struct MeshEdgePosition
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public MeshEdgePosition(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }



    [Header("범위")]
    public LayerMask obstacleMask;
    public float visionRadius = 5;
    [Range(0, 360)]
    public float visionAngle = 180;

    [MyReadOnly]
    [Range(0, 360)]
    public float angleOffset = 0;
    [MyReadOnly]
    public Vector3 direction;
    [MyReadOnly]
    public Vector3 eulerAngles;

    [Header("오브젝트 감지")]
    public bool shouldScanForTargets = true;
    public LayerMask targetMask;
    public float scanFrequencyInSeconds = 1;
    [MyReadOnly]
    public List<Transform> visibleTargets = new List<Transform>();

    [Header("메시")]
    public bool shouldDrawMesh = true;
    public float meshDensity = 0.2f;    // 밀도
    public int edgePrecision = 3;       // 정밀도
    public float edgeThreshold = 0.5f;  // 임계값

    public MeshFilter visionMeshFilter;

    protected Mesh visionMesh;
    protected Collider2D[] targetsWithinDistance;
    protected Transform target;
    protected Vector3 directionToTarget;
    protected float distanceToTarget;
    protected float lastScanTime;

    protected RaycastHit2D scanForTargetsHit2D;
    protected List<Vector3> viewPoints = new List<Vector3>();
    protected RaycastData oldViewCast = new RaycastData();
    protected RaycastData viewCast = new RaycastData();

    protected Vector3[] vertices;
    protected int[] triangles;
    protected Vector3 minPoint, maxPoint, dir;
    protected RaycastData returnRaycastData;
    protected RaycastHit2D raycastAtAngleHit2D;
    protected int numOfVerticesLastTime = 0;



    protected virtual void Awake()
    {
        visionMesh = new Mesh();
        direction = Vector3.right;

        visionMeshFilter = transform.GetComponentInChildren<MeshFilter>();
        visionMeshFilter.mesh = visionMesh;
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void LateUpdate()
    {
        if ((Time.time - lastScanTime > scanFrequencyInSeconds) && shouldScanForTargets)
        {
            ScanForTargets();
        }

        DrawMesh();
    }

    public virtual void SetDirectionAndAngles(Vector3 _direction, Vector3 _eulerAngles)
    {
        direction = _direction;
        eulerAngles = _eulerAngles;
        eulerAngles.y += angleOffset;
    }

    protected virtual void ScanForTargets()
    {
        lastScanTime = Time.time;
        visibleTargets.Clear();
        targetsWithinDistance = Physics2D.OverlapCircleAll(this.transform.position, visionRadius, targetMask);

        foreach (Collider2D col in targetsWithinDistance)
        {
            target = col.transform;
            directionToTarget = (target.position - this.transform.position).normalized;

            if (Vector3.Angle(direction, directionToTarget) < visionAngle * 0.5f)
            {
                distanceToTarget = Vector3.Distance(this.transform.position, target.position);

                scanForTargetsHit2D = Physics2D.Raycast(this.transform.position, directionToTarget, distanceToTarget, obstacleMask);

                if (false == scanForTargetsHit2D)
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    protected virtual void DrawMesh()
    {
        if (false == shouldDrawMesh)
        {
            if (viewPoints.Count > 0)
            {
                viewPoints.Clear();
            }
            if (visionMesh != null)
            {
                visionMesh.Clear();
            }

            return;
        }

        int steps = Mathf.RoundToInt(meshDensity * visionAngle);
        float stepsAngle = visionAngle / steps;

        viewPoints.Clear();

        for (int i = 0; i <= steps; i++)
        {
            float angle = (stepsAngle * i) + eulerAngles.y - (visionAngle * 0.5f);
            viewCast = RaycastAtAngle(angle);

            if (i > 0)
            {
                bool thresholdExeeded = Mathf.Abs(oldViewCast.distance -  viewCast.distance) > edgeThreshold;

                if ((oldViewCast.hit != viewCast.hit) || (oldViewCast.hit && viewCast.hit && true == thresholdExeeded))
                {
                    MeshEdgePosition edge = FindMeshEdgePosition(oldViewCast, viewCast);

                    if(edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }

                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(viewCast.point);
            oldViewCast = viewCast;
        }

        int numOfVertices = viewPoints.Count + 1;
        if (numOfVertices != numOfVerticesLastTime)
        {
            Array.Resize(ref vertices, numOfVertices);
            Array.Resize(ref triangles, (numOfVertices - 2) * 3);
        }

        vertices[0].x = 0;
        vertices[0].y = 0;
        vertices[0].z = 0;

        for (int i = 0; i < numOfVertices - 1; i++) 
        {
            vertices[i + 1] = this.transform.InverseTransformPoint(viewPoints[i]);

            if (i < numOfVertices - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();

        numOfVerticesLastTime = numOfVertices;
    }

    RaycastData RaycastAtAngle(float _angle)
    {
        dir = Vector3.zero;
        dir.x = Mathf.Cos(_angle * Mathf.Deg2Rad);
        dir.y = Mathf.Sin(_angle * Mathf.Deg2Rad);
        dir.z = 0f;

        raycastAtAngleHit2D = Physics2D.Raycast(this.transform.position, dir, visionRadius, obstacleMask);

#if UNITY_EDITOR
        //Debug.DrawRay(this.transform.position, dir * visionRadius, raycastAtAngleHit2D ? Color.red : Color.green, 0.1f);
#endif

        if (raycastAtAngleHit2D)
        {
            returnRaycastData.hit = true;
            returnRaycastData.point = raycastAtAngleHit2D.point;
            returnRaycastData.distance = raycastAtAngleHit2D.distance;
            returnRaycastData.angle = _angle;
        }
        else
        {
            returnRaycastData.hit = false;
            returnRaycastData.point = this.transform.position + (dir * visionRadius);
            returnRaycastData.distance = visionRadius;
            returnRaycastData.angle = _angle;
        }

        return returnRaycastData;
    }

    MeshEdgePosition FindMeshEdgePosition(RaycastData _minViewCast, RaycastData _maxViewCast)
    {
        float minAngle = _minViewCast.angle;
        float maxAngle = _maxViewCast.angle;
        minPoint = _minViewCast.point;
        maxPoint = _maxViewCast.point;

        for (int i = 0; i < edgePrecision; i++)
        {
            float angle = (minAngle + maxAngle) * 0.5f;
            RaycastData newViewCast = RaycastAtAngle(angle);

            bool thresholdExeeded = Mathf.Abs(_minViewCast.distance - newViewCast.distance) > edgeThreshold;

            if (newViewCast.hit == _minViewCast.hit && false == thresholdExeeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new MeshEdgePosition(minPoint, maxPoint);
    }
}
