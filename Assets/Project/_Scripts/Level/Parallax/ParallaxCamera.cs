using Unity.Cinemachine;
using UnityEngine;



//[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float _deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;


    private CinemachineBrain m_brain; 

    private float m_currentPosition;
    private float m_previousPosition;



    private void Awake()
    {
        m_brain = GetComponent<CinemachineBrain>();
    }

    private void Start()
    {
        m_currentPosition = transform.position.x;
        m_previousPosition = m_currentPosition;
    }

    private void Update()
    {
        m_currentPosition = transform.position.x;

        if (m_brain != null && m_brain.IsBlending)
        {
            m_previousPosition = m_currentPosition;
            return;
        }

        HandleCameraTranslation();
        m_previousPosition = m_currentPosition;
    }

    private void HandleCameraTranslation()
    {
        if (m_currentPosition == m_previousPosition)
        {
            return;
        }

        if (onCameraTranslate != null)
        {
            float delta = m_previousPosition - m_currentPosition;
            onCameraTranslate(delta);
        }
    }
}
