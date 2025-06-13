using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParkourInputController : MonoBehaviour
{
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool jump;
    public bool sprint;
    public bool cursorLocked;
    
#if ENABLE_INPUT_SYSTEM

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    
    public void OnJump(InputValue value)
    {
        jump = value.isPressed;
    }
    
    public void OnSprint(InputValue value)
    {
        sprint = value.isPressed;
    }
#endif
    
    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
