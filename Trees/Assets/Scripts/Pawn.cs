using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Pawn : BaseBehaviour
{
    public const float GroundOffset = 0.495f;

    public BodyPart body;
    public DecalProjector shadowProjector;
    public float maxEyePitch = 30f;
    public float maxEyeYaw = 35f;
    public float maxHeadYaw = 60f;
    public Vector2 eyePitchYaw;
    public Vector2 headPitchYaw;
    public float bodyYaw;
    public float eyeDistance = 5f;
    public float shadowOpacity = 0.3f;
    public float minEyeAngleVariation = 0.1f;
    public float maxEyeAngleVariation = 0.5f;
    public float variationAmount = 4f;

    private float nextEyeVariation;
    private Vector2 pitchYawVariation;

    public IEnumerable<PawnPart> Parts
    {
        get
        {
            yield return body;
            yield return body.head;
            yield return body.head.leftEye;
            yield return body.head.rightEye;
            yield return body.leftHand;
            yield return body.rightHand;
        }
    }

    public Quaternion GetLookRotation(Vector2 pitchYawOffset = default)
    {
        Quaternion bodyRotation = Quaternion.Euler(0f, bodyYaw, 0f);
        Quaternion headRotation = Quaternion.Euler(headPitchYaw.x, headPitchYaw.y, 0f);
        Quaternion eyeRotation = Quaternion.Euler(eyePitchYaw.x + pitchYawOffset.x, eyePitchYaw.y + pitchYawOffset.y, 0f);
        return bodyRotation * headRotation * eyeRotation;
    }

    private void Reset()
    {
        body = GetComponentInChildren<BodyPart>();
    }

    private void OnDrawGizmosSelected()
    {
        EditorGizmos.color = Color.yellow;
        Vector3 eyeTargetPosition = GetEyeTargetPosition();
        Vector3 leftEyePos = body.head.leftEye.transform.position;
        Vector3 rightEyePos = body.head.rightEye.transform.position;
        EditorGizmos.DrawLine(leftEyePos, eyeTargetPosition);
        EditorGizmos.DrawLine(rightEyePos, eyeTargetPosition);
    }

    private void Update()
    {
        UpdateLookOrientation();
        UpdateShadowTransform();
    }

    private void UpdateShadowTransform()
    {
        const float MaxShadowDistance = 8f;
        if (TryGetGroundPosition(MaxShadowDistance, out RaycastHit groundHit))
        {
            Vector3 shadowTop = transform.position + new Vector3(0f, -GroundOffset, 0f);
            if (groundHit.point.y > shadowTop.y)
            {
                Vector3 position = shadowProjector.transform.position;
                position.y = groundHit.point.y + 0.01f;
                shadowProjector.transform.position = position;
                shadowProjector.transform.localScale = new(1f, 1f, 0.05f);
                shadowProjector.fadeFactor = 1f * shadowOpacity;
            }
            else
            {
                float shadowDistance = Vector3.Distance(shadowTop, groundHit.point);
                Vector3 localPosition = shadowProjector.transform.localPosition;
                localPosition.y = -(0.5f * shadowDistance + 0.5f);
                shadowProjector.transform.localPosition = localPosition;
                shadowProjector.transform.localScale = new(1f, 1f, shadowDistance + 0.05f);
                shadowProjector.fadeFactor = (1f - shadowDistance / MaxShadowDistance) * shadowOpacity;
            }

            shadowProjector.gameObject.SetActive(true);
        }
        else
        {
            shadowProjector.gameObject.SetActive(false);
        }
    }

    public bool TryGetGroundPosition(float distance, out RaycastHit closestHit)
    {
        int mask = ~LayerMask.GetMask("Unit");
        Ray ray = new(transform.position + new Vector3(0f, GroundOffset, 0f), -transform.up);
        return Raycasting.TryGetClosestHit(ray, distance + GroundOffset, mask, IgnoreMyColliders, out closestHit);
    }

    private bool IgnoreMyColliders(RaycastHit hit)
    {
        return !hit.collider.transform.IsChildOf(transform);
    }

    public void StareAt(Vector3 lookAt)
    {
        Vector3 newDirection = (lookAt - body.head.transform.position).normalized;
        eyeDistance = Vector3.Distance(body.head.transform.position, lookAt);
        Vector3 localDirection = Quaternion.Inverse(body.transform.rotation) * newDirection;
        eyePitchYaw.x = -Mathf.Asin(localDirection.y) * Mathf.Rad2Deg;
        eyePitchYaw.y = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        UpdateLookOrientation();
    }

    public void UpdateLookOrientation()
    {
        Transform headTransform = body.head.transform;
        Transform bodyTransform = body.transform;
        headPitchYaw.y = Mathf.Clamp(headPitchYaw.y, -maxHeadYaw, maxHeadYaw);
        eyePitchYaw.x = Mathf.Clamp(eyePitchYaw.x, -maxEyePitch, maxEyePitch);
        eyePitchYaw.y = Mathf.Clamp(eyePitchYaw.y, -maxEyeYaw, maxEyeYaw);
        bodyTransform.rotation = Quaternion.Euler(0f, bodyYaw, 0f);
        headTransform.localRotation = Quaternion.Euler(headPitchYaw.x, headPitchYaw.y, 0f);
        UpdatePupilRotation();
    }

    private void UpdatePupilRotation()
    {
        Vector3 eyeTargetPosition = GetEyeTargetPosition();
        body.head.leftEye.pupil.LookAt(eyeTargetPosition);
        body.head.rightEye.pupil.LookAt(eyeTargetPosition);
    }

    private Vector3 GetEyeTargetPosition()
    {
        Vector2 variation = GetPitchYawVariation();
        Quaternion rotation = GetLookRotation(variation);
        Vector3 direction = rotation * Vector3.forward;
        return body.head.transform.position + direction * Mathf.Max(eyeDistance, 0.3f);
    }

    private Vector2 GetPitchYawVariation()
    {
        if (!Application.isPlaying)
        {
            return default;
        }

        if (Time.time > nextEyeVariation)
        {
            RandomGenerator rng = new(GetEntityId().GetLongHashCode());
            float t = (Time.time * 30f) + rng.NextFloat();
            float e = Mathf.PerlinNoise(t, t * 0.5f);
            nextEyeVariation = Time.time + Mathf.LerpUnclamped(minEyeAngleVariation, maxEyeAngleVariation, e);
            pitchYawVariation = Random.insideUnitCircle * variationAmount;
        }

        return pitchYawVariation;
    }
}
