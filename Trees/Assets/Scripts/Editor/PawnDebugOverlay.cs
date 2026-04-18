using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Pawns", true)]
public class PawnDebugOverlay : Overlay
{
    public bool lookAtCamera;

    public override void OnCreated()
    {
        EditorApplication.update += Update;
    }

    public override void OnWillBeDestroyed()
    {
        EditorApplication.update -= Update;
    }

    private void Update()
    {
        if (lookAtCamera)
        {
            UpdatePawns();
        }
    }

    private void UpdatePawns()
    {
        Camera current = Camera.current;
        if (current == null)
        {
            return;
        }

        Vector3 position = current.transform.position;
        Unit[] units = Object.FindObjectsByType<Unit>();
        foreach (Unit unit in units)
        {
            unit.StareAt(position);
        }
    }

    public override VisualElement CreatePanelContent()
    {
        VisualElement root = new() { name = "Pawn Debug Root" };
        Toggle lookAtCameraToggle = new("Look At Camera") { value = lookAtCamera };
        lookAtCameraToggle.RegisterValueChangedCallback(evt => lookAtCamera = evt.newValue);
        root.Add(lookAtCameraToggle);
        return root;
    }
}
