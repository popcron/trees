using GOAP;
using System;
using System.Collections.Generic;
using TypeRegistries;
using Action = GOAP.Action;

public class ActionRegistry : TypeRegistry<ActionHandler>
{
    private static readonly Dictionary<ulong, ActionHandler> handlers = new();
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
            ulong hash = actionType.GetID();
            TypeExtensions.types[hash] = actionType;
            handlers.Add(hash, actionHandler);
            maxLayer = Math.Max(maxLayer, action.layer);
        }

        Array.Resize(ref actions, actionList.Count);
        actionList.CopyTo(actions);
        layerCount = maxLayer + 1;
    }

    public static ActionHandler GetHandler(Type actionType)
    {
        return handlers[actionType.GetID()];
    }

    public static bool TryGetHandler(Type actionType, out ActionHandler handler)
    {
        return handlers.TryGetValue(actionType.GetID(), out handler);
    }
}