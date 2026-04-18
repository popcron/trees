using GOAP;
using System;
using System.Collections.Generic;
using TypeRegistries;
using Action = GOAP.Action;

public class ActionRegistry : TypeRegistry<ActionHandler>
{
    private static readonly Dictionary<TypeID, ActionHandler> handlers = new();
    private static Action[] actions = { };
    private static int layerCount = 1;

    public static ReadOnlySpan<Action> Actions => actions;
    public static int LayerCount => layerCount;

    static ActionRegistry()
    {
        ActionHandlerRegistryLoader.RegisterAllTypes();
    }

    public override void FinalizeRegistration()
    {
        base.FinalizeRegistration();
        Initialize();
    }

    private void Initialize()
    {
        int maxLayer = 0;
        List<Action> actionList = new();
        foreach (Type registeredType in RegisteredTypes)
        {
            if (registeredType.ContainsGenericParameters || registeredType.IsAbstract)
            {
                continue;
            }

            ActionHandler actionHandler = (ActionHandler)Activator.CreateInstance(registeredType);
            Type actionType = actionHandler.ActionType;
            Action action = (Action)Activator.CreateInstance(actionType);
            actionList.Add(action);
            TypeID id = new(actionType);
            handlers.Add(id, actionHandler);
            maxLayer = Math.Max(maxLayer, action.layer);
        }

        Array.Resize(ref actions, actionList.Count);
        actionList.CopyTo(actions);
        layerCount = maxLayer + 1;
    }

    public static ActionHandler GetHandler(TypeID actionType)
    {
        return handlers[actionType];
    }

    public static bool TryGetHandler(TypeID actionType, out ActionHandler handler)
    {
        return handlers.TryGetValue(actionType, out handler);
    }
}