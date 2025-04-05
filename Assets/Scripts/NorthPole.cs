using UnityEngine;

public class NorthPole : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.red; // 🔴 N극 = 빨강
    }
}
