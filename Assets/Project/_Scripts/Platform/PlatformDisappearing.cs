using System.Collections;
using UnityEngine;

public class PlatformDisappearing : MonoBehaviour
{
    public bool isLeftChecking = true;
    public bool isRightChecking = true;

    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    private Sprite spriteUnBreak;
    public Sprite spriteBreak;
    [Space(5)]

    [Header("타이머")]
    public float breakTime;
    public float restoreTime;
    public float breakTimer { get; private set; }
    public bool isBreaking {  get; private set; }

    private readonly Vector3[] directions = {Vector3.left, Vector3.right};
    private readonly Vector3[] point = new Vector3[2];

    private RaycastHit2D leftHit;
    [SerializeField] private PlatformDisappearing left;
    private RaycastHit2D rightHit;
    [SerializeField] private PlatformDisappearing right;



    private void Awake()
    {
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteUnBreak = spriteRenderer.sprite;

        point[0] = transform.position + directions[0] * Mathf.Abs(transform.localScale.x * 0.51f);
        point[1] = transform.position + directions[1] * Mathf.Abs(transform.localScale.x * 0.51f);
    }

    private void Start()
    {
        InitializeBreaking(false);
    }

    private void Update()
    {
        bool leftBreaking = isLeftChecking && left != null && left.isBreaking;
        bool rightBreaking = isRightChecking && right != null && right.isBreaking;

        if (leftBreaking || rightBreaking)
        {
            isBreaking = true;
        }

        if (false == isBreaking)
        {
            return;
        }

        breakTimer -= Time.deltaTime;

        if (breakTimer < 0)
        {
            BreakPlatform();
            StartCoroutine(nameof(RestoreAfterDelay));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && false == isBreaking)
        {
            Debug.Log("Breaking");

            InitializeBreaking(true);        
        }
    }

    private void InitializeBreaking(bool _flag)
    {
        isBreaking = _flag;
        breakTimer = breakTime;

        CheckFriends();
    }

    private void CheckFriends()
    {
        if (true == isLeftChecking)
        {          
            leftHit = Physics2D.Raycast(point[0], directions[0], 0.001f);

            if (leftHit.collider != null && leftHit.collider.TryGetComponent<PlatformDisappearing>(out left))
            {
                left.isBreaking = isBreaking;
            }
            else
            {
                left = null;
            }
        }
        else
        {
            left = null;
        }

        if (true == isRightChecking)
        {           
            rightHit = Physics2D.Raycast(point[1], directions[1], 0.001f);

            if (rightHit.collider != null && rightHit.collider.TryGetComponent<PlatformDisappearing>(out right))
            {
                right.isBreaking = isBreaking;
            }
            else
            {
                right = null;
            }
        }
        else
        {
            right = null;
        }
    }

    private IEnumerator RestoreAfterDelay()
    {
        isBreaking = false;
        breakTimer = breakTime;
        yield return new WaitForSeconds(restoreTime);
        RestorePlatform();
    }

    private void RestorePlatform()
    {
        spriteRenderer.sprite = spriteUnBreak;
        col.enabled = true;
    }

    private void BreakPlatform()
    {
        spriteRenderer.sprite = spriteBreak;
        col.enabled = false;
    }

    #region EDITOR METHODS
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        point[0] = transform.position + directions[0] * Mathf.Abs(transform.localScale.x * 0.51f);
        point[1] = transform.position + directions[1] * Mathf.Abs(transform.localScale.x * 0.51f);

        CheckFriends();

        if (left != null)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color= Color.red;
        }
        Gizmos.DrawRay(transform.position, directions[0] * Mathf.Abs(transform.localScale.x * 0.51f));

        if (right != null)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color= Color.red;
        }
        Gizmos.DrawRay(transform.position, directions[1] * Mathf.Abs(transform.localScale.x * 0.51f));
#endif
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            CheckFriends();
        }
    }
    #endregion
}
