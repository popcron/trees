using UnityEngine;

/// <summary>
/// Can be queried for using an <see cref="EntityId"/>.
/// </summary>
public abstract class BaseBehaviour : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        EntityIdRegistry.map.Add(GetEntityId(), this);
    }

    protected virtual void OnDisable()
    {
        EntityIdRegistry.map.Remove(GetEntityId());
    }
}
