using UnityEngine;



public class MyInput : MonoBehaviour
{
    public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp }
    public enum AxisTypes { Positive,  Negative }

    public static ButtonStates ProcessAxisAsButton(string _axisName, float _threshold, ButtonStates _currentState, AxisTypes _axisTypes = AxisTypes.Positive)
    {
        float axisValue = Input.GetAxis(_axisName);
        ButtonStates returnState;

        bool comparison = (_axisTypes == AxisTypes.Positive) ? axisValue < _threshold : axisValue > _threshold;
        if (comparison)
        {
            if (_currentState == ButtonStates.ButtonPressed)
            {
                returnState = ButtonStates.ButtonUp;
            }
            else
            {
                returnState = ButtonStates.Off;
            }
        }
        else
        {
            if (_currentState == ButtonStates.Off)
            {
                returnState= ButtonStates.ButtonDown;
            }
            else
            {
                returnState = ButtonStates.ButtonPressed;
            }
        }

        return returnState;
    }

    public class IMButton
    {
        public MyStateMachine<MyInput.ButtonStates> state { get; protected set; }
        public string buttonID;

        public delegate void ButtonDownMethodDelegate();
        public delegate void ButtonPressedMethodDelegate();
        public delegate void ButtonUpMethodDelegate();

        public ButtonDownMethodDelegate ButtonDownMethod;
        public ButtonPressedMethodDelegate ButtonPressedMethod;
        public ButtonUpMethodDelegate ButtonUpMethod;


        public float timeSinceLastButtonDown
        {
            get
            {
                return Time.unscaledTime - m_lastButtonDownAt;
            }
        }
        public float timeSinceLastButtonUp
        {
            get
            {
                return Time.unscaledTime - m_lastButtonUpAt;
            }
        }
        public bool ButtonDownRecently(float _time)
        {
            return timeSinceLastButtonDown <= _time;
        }
        public bool ButtonUpRecently(float _time)
        {
            return timeSinceLastButtonUp <= _time;
        }

        protected float m_lastButtonDownAt;
        protected float m_lastButtonUpAt;



        public IMButton(string _buttonID, ButtonDownMethodDelegate _btnDown = null, ButtonPressedMethodDelegate _btnPressed = null, ButtonUpMethodDelegate _btnUp = null)
        {
            buttonID = _buttonID;
            ButtonDownMethod = _btnDown;
            ButtonPressedMethod = _btnPressed;
            ButtonUpMethod = _btnUp;

            state = new MyStateMachine<MyInput.ButtonStates>(null, false);
            state.StateChange(MyInput.ButtonStates.Off);
        }



        public bool IsDown      => (state.currentState == MyInput.ButtonStates.ButtonDown);
        public bool IsPressed   => (state.currentState == MyInput.ButtonStates.ButtonPressed);
        public bool IsUp        => (state.currentState == MyInput.ButtonStates.ButtonUp);
        public bool IsOff       => (state.currentState == MyInput.ButtonStates.Off);

        public void TriggerButtonDown()
        {
            m_lastButtonDownAt = Time.unscaledTime;
            if (ButtonDownMethod == null)
            {
                state.StateChange(MyInput.ButtonStates.ButtonDown);
            }
            else
            {
                ButtonDownMethod();
            }
        }

        public void TriggerButtonPressed()
        {
            if (ButtonPressedMethod == null)
            {
                state.StateChange(MyInput.ButtonStates.ButtonPressed);
            }
            else
            {
                ButtonPressedMethod();
            }
        }

        public void TriggerButtonUp()
        {
            m_lastButtonUpAt = Time.unscaledTime;
            if (ButtonUpMethod == null)
            {
                state.StateChange(ButtonStates.ButtonUp);
            }
            else
            {
                ButtonUpMethod();
            }
        }
    }
}
