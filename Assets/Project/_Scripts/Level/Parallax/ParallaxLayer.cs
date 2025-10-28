using UnityEngine;



[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;


    private Vector2 m_initialPosition;



    private void Start()
    {
        m_initialPosition = transform.position;
    }

    public void MoveLayer(float _deltaPosition)
    {
        Vector3 newPosition = transform.localPosition;
        newPosition.x -= _deltaPosition * parallaxFactor;

        transform.localPosition = newPosition;
    }
}
