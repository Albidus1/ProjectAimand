using System;
using UnityEngine;




public struct ButtonEvent
{
    static ButtonEvent e;

    public string buttonID;
    public Button buttonPressed;

    public ButtonEvent(string _id, Button _buttonPressed)
    {
        buttonID = _id;
        buttonPressed = _buttonPressed;
    }

    public static void Trigger(string _id, Button _buttonPressed)
    {
        e.buttonID = _id;
        e.buttonPressed = _buttonPressed;
        EventManager.TriggerEvent(e);
    }
}

public class Button : MonoBehaviour, ISaveLoadManagerMethods
{
    public enum ButtonState { UnPressed, Pressed };

    [SerializeField] private string ButtonID = "default";

    public enum ButtonDirection { Left, Right, Up, Down };
    public ButtonDirection direction;
    private Vector3 dir;

    protected const string playerTag = "Player";
    public LayerMask checkLayer;


    #region SAVELOAD
    public string Save() => JsonUtility.ToJson(this);
    public void Load(string _json)
    {
        JsonUtility.FromJsonOverwrite(_json, this);
        //JumpPadData data = JsonUtility.FromJson<JumpPadData>(_json);
        //direction = data.directionData;
        SetDirection();
    }
    #endregion

    private void Awake()
    {
        SetDirection();
    }

    private void Start()
    {
        
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (false == MyLayers.LayerInLayerMask(this.gameObject.layer, checkLayer))
        {
            return;
        }

        ButtonEvent.Trigger(ButtonID, this);
    }

    #region OTHER METHODS
    private void SetDirection()
    {
        switch (direction)
        {
            case ButtonDirection.Left:
                dir = Vector3.left;
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;

            case ButtonDirection.Right:
                dir = Vector3.right;
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;

            case ButtonDirection.Up:
                dir = Vector3.up;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case ButtonDirection.Down:
                dir = Vector3.down;
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
        }
    }
    #endregion

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
