using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public bool isStunned { get; set; }

    

    public void ApplyStun(float _stunTime)
    {
        isStunned = true;

        Color a = spriteRenderer.color;
        a.a = 0.5f;
        spriteRenderer.color = a;

        Invoke(nameof(StunRecovery), _stunTime);
    }

    private void StunRecovery()
    {
        isStunned = false;

        Color a = spriteRenderer.color;
        a.a = 1f;
        spriteRenderer.color = a;
    }
}
