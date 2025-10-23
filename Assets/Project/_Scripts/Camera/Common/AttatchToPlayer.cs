using UnityEngine;


[RequireComponent(typeof(MyFollowTarget))]
public class AttatchToPlayer : MonoBehaviour
{
    protected MyFollowTarget m_target;



    protected virtual void Awake()
    {
        m_target = GetComponent<MyFollowTarget>();

        m_target.interpolatePosition = false;
        m_target.updateMode = MyFollowTarget.UpdateModes.LateUpdate;
    }

    public virtual void Attatch(Transform _target)
    {
        m_target.target = _target;
    }
}
