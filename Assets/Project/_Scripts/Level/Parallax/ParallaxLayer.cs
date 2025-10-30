using UnityEngine;



[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;



    private void Start()
    {

    }

    public void MoveLayer(float _deltaPosition)
    {
        Vector3 newPosition = transform.position;
        Vector3 targetPosition = new Vector3(_deltaPosition, 0);
        Vector3 speed = new Vector3(parallaxFactor, 0);

        newPosition += Vector3.Scale(targetPosition, speed) * -1;

        transform.position = newPosition;
    }
}
