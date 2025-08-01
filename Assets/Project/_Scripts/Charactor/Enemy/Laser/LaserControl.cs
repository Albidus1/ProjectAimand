using System;
using System.Collections.Generic;
using UnityEngine;

public class LaserControl : MonoBehaviour
{
    [Header("VFX")]
    public GameObject startVFX;
    public GameObject endVFX;

    private List<ParticleSystem> m_particles = new List<ParticleSystem>();
    private LineRenderer m_lineRenderer;
    private Vector2 m_direction;
    private Vector2 m_endPoint;


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

    private void LaserOnoff(bool _onoff)
    {
        m_lineRenderer.enabled = _onoff;
    }

    private void Update()
    {
        RotateLaser();
        UpdateLaser();
    }

    private void UpdateLaser()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, m_direction, 50f);

        m_endPoint =  hit ? m_endPoint = hit.point : (Vector2)transform.position + m_direction * 50f;

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
}
