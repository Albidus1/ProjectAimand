using UnityEngine;

public class ButtonObjectActivate : MonoBehaviour, IEventListener<ButtonEvent>
{
    [SerializeField] private string obejctID = "default";

    Button button = null;




    public virtual void OnEvent(ButtonEvent _buttonEvent)
    {
        if (_buttonEvent.buttonID != obejctID)
        {
            return;
        }

        ObejctActivate(_buttonEvent.buttonPressed);
    }

    public virtual void ObejctActivate(Button _button)
    {
        Debug.Log("버튼 누름");
        button = _button;
        ButtonEvent.Trigger(obejctID, _button);
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening();
    }
}
