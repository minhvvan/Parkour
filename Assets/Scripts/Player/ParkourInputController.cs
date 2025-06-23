using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParkourInputController : MonoBehaviour, Parkour.IPlayerActions
{
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool jump;
    public bool sprint;
    public bool cursorLocked;
    public bool crouch;
    
    private Parkour _parkour;
    
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        jump = context.ReadValue<bool>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        sprint = context.ReadValue<bool>();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if(context.started) crouch = true;
        else if(context.canceled) crouch = false;
    }

    private void OnEnable()
    {
        if (_parkour == null)
        {
            _parkour = new Parkour();
            _parkour.Player.SetCallbacks(this);
        }
    
        _parkour.Player.Enable();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
