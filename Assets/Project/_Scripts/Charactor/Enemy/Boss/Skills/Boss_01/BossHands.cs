using UnityEngine;

public class BossHands : MonoBehaviour
{
    public GameObject rightHand;
    public GameObject leftHand;

    public bool isAbilityActive { get; set; }
    public bool isOnCollision { get; set; }
    public bool hitPlayer { get; private set; }


    protected void Awake()
    {
        isAbilityActive = false;
        isOnCollision = false;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOnCollision && collision.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} - {collision.gameObject.name}에게 피해");
            isOnCollision = false;
            hitPlayer = true;
        }
    }
}
