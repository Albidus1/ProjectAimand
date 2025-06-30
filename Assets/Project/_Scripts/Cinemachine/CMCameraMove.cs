using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CMCameraMove : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;

    [SerializeField] private GameObject virtualCamera;
    [SerializeField] private CinemachineBrain cmBrain;

    private bool isSwitching;
    private CinemachineConfiner2D m_confiner;


    private void Start()
    {
        Initialization();
    }

    public void Initialization()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerMovement>();
        }

        if (cmBrain == null)
        {
            cmBrain = FindAnyObjectByType<CinemachineBrain>();
        }

        if (player != null && virtualCamera == null)
        {
            virtualCamera = transform.GetChild(0).gameObject;

            var cam = virtualCamera.GetComponent<CinemachineCamera>();
            cam.Target.TrackingTarget = player.transform;
            virtualCamera.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger && false == isSwitching)
        {
            //StartCoroutine(nameof(SwitchCamera), true);
            m_confiner.BoundingShape2D = GetComponent<BoxCollider2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger && false == isSwitching)
        {
            //StartCoroutine(nameof(SwitchCamera), false);
        }
    }

    private void OnDisable()
    {
        StopCoroutine(nameof(SwitchCamera));
    }


    private IEnumerator SwitchCamera(bool _enable)
    {
        isSwitching = true;

        Time.timeScale = 0;

        if (player != null)
        {
            player.enabled = false;
        }

        virtualCamera.SetActive(_enable);

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