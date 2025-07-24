using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("플레이어")]
    public LayerMask playerLayerMask;

    [Header("속도")]
    public float normalSpeed;
    public float chaseSpeed;





    public bool isFacingRight { get; protected set; }
    public bool isFalling { get; protected set; }
    public bool isAttacking { get; protected set; }
    public bool isStunned { get; set; }

    public float activityRange { get; set; }
    public float detectRange { get; set; }
    public float attackRange { get; set; }
    public float chaseWaitTime { get; set; }

    public GameObject target { get; protected set; }
    public Rigidbody2D rb { get; protected set; }
    public BoxCollider2D boxCollider { get; protected set; }

    protected int facingDirection = 1;
}
