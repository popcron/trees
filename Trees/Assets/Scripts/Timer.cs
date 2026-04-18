using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public float interval = 0.5f;
    public UnityEvent onInterval;

    private float time;

    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time >= interval)
        {
            time -= interval;
            if (onInterval != null)
            {
                onInterval.Invoke();
            }
        }
    }
}