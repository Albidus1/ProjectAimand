using System.Collections;
using UnityEngine;

public class BonusDashItem : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private Sprite spriteUnused;
    public Sprite spriteUsed;

    [Header("충전 시간")]
    private bool isRefilling;
    public float refillTime;



    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteUnused = GetComponent<SpriteRenderer>().sprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<PlayerMovement>();

            if (player.dashesLeft < player.data.dash_amount &&
                false == isRefilling)
            {
                Debug.Log("BonusDash");

                player.BonusDash();

                StartCoroutine(nameof(RefillItem), refillTime);
            }
        }
    }

    private IEnumerator RefillItem(float _timer)
    {
        isRefilling = true;
        spriteRenderer.sprite = spriteUsed;

        yield return new WaitForSeconds(_timer);

        isRefilling = false;
        spriteRenderer.sprite = spriteUnused;
    }
}
