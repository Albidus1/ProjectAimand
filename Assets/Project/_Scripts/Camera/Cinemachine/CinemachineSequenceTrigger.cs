using System;
using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;




public class CinemachineSequenceTrigger : MonoBehaviour
{
    public CinemachineCamera CM_Camera;
    public BoxCollider2D bounds2D;

    [Header("이동")]
    public float pathDuration = 2f;
    public float startDelay = 0f;
    public float endDelay = 1f;

    [MyReadOnly]
    public bool isSequencePlaying = false;

    private CinemachineBrain m_cinemachineBrain;
    private CinemachineCamera m_currentCamera;
    private CinemachineSplineDolly m_splineDolly;



    private void Awake()
    {

    }

    private void Start()
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

        //m_currentCamera = m_cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;


        if (CM_Camera != null)
        {
            CM_Camera.Priority = 100;
            CM_Camera.enabled = true;
        }

        if (m_splineDolly != null)
        {
            m_splineDolly.CameraPosition = 0f;
        }

        yield return new WaitForSeconds(startDelay);

        float timer = 0f;
        while (timer < pathDuration)
        {
            timer += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(timer / pathDuration);

            m_splineDolly.CameraPosition = normalizedTime;

            yield return null;
        }

        yield return new WaitForSeconds(endDelay);

        if (CM_Camera != null)
        {
            CM_Camera.Priority = 0;
            CM_Camera.enabled = false;
        }
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
