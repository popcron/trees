public class DebugDummyController : Controller
{
    private void OnEnable()
    {
        LogicLoop.Register(Tick);
    }

    private void Tick()
    {
    }
}