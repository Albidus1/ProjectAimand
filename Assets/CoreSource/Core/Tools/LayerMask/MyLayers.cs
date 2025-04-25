using UnityEngine;



public class MyLayers
{
    public static bool LayerInLayerMask(int _layer, LayerMask _layerMask)
    {
        return ((1 << _layer) & _layerMask) != 0;
    }
}
