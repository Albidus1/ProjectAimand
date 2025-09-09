using System.Collections;
using UnityEngine;

public class BossHands : MonoBehaviour
{
    public GameObject rightHand;
    public GameObject leftHand;

    public bool isAbilityActive { get; set; }
    public bool isOnCollision 
    {
        get => m_isOnCollision;
        set 
        {
            m_isOnCollision = value;

            if (m_damageOnTouch != null)
            {
                if (value == false)
                {
                    Invoke(nameof(DisableDamageOnTouch), 0.1f);
                }
                else
                {
                    m_damageOnTouch.enabled = true;
                }
            }
        }
    }
    
    public bool hitPlayer { get; private set; }


    private DamageOnTouch m_damageOnTouch;
    private bool m_isOnCollision;


    protected void Awake()
    {
        m_damageOnTouch = GetComponent<DamageOnTouch>();
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

    private void DisableDamageOnTouch()
    {
        if (m_damageOnTouch != null)
        {
            m_damageOnTouch.enabled = false;
        }
    }
}
