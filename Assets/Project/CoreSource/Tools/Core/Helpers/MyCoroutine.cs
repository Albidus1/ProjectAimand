using System.Collections;
using UnityEngine;



public static class MyCoroutine 
{
    public static IEnumerator WaitForFrams(int _frameCount)
    {
        while (_frameCount > 0)
        {
            _frameCount--;
            yield return null;
        }
    }

    public static IEnumerator WaitFor(float _seconds)
    {
        for (float timer = 0f; timer < _seconds; timer += Time.deltaTime)
        {
            yield return null;
        }
    }

    public static IEnumerator WaitForUnscaled(float _seconds)
    {
        for (float timer = 0f; timer < _seconds; timer += Time.unscaledDeltaTime)
        {
            yield return null;
        }
    }
}
