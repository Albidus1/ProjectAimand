using System.Collections;
using UnityEngine;



//[ExecuteInEditMode]
public class ParallaxPlayer : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float _deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;


    private bool m_initialized;
    private float m_currentPositionX;
    private float m_previousPositionX;



    private void Awake()
    {

    }

    private void Start()
    {
        m_initialized = false;
        m_previousPositionX = transform.position.x;

        StartCoroutine(SetPositionAfter2Frams());
    }

    private IEnumerator SetPositionAfter2Frams()
    {
        yield return null;
        yield return null;

        m_initialized = true;
        //m_previousPositionX = transform.position.x;
    }

    private void LateUpdate()
    {
        if (false == m_initialized)
        {
            return;
        }

        m_currentPositionX = transform.position.x;

        HandleCameraTranslation();

        m_previousPositionX = m_currentPositionX;
    }

    private void HandleCameraTranslation()
    {
        float deltaPositionX = m_previousPositionX - m_currentPositionX;

        if (Mathf.Abs(deltaPositionX) < 0.1f)
        {
            return;
        }

        if (onCameraTranslate != null)
        {
            //Debug.Log(m_currentPositionX + " " + m_previousPositionX);
            onCameraTranslate(deltaPositionX);
        }
    }
}
