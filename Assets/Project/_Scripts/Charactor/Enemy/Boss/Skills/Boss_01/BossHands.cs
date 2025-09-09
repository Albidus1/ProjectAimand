using System.Collections;
using UnityEngine;

public class BossHands : MonoBehaviour
{
    public GameObject rightHand;
    public GameObject leftHand;

    public bool isAbilityActive { get; set; }
    public bool isOnCollision { get; set; }
    
    public bool hitPlayer { get; private set; }


    public Collider2D rightCollider2D;
    public Collider2D leftCollider2D;
    private bool m_isOnCollision;


    protected void Awake()
    {

    }

    protected void Start()
    {
        isAbilityActive = false;
        isOnCollision = false;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOnCollision && collision.CompareTag("Player"))
        {
            isOnCollision = false;
            hitPlayer = true;
        }
    }
}
