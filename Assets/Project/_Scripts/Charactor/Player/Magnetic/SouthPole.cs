using UnityEngine;

public class SouthPole : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.blue; // 🔵 S극 = 파랑
    }
}
