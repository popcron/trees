using UnityEngine;

[CreateAssetMenu]
public class WheelSettings : ScriptableObject
{
    public float brakeTorque = 25f;
    public float engineTorqueScale = 10f;
    public float reverseTorqueScale = 6f;
    public float forwardTopSpeed = 20f;
    public float reverseTopSpeed = 8f;
    public float springStrength = 100f;
    public float springDamper = 10f;
    public float tyreMass = 3f;
    public AnimationCurve powerCurve = new();
    public AnimationCurve reversePowerCurve = new();
    public AnimationCurve lateralGripCurve = AnimationCurve.Constant(0f, 1f, 0.8f);
    public AnimationCurve longitudinalGripCurve = AnimationCurve.Constant(0f, 1f, 0.8f);
}
