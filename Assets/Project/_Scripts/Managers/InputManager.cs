using System;
using UnityEngine;
using UnityEngine.PlayerLoop;



public class InputManager : MySingleton<InputManager>
{




    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeStatics()
    {
        m_instance = null;
    }

    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("키 입력");
            MainEvent.Trigger(MainEventTypes.TogglePause);
        }
    }

    private void LateUpdate()
    {
        
    }
}
