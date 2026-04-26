using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GOAP
{
    public static class ActionRegistry
    {
        public static readonly Dictionary<ulong, ActionHandler> handlers = new();
        public static readonly List<Action> actions = new();

        static ActionRegistry()
        {
            OptionalInitialization();
        }

        private static void OptionalInitialization()
        {
            Type initializerType = Type.GetType("ActionRegistryLoader, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (initializerType != null)
            {
                RuntimeHelpers.RunClassConstructor(initializerType.TypeHandle);
            }
        }

        public static void Register(ActionHandler actionHandler)
        {
            Type actionType = actionHandler.ActionType;
            Action action = (Action)Activator.CreateInstance(actionType);
            ulong hash = actionType.GetID();
            TypeExtensions.types[hash] = actionType;
            handlers.Add(hash, actionHandler);
            actions.Add(action);
        }

        public static ActionHandler GetHandler(Type actionType)
        {
            return handlers[actionType.GetID()];
        }

        public static ActionHandler GetHandler<T>() where T : Action
        {
            return handlers[typeof(T).GetID()];
        }

        public static bool TryGetHandler(Type actionType, out ActionHandler handler)
        {
            return handlers.TryGetValue(actionType.GetID(), out handler);
        }

        public static bool TryGetHandler<T>(out ActionHandler handler) where T : Action
        {
            return handlers.TryGetValue(typeof(T).GetID(), out handler);
        }
    }
}