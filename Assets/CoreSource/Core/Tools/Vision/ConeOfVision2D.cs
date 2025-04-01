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



    [Header("시야")]
    public LayerMask obstacleMask;
    public float visionRadius = 5;
    [Range(0, 360)]
    public float visionAngle = 180;
    [Range(0, 360)]
    public float angleOffset = 0;
    [MyReadOnly]
    public Vector3 direction;
    [MyReadOnly]
    public Vector3 eulerAngles;

    [Header("Target Scanning")]
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
    protected List<Vector3> viewPoint = new List<Vector3>();
    protected RaycastData oldViewCast = new RaycastData();
    protected RaycastData viewCast = new RaycastData();

    protected Vector3[] vertices;
    protected int[] triangles;
    protected Vector3 minPoint, maxPoint, dir;
    protected RaycastData returnRaycastData;
    protected RaycastHit2D raycastAtAngleHit2D;
    protected int numberOfVerticesLastTime = 0;



    protected virtual void Awake()
    {
        visionMesh = new Mesh();
        direction = Vector3.right;

        if (true == shouldDrawMesh)
        {
            visionMeshFilter.mesh = visionMesh;
        }
    }

    protected virtual void LateUpdate()
    {
        if ((Time.time - lastScanTime > scanFrequencyInSeconds) && true == shouldScanForTargets)
        {
            ScanForTargets();
        }

        DrawMesh();
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
            return;
        }

        int steps = Mathf.RoundToInt(meshDensity * visionAngle);
        float stepsAngle = visionAngle / steps;

        viewPoint.Clear();

        for (int i = 0; i <= steps; i++)
        {
            float angle = (stepsAngle * i) + eulerAngles.y - (visionAngle * 0.5f);
            viewCast = RaycastAtAngle(angle);
        }
    }

    RaycastData RaycastAtAngle(float _angle)
    {
        dir = Vector3.zero;
        dir.x = Mathf.Cos(_angle * Mathf.Deg2Rad);
        dir.y = Mathf.Sin(_angle * Mathf.Deg2Rad);
        dir.z = 0f;

        raycastAtAngleHit2D = Physics2D.Raycast(this.transform.position, dir, visionRadius, obstacleMask);

        if (raycastAtAngleHit2D)
        {
            returnRaycastData.hit = true;
            returnRaycastData.point = raycastAtAngleHit2D.point;
            returnRaycastData.distance = raycastAtAngleHit2D.distance;
            returnRaycastData.angle = _angle;
        }
        else
        {
            returnRaycastData.hit = true;
            returnRaycastData.point = this.transform.position + (dir * visionRadius);
            returnRaycastData.distance = visionRadius;
            returnRaycastData.angle = _angle;
        }

        return returnRaycastData;
    }
}
