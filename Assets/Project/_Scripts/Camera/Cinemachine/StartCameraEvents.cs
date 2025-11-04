using UnityEngine;

public class StartCameraEvents : MonoBehaviour
{
    public void StartCameraShaking()
    {
        CameraShakeEvent.Trigger("", 0f, 0.25f, 30f);
    }

    public void StopCameraShaking()
    {
        CameraShakeStopEvent.Trigger("");
    }
}
