using UnityEngine;

public class PlatformSeesaw : MonoBehaviour
{

    private Vector2 m_initialPosition;



    void Start()
    {
        m_initialPosition = transform.position;  
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = m_initialPosition;
    }
}
