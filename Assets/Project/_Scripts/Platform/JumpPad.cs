using System;
using UnityEngine;



public class JumpPad : MonoBehaviour, ISaveLoadManagerMethods
{
    private PlayerMovement player;

    public enum JumpDirection { Left, Right, Up };
    public JumpDirection direction;
    [MyReadOnly]
    public Vector3 dir;

    [Range(1.01f, 1.5f)] public float force;

    #region SAVELOAD
    public string Save()
    {
        return JsonUtility.ToJson(this);
    }

    public void Load(string _json)
    {
        JsonUtility.FromJsonOverwrite(_json, this);
        //JumpPadData data = JsonUtility.FromJson<JumpPadData>(_json);
        //direction = data.directionData;
        SetDirection();
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetDirection();

            player = collision.gameObject.GetComponent<PlayerMovement>();

            player.isOnJumpPad = true;
            player.padDirection = dir;
            player.padForce = force;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player.isOnJumpPad = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player.padDirection = Vector2.zero;
            player.padForce = 1;

            player = null;
        }
    }

    private void SetDirection()
    {
        switch (direction)
        {
            case JumpDirection.Left:
                dir = Vector3.left;
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;

            case JumpDirection.Right:
                dir = Vector3.right;
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;

            case JumpDirection.Up:
                dir = Vector3.up;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Collider2D col = gameObject.GetComponent<Collider2D>();

        MyDebug.DrawGizmoArrow(col.bounds.center, dir * 1.5f, Color.green, 0.3f);
    }

    private void OnValidate()
    {
        SetDirection();
    }
#endif
}
