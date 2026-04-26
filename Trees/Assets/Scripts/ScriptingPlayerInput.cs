using Scripting;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScriptingPlayerInput : ScriptBehaviour
{
    public PlayerInput playerInput;
    public SourceCode actionTriggered;

    [NonSerialized]
    public InputAction.CallbackContext context;

    private void Reset()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        playerInput.onActionTriggered += OnActionTriggered;
        playerInput.onDeviceLost += OnDeviceLost;
        playerInput.onDeviceRegained += OnDeviceRegained;
    }

    private void OnDisable()
    {
        playerInput.onDeviceRegained -= OnDeviceRegained;
        playerInput.onDeviceLost -= OnDeviceLost;
        playerInput.onActionTriggered -= OnActionTriggered;
    }

    public void OnDeviceRegained(PlayerInput input)
    {
    }

    public void OnDeviceLost(PlayerInput input)
    {
    }

    public void OnActionTriggered(InputAction.CallbackContext context)
    {
        this.context = context;
        UpdateBindings();
        interpreter.Evaluate(actionTriggered.content);
    }

    public override void UpdateBindings()
    {
        base.UpdateBindings();
        interpreter.DeclareBinding("name", ReadName);
        interpreter.DeclareBinding("value", ReadValue);
        interpreter.DeclareFunction("print", Print);
    }

    private Value ReadName()
    {
        return Value.Serialize(context.action.name);
    }

    private Value ReadValue()
    {
        if (context.action.type == InputActionType.Button)
        {
            return Value.Serialize(context.action.IsPressed());
        }
        else if (context.action.type == InputActionType.Value)
        {
            if (context.action.activeValueType == typeof(float))
            {
                return Value.Serialize(context.action.ReadValue<float>());
            }
            else if (context.action.activeValueType == typeof(Vector2))
            {
                return Value.Serialize(context.action.ReadValue<Vector2>());
            }
            else
            {
                return Value.Serialize(context.action.ReadValue<float>());
            }
        }
        else
        {
            return Value.Null;
        }
    }

    private void Print(Value[] arguments)
    {
        if (arguments.Length == 1)
        {
            Debug.Log(arguments[0].ToString(), this);
        }
        else if (arguments.Length > 1)
        {
            string format = arguments[0].ToString();
            object[] args = new object[arguments.Length - 1];
            for (int i = 1; i < arguments.Length; i++)
            {
                args[i - 1] = arguments[i].ToString();
            }

            Debug.LogFormat(this, format, args);
        }
    }
}
