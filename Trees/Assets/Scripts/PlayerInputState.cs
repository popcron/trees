using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct PlayerInputState : IEquatable<PlayerInputState>
{
    public Vector2 movement;
    public Vector2 look;
    public Vector3 actions;
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
                state.actions.x = 1f;
            }

            if (mouse.rightButton.isPressed)
            {
                state.actions.y = 1f;
            }

            if (mouse.middleButton.isPressed)
            {
                state.actions.z = 1f;
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
            state.movement += gamepad.leftStick.ReadValue();
            state.look += gamepad.rightStick.ReadValue();
            state.actions.x = gamepad.leftTrigger.ReadValue();
            state.actions.y = gamepad.rightTrigger.ReadValue();

            if (gamepad.aButton.isPressed)
            {
                state.buttons |= Button.Jump;
            }

            if (gamepad.bButton.isPressed)
            {
                state.buttons |= Button.Control;
            }

            if (gamepad.xButton.isPressed)
            {
                state.buttons |= Button.Shift;
            }

            if (gamepad.yButton.isPressed)
            {
                state.buttons |= Button.Control;
            }

            state.movement = Vector2.ClampMagnitude(state.movement, 1f);
        }

        return state;
    }

    public readonly override bool Equals(object obj)
    {
        return obj is PlayerInputState state && Equals(state);
    }

    public readonly bool Equals(PlayerInputState other)
    {
        return movement.Equals(other.movement) && look.Equals(other.look) && scroll == other.scroll && buttons == other.buttons;
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(movement, look, scroll, buttons);
    }

    public static bool operator ==(PlayerInputState left, PlayerInputState right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PlayerInputState left, PlayerInputState right)
    {
        return !(left == right);
    }

    [Flags]
    public enum Button
    {
        Jump = 1,
        Shift = 2,
        Control = 4
    }
}