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
    public const string horizontalID = "Horizontal";
    public const string verticalID = "Vertical";

    public string cancleButtonID = "Cancle";
    public string jumpButtonID = "Jump";
    public string colorInvertButtonID = "ColorInvert";
    public string pauseButtonID = "Pause";

    public MyInput.IMButton HorizontalButton    { get; protected set; }
    public MyInput.IMButton VerticalButton      { get; protected set; }
    public MyInput.IMButton CancleButton        { get; protected set; }
    public MyInput.IMButton JumpButton          { get; protected set; }
    public MyInput.IMButton ColorInvertButton   { get; protected set; }
    public MyInput.IMButton PauseButton         { get; protected set; }

    public Vector2 primaryMovement
    {
        get 
        { 
            return m_primaryMovement; 
        }
    }
    public Vector2 seconaryMovement
    {
        get
        {
            return m_secondaryMovement;
        }
    }

    protected List<MyInput.IMButton> m_buttonList;
    protected Vector2 m_primaryMovement;
    protected Vector2 m_secondaryMovement;


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
    }

    protected virtual void InitializeButton()
    {
        m_buttonList = new List<MyInput.IMButton>();
        m_buttonList.Add(HorizontalButton = new MyInput.IMButton(horizontalID, HorizontalButtonDown, HorizontalButtonPressed, HorizontalButtonUp));
        m_buttonList.Add(VerticalButton = new MyInput.IMButton(verticalID, VerticalButtonDown, VerticalButtonPressed, VerticalButtonUp));
        m_buttonList.Add(CancleButton = new MyInput.IMButton(cancleButtonID, CancleButtonDown, CancleButtonPressed, CancleButtonUp));
        m_buttonList.Add(JumpButton = new MyInput.IMButton(jumpButtonID, JumpButtonDown, JumpButtonPressed, JumpButtonUp));
        m_buttonList.Add(ColorInvertButton = new MyInput.IMButton(colorInvertButtonID, ColorInvertButtonDown, ColorInvertButtonPressed, ColorInvertButtonUp));
        m_buttonList.Add(PauseButton = new MyInput.IMButton(pauseButtonID, PauseButtonDown, PauseButtonPressed, PauseButtonUp));
    }

    protected virtual void Update()
    {
        if (inputDetectionActive)
        {
            SetMovement();
            SetSecondaryMovement();
            GetInputButtons();
        }
    }

    protected virtual void SetMovement()
    {
        if (smoothMovement)
        {
            m_primaryMovement.x = Input.GetAxis(horizontalID);
            m_primaryMovement.y = Input.GetAxis(verticalID);
        }
        else
        {
            m_primaryMovement.x = Input.GetAxisRaw(horizontalID);
            m_primaryMovement.y = Input.GetAxisRaw(verticalID);
        }
    }

    protected virtual void SetSecondaryMovement()
    {
        m_secondaryMovement.x = Input.GetAxis(horizontalID);
        m_secondaryMovement.y = Input.GetAxis(verticalID);
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

            //Debug.Log(button.state.currentState);
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
    //public virtual void ButtonDown() { Button.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    //public virtual void ButtonPressed() { Button.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    //public virtual void ButtonUp() { Button.state.StateChange(MyInput.ButtonStates.ButtonUp); }

    public virtual void HorizontalButtonDown()      { HorizontalButton.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    public virtual void HorizontalButtonPressed()   { HorizontalButton.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    public virtual void HorizontalButtonUp()        { HorizontalButton.state.StateChange(MyInput.ButtonStates.ButtonUp); }

    public virtual void VerticalButtonDown()        { VerticalButton.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    public virtual void VerticalButtonPressed()     { VerticalButton.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    public virtual void VerticalButtonUp()          { VerticalButton.state.StateChange(MyInput.ButtonStates.ButtonUp); }

    public virtual void CancleButtonDown()          { CancleButton.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    public virtual void CancleButtonPressed()       { CancleButton.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    public virtual void CancleButtonUp()            { CancleButton.state.StateChange(MyInput.ButtonStates.ButtonUp); }

    public virtual void JumpButtonDown()            { JumpButton.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    public virtual void JumpButtonPressed()         { JumpButton.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    public virtual void JumpButtonUp()              { JumpButton.state.StateChange(MyInput.ButtonStates.ButtonUp); }

    public virtual void ColorInvertButtonDown()     { ColorInvertButton.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    public virtual void ColorInvertButtonPressed()  { ColorInvertButton.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    public virtual void ColorInvertButtonUp()       { ColorInvertButton.state.StateChange(MyInput.ButtonStates.ButtonUp); }

    public virtual void PauseButtonDown()           { PauseButton.state.StateChange(MyInput.ButtonStates.ButtonDown); }
    public virtual void PauseButtonPressed()        { PauseButton.state.StateChange(MyInput.ButtonStates.ButtonPressed); }
    public virtual void PauseButtonUp()             { PauseButton.state.StateChange(MyInput.ButtonStates.ButtonUp); }
    #endregion
}
