using UnityEngine;

public class PlayerMagneticController : MonoBehaviour
{
    public Camera ObstacleCamera;
    public ComputeShader computeShader;
    public RenderTexture obstacleMap;  // ObstacleCamera가 출력한 RT
    public RenderTexture resultTexture;
    public Vector2 center;
    public float range = 50;
    public float stepSize = 1;
    public int maxSteps = 100;

    void Start()
    {
        //resultTexture = new RenderTexture(256, 256, 0)
        //{
        //    enableRandomWrite = true,
        //    format = RenderTextureFormat.R8
        //};
        //resultTexture.Create();
    }

    void Update()
    {
        int kernel = computeShader.FindKernel("MagneticFOV");

        Vector3 worldCenter = transform.position;
        Vector3 viewport = ObstacleCamera.WorldToViewportPoint(worldCenter);
        Vector2 texCoord = new Vector2(viewport.x * resultTexture.width, viewport.y * resultTexture.height);

        computeShader.SetTexture(kernel, "ObstacleMap", obstacleMap);
        computeShader.SetTexture(kernel, "Result", resultTexture);
        computeShader.SetVector("_Center", texCoord);
        computeShader.SetFloat("_Range", range);
        computeShader.SetFloat("_StepSize", stepSize);
        computeShader.SetInt("_MaxSteps", maxSteps);

        computeShader.Dispatch(kernel, resultTexture.width / 8, resultTexture.height / 8, 1);
    }
}
