using System.Linq;
using Unified.UniversalBlur.Runtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;




public class BlurCameraController : MonoBehaviour
{
    [Range(1, 12)]
    public int iterations = 4;
    [Range(1f, 10f)]
    public float downsample = 2.0f;

    [SerializeField] private Renderer2DData rendererData;

    private UniversalBlurFeature m_blurFeature;
    private Camera m_camera;


    private void Start()
    {
        if (rendererData != null)
        {
            m_blurFeature = rendererData.rendererFeatures
                .OfType<UniversalBlurFeature>()
                .FirstOrDefault();
        }

        m_camera = GetComponent<Camera>();

        MyFollowTarget follow = transform.AddComponent<MyFollowTarget>();
        follow.interpolatePosition = false;
        follow.target = Camera.main.transform;
    }

    private void Update()
    {
        if (Camera.main.orthographicSize != m_camera.orthographicSize)
        {
            m_camera.orthographicSize = Camera.main.orthographicSize;
        }
    }

    public void SetCustomBlur(int _iterations, float _downsample)
    {
        if (m_blurFeature != null)
        {
            //Debug.Log("Blur 설정 변경: " + _iterations + ", " + _downsample);
            m_blurFeature.Iterations = _iterations;
            m_blurFeature.Downsample = _downsample;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetCustomBlur(iterations, downsample);
    }
#endif
}
