using UnityEngine;

[CreateAssetMenu]
public class WheelSettings : ScriptableObject
{
    public float mass = 200f;
    public float frontAntiRollStiffness = 3000f;
    public float rearAntiRollStiffness = 3000f;
    public float brakeTorque = 25f;
    public float engineTorqueScale = 10f;
    public float reverseTorqueScale = 6f;
    public float forwardTopSpeed = 20f;
    public float reverseTopSpeed = 8f;
    public float springStrength = 100f;
    public float dampingRatio = 0.6f;
    public float tyreMass = 3f;
    public float rollingResistance = 0.08f;
    public float peakFrictionCoefficient = 1.2f;
    public float lateralStiffness = 1000f;
    public float longitudinalStiffness = 120f;
    public AnimationCurve powerCurve = new();
    public AnimationCurve lateralGripCurve = AnimationCurve.Constant(0f, 1f, 0.8f);
    public AnimationCurve longitudinalGripCurve = AnimationCurve.Constant(0f, 1f, 0.8f);
}
