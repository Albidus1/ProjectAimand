using System.Collections.Generic;
using UnityEngine;



[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    public ParallaxCamera parallaxCamera;
    

    private List<ParallaxLayer> m_parallaxLayers = new List<ParallaxLayer>();



    private void Start()
    {
        if (parallaxCamera == null)
        {
            parallaxCamera = Camera.main.GetComponent<ParallaxCamera>();
        }

        if (parallaxCamera != null)
        {
            parallaxCamera.onCameraTranslate += MoveLayers;
        }

        SetLayers();
    }

    private void SetLayers()
    {
        m_parallaxLayers.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

            if (layer != null)
            {
                layer.name = "Layer_" + i;
                m_parallaxLayers.Add(layer);
            }
        }
    }

    private void MoveLayers(float _delta)
    {
        foreach (var layer in m_parallaxLayers)
        {
            layer.MoveLayer(_delta);
        }
    }
}
