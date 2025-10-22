using UnityEngine;




[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class MatchBaseCamera : MonoBehaviour
{
    public Camera baseCamera;
    private Camera m_overlayCamera;

    private void Awake()
    {
        baseCamera = baseCamera = transform.parent ? transform.parent.GetComponentInParent<Camera>() : null;
        m_overlayCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (baseCamera == null) 
        {
            return;
        }

        // Orthographic 모드일 경우
        if (baseCamera.orthographic && 
            m_overlayCamera.orthographicSize != baseCamera.orthographicSize)
        {
            m_overlayCamera.orthographicSize = baseCamera.orthographicSize;
        }
        else
        {
            // Perspective 모드일 경우
            m_overlayCamera.fieldOfView = baseCamera.fieldOfView;
        }

        // 카메라 위치/회전도 일치시키고 싶다면
        m_overlayCamera.transform.position = baseCamera.transform.position;
        m_overlayCamera.transform.rotation = baseCamera.transform.rotation;
    }
}
