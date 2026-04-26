using GOAP;

// initialized by GOAP.ActionRegistry
public static class ActionRegistryLoader
{
    static ActionRegistryLoader()
    {
        ActionRegistry.Register(new NavigateToHandler());
        ActionRegistry.Register(new ApplyLookInputHandler());
        ActionRegistry.Register(new LookHandler());
        ActionRegistry.Register(new TurnHandler());
        ActionRegistry.Register(new DoDropHandler());
        ActionRegistry.Register(new DoPickUpHandler());
        ActionRegistry.Register(new JumpHandler());
        ActionRegistry.Register(new LookAtCameraEyesOnlyHandler());
        ActionRegistry.Register(new ApplyPlayerMoveInputHandler());
        ActionRegistry.Register(new ApplyMoveInputHandler());
    }
}