using System.Collections;
using UnityEngine;

public class ButtonObjectActivate : MonoBehaviour, IEventListener<ButtonEvent>, ISaveLoadManagerMethods
{
    [SerializeField] protected string objectID = "default";

    protected Button button = null;



    public virtual string Save()
    {
        return JsonUtility.ToJson(this);
    }

    public virtual void Load(string _json)
    {
        JsonUtility.FromJsonOverwrite(_json, this);
    }

    public virtual void OnEvent(ButtonEvent _buttonEvent)
    {
        if (_buttonEvent.buttonID != objectID)
        {
            return;
        }

        ObjectActivate(_buttonEvent.buttonPressed);
    }

    public virtual void ObjectActivate(Button _button)
    {
        button = _button;

        //StartCoroutine(nameof(ObjectMoving));
    }

    private IEnumerator ObjectMoving()
    {
        // 테스트용 코드
        Vector3 direction = Vector3.right;
        Vector3 targetToPosition = transform.position + Vector3.right;
        float moveSpeed = 2f * Time.deltaTime;
        float distanceToTarget = 0;

        while (true)
        {
            distanceToTarget = (targetToPosition - transform.position).magnitude;

            transform.Translate(direction * moveSpeed, Space.World);

            if (moveSpeed >= distanceToTarget)
            {
                yield break;
            }

            yield return null;
        }
    }

    protected virtual void OnEnable()
    {
        this.EventStartListening<ButtonEvent>();
    }

    protected virtual void OnDisable()
    {
        this.EventStopListening<ButtonEvent>();
    }
}
