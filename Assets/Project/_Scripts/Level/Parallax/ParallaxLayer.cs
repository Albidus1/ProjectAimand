using UnityEngine;



//[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;

    

    public void MoveLayer(float _deltaPosition)
    {
        Vector3 newPosition = transform.localPosition;
        newPosition.x -= _deltaPosition * parallaxFactor;

        transform.localPosition = newPosition;
    }
}
