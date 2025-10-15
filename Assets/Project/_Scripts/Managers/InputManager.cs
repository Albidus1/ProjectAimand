using System;
using System.Collections.Generic;
using UnityEngine;



public class InputManager : MySingleton<InputManager>
{
    [Header("설정")]
    public bool inputDetectionActive = true;
    public bool resetButtonStatesOnFocusLoss = true;

    [Header("이동")]
    public bool smoothMovement = true;

    [Header("버튼")]
    public string JumpButtonID = "Jump";
    public string ColorInvertButtonID = "ColorInvert";
    public string PauseButtonID = "Pause";

    public MyInput.IMButton JumpButton      { get; protected set; }
    public MyInput.IMButton ColorInvertButton   { get; protected set; }
    public MyInput.IMButton PauseButton     { get; protected set; }
    public Vector2 primaryMovement
    {
        get 
        { 
            return m_primaryMovement; 
        }
    }

    protected List<MyInput.IMButton> m_buttonList;
    protected Vector2 m_primaryMovement;
    protected string m_axisHorizontal;
    protected string m_axisVertical;



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected static void InitializeStatics()
    {
        m_instance = null;
    }

    protected virtual void Start()
    {
        Initialization();
    }

    protected virtual void Initialization()
    {
        InitializeButton();
        InitializeAxis();
    }

    protected virtual void InitializeButton()
    {
        m_buttonList = new List<MyInput.IMButton>();
        m_buttonList.Add(JumpButton = new MyInput.IMButton(JumpButtonID, JumpButtonDown, JumpButtonPressed, JumpButtonUp));
        m_buttonList.Add(ColorInvertButton = new MyInput.IMButton(ColorInvertButtonID, ColorInvertButtonDown, ColorInvertButtonPressed, ColorInvertButtonUp));
        m_buttonList.Add(PauseButton = new MyInput.IMButton(PauseButtonID, PauseButtonDown, PauseButtonPressed, PauseButtonUp));
    }

    protected virtual void InitializeAxis()
    {
        m_axisHorizontal = "Horizontal";
        m_axisVertical = "Vertical";
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GUIManager.HasInstance)
            {
                if (GUIManager.Instance.enableQuitPopUp)
                {
                    GUIManager.Instance.enableQuitPopUp = false;
                    return;
                }
            }

            if (GameManager.HasInstance)
            {
                if (false == GUIManager.Instance.enableQuitPopUp)
                {
                    //Debug.Log("키 입력");
                    MainEvent.Trigger(MainEventTypes.TogglePause);
                }
            }
        }

        if (inputDetectionActive)
        {
            SetMovement();
            GetInputButtons();
        }
    }

    protected virtual void SetMovement()
    {
        if (inputDetectionActive)
        {
            if (smoothMovement)
            {
                m_primaryMovement.x = Input.GetAxis(m_axisHorizontal);
                m_primaryMovement.y = Input.GetAxis(m_axisVertical);
            }
            else
            {
                m_primaryMovement.x = Input.GetAxisRaw(m_axisHorizontal);
                m_primaryMovement.y = Input.GetAxisRaw(m_axisVertical);
            }
        }
    }

    protected virtual void GetInputButtons()
    {
        foreach (MyInput.IMButton button in m_buttonList)
        {
            if (Input.GetButton(button.buttonID))
            {
                button.TriggerButtonPressed();
            }
            if (Input.GetButtonDown(button.buttonID))
            {
                button.TriggerButtonDown();
            }
            if (Input.GetButtonUp(button.buttonID))
            {
                button.TriggerButtonUp();
            }

            Debug.Log(button.state.currentState);
        }
    }

    protected virtual void LateUpdate()
    {
        ProcessButtonStates();    
    }

    protected virtual void ProcessButtonStates()
    {
        foreach (MyInput.IMButton button in m_buttonList)
        {
            if (button.state.currentState == MyInput.ButtonStates.ButtonDown)
            {
                button.state.StateChange(MyInput.ButtonStates.ButtonPressed);
            }
            if (button.state.currentState == MyInput.ButtonStates.ButtonUp)
            {
                button.state.StateChange(MyInput.ButtonStates.Off);
            }
        }
    }

    protected virtual void OnApplicationFocus(bool focus)
    {
        if (false == focus && resetButtonStatesOnFocusLoss && m_buttonList != null)
        {
            ForceAllButtonStatesTo(MyInput.ButtonStates.ButtonUp);
        }
    }

    protected virtual void ForceAllButtonStatesTo(MyInput.ButtonStates _newState)
    {
        foreach (MyInput.IMButton button in m_buttonList)
        {
            button.state.StateChange(_newState);
        }
    }


    #region BUTTON ASSIGNMENT METHODS
    public virtual void JumpButtonDown()            { JumpButton.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    public virtual void JumpButtonPressed()         { JumpButton.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    public virtual void JumpButtonUp()              {JumpButton.state.StateChange(MyInput.ButtonStates.ButtonUp); }

    public virtual void ColorInvertButtonDown()     { ColorInvertButton.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    public virtual void ColorInvertButtonPressed()  { ColorInvertButton.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    public virtual void ColorInvertButtonUp()       { ColorInvertButton.state.StateChange(MyInput.ButtonStates.ButtonUp); }

    public virtual void PauseButtonDown()           { PauseButton.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    public virtual void PauseButtonPressed()        { PauseButton.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    public virtual void PauseButtonUp()             { PauseButton.state.StateChange(MyInput.ButtonStates.ButtonUp); }
    #endregion
}
