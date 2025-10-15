using System;
using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;




public class CinemachineSequenceTrigger : MonoBehaviour, Respawnable
{
    public enum EasingType
    {
        Linear,
        SmoothStep,
        EaseInOutCubic,
        EaseInOutQuad
    }

    public CinemachineCamera CM_Camera;
    public BoxCollider2D bounds2D;

    [Header("이동")]
    public EasingType easingType = EasingType.EaseInOutQuad;
    public float pathDuration = 2f;
    public float startDelay = 0f;
    public float endDelay = 1f;

    [MyReadOnly]
    public bool isSequencePlaying = false;
    [MyReadOnly]
    public bool cameraMoveStart = true;

    private bool m_isInitialized = false;
    private CinemachineBrain m_cinemachineBrain;
    private CinemachineCamera m_currentCamera;
    private CinemachineSplineDolly m_splineDolly;




    private void Awake()
    {

    }

    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        if (false == m_isInitialized)
        {
            m_cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

            if (CM_Camera == null)
            {
                CM_Camera = GetComponentInChildren<CinemachineCamera>();
            }

            CM_Camera.Priority = 0;
            CM_Camera.enabled = false;
            m_splineDolly = CM_Camera.GetComponent<CinemachineSplineDolly>();

            if (m_splineDolly != null)
            {
                for (int i = 0; i < m_splineDolly.Spline.Splines[0].Knots.Count(); i++)
                {
                    var knot = m_splineDolly.Spline.Splines[0].Knots.ElementAt(i);
                    knot.Position = new Vector3(knot.Position.x, knot.Position.y, -10f);
                    m_splineDolly.Spline.Splines[0].SetKnot(i, knot);
                }
            }

            m_isInitialized = true;
        }

        isSequencePlaying = false;

        if (CM_Camera != null)
        {
            CM_Camera.Priority = 0;
            CM_Camera.enabled = false;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSequencePlaying)
            return;

        if (collision.CompareTag("Player"))
        {
            StartCoroutine(PlayCinemachineSequence());
        }
    }

    private IEnumerator PlayCinemachineSequence()
    {
        isSequencePlaying = true;
        cameraMoveStart = true;

        GUIManager.Instance.SetHUDActive(false);
        LevelManager.Instance.player.isStunned = true;

        if (CM_Camera != null)
        {
            CM_Camera.Priority = 100;
            CM_Camera.enabled = true;
        }

        if (m_splineDolly != null)
        {
            m_splineDolly.CameraPosition = 0f;
        }

        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        float timer = 0f;
        while (timer < pathDuration)
        {
            if (Time.timeScale > 0f)
            {
                timer += Time.deltaTime;
            }

            float normalizedTime = Mathf.Clamp01(timer / pathDuration);
            float smoothedTime = ApplySmoothingCurve(normalizedTime);

            m_splineDolly.CameraPosition = Mathf.Lerp(m_splineDolly.CameraPosition, smoothedTime, 5f * Time.deltaTime);

            yield return null;
        }

        cameraMoveStart = false;

        if (endDelay > 0f)
        {
            yield return new WaitForSeconds(endDelay);
        }

        GUIManager.Instance.SetHUDActive(true);
        LevelManager.Instance.player.isStunned = false;


        if (CM_Camera != null)
        {
            CM_Camera.Priority = 0;
            CM_Camera.enabled = false;
        }
    }

    private float ApplySmoothingCurve(float t)
    {
        switch (easingType)
        {
            case EasingType.SmoothStep:
                return SmoothStep(t);
            case EasingType.EaseInOutCubic:
                return EaseInOutCubic(t);
            case EasingType.EaseInOutQuad:
                return EaseInOutQuad(t);
        }

        return t;
    }

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }

    public void OnPlayerRespawn(CheckPoint _checkPoint, PlayerMovement _player)
    {
        Initialization();
    }

    protected virtual void OnEnable()
    {
        Initialization();
    }

    protected virtual void OnDisable()
    {
        
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (bounds2D != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(bounds2D.bounds.center, bounds2D.bounds.size);
        }
    }
#endif
}
