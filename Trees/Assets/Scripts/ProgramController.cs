using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Stores the state of the program.
/// </summary>
public class ProgramController : MonoBehaviour
{
    public static ProgramController singleton;

    private void OnEnable()
    {
        singleton = this;
    }

    private void OnDisable()
    {
        singleton = null;
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        BaseBehaviour hoveringOver = Program.GetHoveringOver();
        if (Program.focalPoint == null)
        {
            if (hoveringOver != null)
            {
                if (Mouse.current is Mouse mouse)
                {
                    if (mouse.leftButton.wasPressedThisFrame)
                    {
                        Program.inControl = true;
                        Program.focalPoint = hoveringOver;
                    }
                    else if (mouse.rightButton.wasPressedThisFrame)
                    {
                        Program.inControl = false;
                        Program.focalPoint = hoveringOver;
                    }
                }
            }
        }
        else
        {
            if (Keyboard.current is Keyboard keyboard)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    Program.inControl = false;
                    Program.focalPoint = null;
                }
            }
        }
    }
}
