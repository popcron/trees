using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class GoalDispatcher : BaseBehaviour
{
    private void Update()
    {
        Camera mainCamera = Camera.main;
        if (Mouse.current is Mouse mouse)
        {
            Vector2 mousePosition = mouse.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            if (Raycasting.TryGetClosestHit(ray, 100f, out RaycastHit hit))
            {
                Vector3 point = hit.point;
                if (WasPressed(Key.G))
                {
                    Vector3 destination = point;

                    if (!NavMesh.SamplePosition(destination, out NavMeshHit navMeshHit, 1f, NavMesh.AllAreas))
                    {
                        if (Physics.Raycast(destination, Vector3.down, out RaycastHit floorHit, 100f))
                        {
                            destination = floorHit.point;
                            NavMesh.SamplePosition(destination, out navMeshHit, 5f, NavMesh.AllAreas);
                            destination = navMeshHit.position;
                        }
                    }
                    else
                    {
                        destination = navMeshHit.position;
                    }

                    List<Unit> selectedUnits = ListPool<Unit>.Get();
                    selectedUnits.AddRange(GetSelectedUnits());
                    Span<Vector2> positions = stackalloc Vector2[selectedUnits.Count];
                    CirclePacking.Generate(selectedUnits.Count, 0.8f, positions);
                    for (int i = 0; i < selectedUnits.Count; i++)
                    {
                        Unit unit = selectedUnits[i];
                        Vector3 unitDestination = destination;
                        unitDestination.x += positions[i].x;
                        unitDestination.z += positions[i].y;
                        unitDestination.y += unit.settings.baseOffset;
                        unit.actor.SubmitGoal(new TryToReach(unitDestination));
                    }

                    ListPool<Unit>.Release(selectedUnits);
                }
                else if (WasPressed(Key.L))
                {
                    foreach (Unit unit in GetSelectedUnits())
                    {
                        unit.actor.SubmitGoal(new LookAt(point));
                    }
                }
            }

            if (WasPressed(Key.V))
            {
                foreach (Unit unit in GetSelectedUnits())
                {
                    unit.actor.SubmitGoal(new LookAt(mainCamera.transform.position));
                }
            }
            else if (WasPressed(Key.F))
            {
                foreach (Unit unit in GetSelectedUnits())
                {
                    unit.actor.SubmitGoal(new LookAtWithEyesOnly(mainCamera.transform.position));
                }
            }
            else if (WasPressed(Key.J))
            {
                foreach (Unit unit in GetSelectedUnits())
                {
                    unit.actor.SubmitGoal(new TryToJump());
                }
            }
        }
    }

    private static IEnumerable<Unit> GetSelectedUnits()
    {
        foreach (Unit unit in Unit.all)
        {
            if (unit == Program.focalPoint && Program.inControl)
            {
                // skip our controlled unit
                continue;
            }

            yield return unit;
        }
    }

    private static bool WasPressed(Key key)
    {
        if (Keyboard.current is Keyboard keyboard)
        {
            return keyboard[key].isPressed;
        }

        return false;
    }
}
