using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CMCameraMove : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    public GameObject cameraTarget;

    public CinemachineBrain cmBrain;
    public CinemachineCamera cinemachineCamera;
    public CinemachineConfiner2D confiner;

    private bool isSwitching;



    private void Awake()
    {
        Initialization();
    }

    public void Initialization()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerMovement>();
        }

        if (cinemachineCamera == null)
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = cameraTarget != null ? cameraTarget.transform : player.transform;
        }

        if (cmBrain == null)
        {
            cmBrain = FindAnyObjectByType<CinemachineBrain>();
        }

        confiner = GetComponent<CinemachineConfiner2D>();
    }

    private void OnDisable()
    {
        
    }


    private IEnumerator SwitchCamera(bool _enable)
    {
        isSwitching = true;

        Time.timeScale = 0;

        if (player != null)
        {
            player.enabled = false;
        }

        //virtualCamera.SetActive(_enable);

        float blend_time = cmBrain.ActiveBlend != null ? cmBrain.ActiveBlend.Duration : 0;
        float elapsed_time = 0;

        while (elapsed_time < blend_time)
        {
            cmBrain.ManualUpdate();
            elapsed_time += Time.unscaledDeltaTime;
            yield return null;
        }

        if (player != null)
        {
            player.enabled = true;
        }

        Time.timeScale = 1;

        isSwitching = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //Collider2D collision = GetComponent<Collider2D>();
        //Vector3 pos = collision.bounds.center;

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireCube(pos, collision.bounds.size);

    }
#endif
}