using System;

namespace GOAP
{
    public abstract class ActionHandler
    {
        public abstract Type ActionType { get; }

        /// <summary>
        /// Tries to complete the action.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when the action is complete.
        /// </returns>
        public abstract bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta);
    }

    public abstract class ActionHandler<T> : ActionHandler where T : Action
    {
        public override Type ActionType => typeof(T);
    }
}