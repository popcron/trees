public abstract class Manager : BaseBehaviour
{

}

public abstract class Manager<T> : Manager where T : Manager<T>
{
    public static T singleton;

    protected override void OnEnable()
    {
        base.OnEnable();
        singleton = (T)this;
    }

    protected override void OnDisable()
    {
        singleton = null;
        base.OnDisable();
    }
}