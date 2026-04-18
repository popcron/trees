using Cinemachine;
using UnityEngine;

public class FollowFocalPoint : CinemachineExtension
{
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            BaseBehaviour focalPoint = Program.focalPoint;
            if (focalPoint != null)
            {
                focalPoint.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                if (focalPoint is Unit unit)
                {
                    position = unit.head.transform.position;
                    rotation = unit.LookRotation;
                }

                state.RawPosition = position;
                state.RawOrientation = rotation;
            }
        }
    }
}
