using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public PlayerMovement playerController;
    public Animator animator;



    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerMovement>();
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
}
