using System;
using System.Collections.Generic;
using UnityEngine;

public class LaserControl : MonoBehaviour
{
    public bool laserOn = true;
    public bool isFiringLaser
    {
        get
        {
            return laserOn;
        }
        set
        {
            laserOn = value;
            if (laserOn)
            {
                LaserEnable();
            }
            else
            {
                LaserDisable();
            }
        }
    }


    [Header("레이 캐스트")]
    public int raycastCount = 1;

    [Header("VFX")]
    public GameObject startVFX;
    public GameObject endVFX;

    private List<ParticleSystem> m_particles = new List<ParticleSystem>();
    private LineRenderer m_lineRenderer;
    private Vector3 m_direction;
    private Vector3 m_endPoint;


    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();

        if (startVFX == null)
        {
            startVFX = transform.Find("StartVFX").gameObject;
        }
        if (endVFX == null)
        {
            endVFX = transform.Find("EndVFX").gameObject;
        }
    }

    private void Start()
    {
        //LaserOnoff(false);
        m_lineRenderer.useWorldSpace = true;

        FillVFXList();

        isFiringLaser = laserOn;
    }

    private void FillVFXList()
    {
        for (int i = 0; i < startVFX.transform.childCount; i++)
        {
            ParticleSystem ps = startVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null)
            {
                m_particles.Add(ps);
            }
        }

        for (int i = 0; i < endVFX.transform.childCount; i++)
        {
            ParticleSystem ps = endVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null)
            {
                m_particles.Add(ps);
            }
        }
    }

    private void Update()
    {
        RotateLaser();
        UpdateLaser();


        float force = (transform.position - m_endPoint).magnitude;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, m_direction, force, LayerManager.playerLayerMask);

        if (hit)
        {
            //Debug.Log("플레이어 감지됨: " + hit.collider.name);
        }
    }

    private void UpdateLaser()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, m_direction, 50f, LayerManager.obstacleLayerMask & ~LayerManager.onewayPlatformsLayerMask);

        m_endPoint =  hit ? m_endPoint = hit.point : transform.position + m_direction * 50f;

        m_lineRenderer.SetPosition(0, transform.position);
        m_lineRenderer.SetPosition(1, m_endPoint);

        startVFX.transform.position = transform.position;
        endVFX.transform.position = m_endPoint;
    }

    private void RotateLaser()
    {
        Vector3 rotateAngle = new Vector3(0, 0, 1);
        transform.parent.Rotate(rotateAngle, 30f * Time.deltaTime);

        Quaternion rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);
        m_direction = rotation * Vector2.right;
    }

    private void LaserEnable()
    {
        m_lineRenderer.enabled = true;

        foreach (ParticleSystem ps in m_particles)
        {
            if (ps != null && !ps.isPlaying)
            {
                ps.Play();
            }
        }
    }

    private void LaserDisable()
    {
        m_lineRenderer.enabled = false;

        foreach (ParticleSystem ps in m_particles)
        {
            if (ps != null && ps.isPlaying)
            {
                ps.Stop();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        for (int i = 0; i < raycastCount; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);
            m_direction = rotation * Vector2.right;

            float force = Application.isPlaying ? (transform.position - m_endPoint).magnitude : 3f;

            Vector3 start = transform.position;
            Vector3 end = start + m_direction * force;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(start, end);
        }
    }

    private void OnValidate()
    {
        if (false == Application.isPlaying)
            return;

        if (m_lineRenderer != null)
        {
            isFiringLaser = laserOn;
        }
    }
#endif
}
