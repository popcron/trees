using System;
using UnityEngine;
using UnityEngine.InputSystem;

public struct PlayerInputState
{
    public Vector2 movement;
    public Vector2 look;
    public float scroll;
    public Button buttons;

    public readonly Vector3 Movement3D
    {
        get
        {
            Vector3 value = movement;
            value.z = value.y;
            value.y = 0;
            if ((buttons & Button.Jump) != 0)
            {
                value.y++;
            }

            if ((buttons & Button.Control) != 0)
            {
                value.y--;
            }

            return value;
        }
    }

    public readonly bool IsButtonPressed(Button button)
    {
        return (buttons & button) != 0;
    }

    public static PlayerInputState Get()
    {
        PlayerInputState state = default;
        if (Mouse.current is Mouse mouse)
        {
            state.look = mouse.delta.ReadValue() * 0.1f;
            state.scroll = mouse.scroll.ReadValue().magnitude;
            if (mouse.leftButton.isPressed)
            {
                state.buttons |= Button.FirstAction;
            }

            if (mouse.rightButton.isPressed)
            {
                state.buttons |= Button.SecondAction;
            }

            if (mouse.middleButton.isPressed)
            {
                state.buttons |= Button.ThirdAction;
            }
        }

        if (Keyboard.current is Keyboard keyboard)
        {
            if (keyboard.wKey.isPressed)
            {
                state.movement += Vector2.up;
            }

            if (keyboard.sKey.isPressed)
            {
                state.movement += Vector2.down;
            }

            if (keyboard.dKey.isPressed)
            {
                state.movement += Vector2.right;
            }

            if (keyboard.aKey.isPressed)
            {
                state.movement += Vector2.left;
            }

            if (keyboard.spaceKey.isPressed)
            {
                state.buttons |= Button.Jump;
            }

            if (keyboard.leftShiftKey.isPressed)
            {
                state.buttons |= Button.Shift;
            }

            if (keyboard.leftCtrlKey.isPressed)
            {
                state.buttons |= Button.Control;
            }

            state.movement.Normalize();
        }

        if (Gamepad.current is Gamepad gamepad)
        {
            Vector2 leftStick = gamepad.leftStick.ReadValue();
            Vector2 rightStick = gamepad.rightStick.ReadValue();
            float deadzone = 0.1f;
            if (leftStick.magnitude > deadzone)
            {
                state.movement += leftStick.normalized;
            }

            if (rightStick.magnitude > deadzone)
            {
                state.look += rightStick * 5f;
            }

            float leftTrigger = gamepad.leftTrigger.ReadValue();
            if (leftTrigger > 0.5f)
            {
                state.buttons |= Button.Shift;
            }

            float rightTrigger = gamepad.rightTrigger.ReadValue();
            if (rightTrigger > 0.5f)
            {
                state.buttons |= Button.Jump;
            }

            if (gamepad.aButton.isPressed)
            {
                state.buttons |= Button.FirstAction;
            }

            if (gamepad.bButton.isPressed)
            {
                state.buttons |= Button.SecondAction;
            }

            if (gamepad.xButton.isPressed)
            {
                state.buttons |= Button.ThirdAction;
            }

            if (gamepad.yButton.isPressed)
            {
                state.buttons |= Button.Control;
            }

            state.movement = Vector2.ClampMagnitude(state.movement, 1f);
        }

        return state;
    }

    [Flags]
    public enum Button
    {
        Jump = 1,
        Shift = 2,
        Control = 4,
        FirstAction = 8,
        SecondAction = 16,
        ThirdAction = 32
    }
}