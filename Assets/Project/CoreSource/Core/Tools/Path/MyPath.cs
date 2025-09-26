using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class MyPathMovementElement
{
    public Vector3 pathElementPosition;
    public float delay;
}

public class MyPath : MonoBehaviour
{
    public enum CycleOptions { Single, Loop, PingPong }
    public enum MovementDirection { Ascending, Descending }


    public int startAt = 0;

    [Header("경로")]
    public CycleOptions CycleOption = CycleOptions.Single;
    public MovementDirection InitialMovementDirection = MovementDirection.Ascending;
    public List<MyPathMovementElement> pathElements = new List<MyPathMovementElement>();
    public MyPath referenceMyPath;
    public bool absoluteReferenceMyPath = false;
    public float minDistanceToGoal = 0.1f;
    public bool EndReached => m_endReached;

    [Header("축 고정")]
    public bool LockHandlesOnXAxis = false;
    public bool LockHandlesOnYAxis = false;
    public bool LockHandlesOnZAxis = true;


    public Vector3 originalTransformPosition { get; set; }
    public bool originalTransformPositionStatus { get; set; } = false;
    public bool canMove { get; set; }
    public bool Initialized { get; set; }
    public int currentIndex { get; private set; }
    public Vector3 currentPoint { get => m_initialPosition + m_currentPoint.Current; }

    protected bool m_isActive = false;
    protected bool m_endReached = false;
    protected IEnumerator<Vector3> m_currentPoint;
    protected int m_direction = 1;
    protected Vector3 m_initialPosition;
    protected Vector3 m_previousPoint;
    protected float m_distanceToNextPoint;

    public bool m_initialEditorPosition { get; set; }



    protected virtual void Start()
    {
        if (false == Initialized)
        {
            Initialization();
        }
    }

    #region INITAILIZATION
    public virtual void Initialization()
    {
        m_isActive = true;
        canMove = false;
        m_endReached = false;

        if (referenceMyPath != null && 
            (referenceMyPath.pathElements != null || referenceMyPath.pathElements.Count > 0))
        {
            if (absoluteReferenceMyPath)
            {
                transform.position = referenceMyPath.transform.position;
            }

            pathElements = referenceMyPath.pathElements;
        }

        if (pathElements == null || pathElements.Count < 1)
        {
            Debug.Log("경로 없음");
            return;
        }

        if (pathElements[0].pathElementPosition != Vector3.zero)
        {
            Vector3 pathPosition_0 = pathElements[0].pathElementPosition;
            transform.position += pathPosition_0;

            foreach (MyPathMovementElement element in pathElements)
            {
                element.pathElementPosition -= pathPosition_0;
            }
        }

        if (InitialMovementDirection == MovementDirection.Ascending)
        {
            m_direction = 1;
        }
        else
        {
            m_direction = -1;
        }

        m_initialPosition = transform.position;

        m_currentPoint = GetPathEnumerator();
        m_previousPoint = m_currentPoint.Current;
        m_currentPoint.MoveNext();

        if (false == originalTransformPositionStatus)
        {
            originalTransformPosition = transform.position;
            originalTransformPositionStatus = true;
        }

        transform.position = originalTransformPosition + m_currentPoint.Current;
    }

    public virtual IEnumerator<Vector3> GetPathEnumerator()
    {
        if (pathElements == null || pathElements.Count < 1)
        {
            yield break;
        }

        int index = 0;
        currentIndex = index;

        while (true)
        {
            index = Mathf.Clamp(index, 0, pathElements.Count - 1);

            currentIndex = index;

            yield return pathElements[index].pathElementPosition;

            if (pathElements.Count <= 1)
            {
                continue;
            }

            #region CYCLE OPTIONS
            if (CycleOption == CycleOptions.Single)
            {
                m_endReached = false;

                if (index <= 0)
                {
                    if (m_direction == 1)
                    {
                        m_direction = 1;
                    }
                    else
                    {
                        index = 0;
                        m_endReached = true;
                    }
                }
                else if (index >= pathElements.Count - 1)
                {
                    if (m_direction == -1)
                    {
                        m_direction = -1;
                    }
                    else
                    {
                        index = pathElements.Count - 1;
                        m_endReached = true;
                    }
                }

                index += m_direction;
            }

            if (CycleOption == CycleOptions.Loop)
            {
                index += m_direction;

                if (index < 0)
                {
                    index = pathElements.Count - 1;
                }
                else if (index > pathElements.Count - 1)
                {
                    index = 0;
                }
            }

            if (CycleOption == CycleOptions.PingPong)
            {
                if (index <= 0)
                {
                    m_direction = 1;
                }
                else if (index >= pathElements.Count - 1)
                {
                    m_direction = -1;
                }

                index += m_direction;
            }
            #endregion
        }
    }
    #endregion

    #region UPDATE
    protected virtual void Update()
    {
        if (pathElements == null || pathElements.Count < 1
            || m_endReached
            || false == canMove)
        {
            return;
        }

        ComputePath();
    }

    protected virtual void ComputePath()
    {
        m_distanceToNextPoint = (transform.position - (originalTransformPosition + m_currentPoint.Current)).magnitude;

        if (m_distanceToNextPoint < minDistanceToGoal)
        {
            m_previousPoint = m_currentPoint.Current;
            m_currentPoint.MoveNext();
        }
    }
    #endregion

    #region GENERAL METHODS
    public virtual void ChangeDirection()
    {
        m_direction *= -1;
        m_currentPoint.MoveNext();
    }

    public virtual void ChangeDirection(int _direction)
    {
        m_direction = _direction;
        m_currentPoint.MoveNext();
    }
    #endregion

    #region GIZMOS
    protected virtual void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (pathElements == null)
            return;

        if (pathElements.Count == 0)
            return;

        if (false == originalTransformPositionStatus)
        {
            originalTransformPosition = transform.position;
            originalTransformPositionStatus = true;
        }

        if (transform.hasChanged && false == m_isActive)
        {
            originalTransformPosition = transform.position;
        }

        for (int i = 0; i < pathElements.Count; i++)
        {
            MyDebug.DrawGizmoPoint(originalTransformPosition + pathElements[i].pathElementPosition, 0.2f, Color.green);

            if (i + 1 < pathElements.Count)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(originalTransformPosition + pathElements[i].pathElementPosition,
                                originalTransformPosition + pathElements[i + 1].pathElementPosition);
            }
            if (i == pathElements.Count - 1 && CycleOption == CycleOptions.Loop)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(originalTransformPosition + pathElements[i].pathElementPosition,
                                originalTransformPosition + pathElements[0].pathElementPosition);
            }
        }

        if (Application.isPlaying && m_currentPoint != null)
        {
            MyDebug.DrawGizmoPoint(originalTransformPosition + m_currentPoint.Current, 0.2f, Color.blue);
            MyDebug.DrawGizmoPoint(originalTransformPosition + m_previousPoint, 0.2f, Color.red);
        }
#endif
    }
    #endregion
}
