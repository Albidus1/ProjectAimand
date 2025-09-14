using UnityEngine;

public static class ObjectFind 
{
    public static Transform GetFineName(this Transform p_this, string p_namestr
        , bool p_recusive = false)
    {
        foreach (Transform t in p_this)
        {
            if( t.name == p_namestr) return t;
        }

        return null;
    }
}
