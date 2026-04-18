using System;
using System.Collections.Generic;

namespace GOAP
{
    [Serializable]
    public class Layer
    {
        public List<Goal> goals = new();
        public List<ActionPlan> plans = new();

        public bool IsActive => goals.Count > 0;
        public Goal TopGoal => goals[^1];
        public ActionPlan TopPlan => plans[^1];

        public void Set(Goal goal, IEnumerable<Action> actions)
        {
            Clear();
            Push(goal, actions);
        }

        public void Push(Goal goal, IEnumerable<Action> actions)
        {
            ActionPlan plan = new();
            plan.Load(actions);
            goals.Add(goal);
            plans.Add(plan);
        }

        public void Pop()
        {
            goals.RemoveAt(goals.Count - 1);
            plans.RemoveAt(plans.Count - 1);
        }

        public void Clear()
        {
            goals.Clear();
            plans.Clear();
        }
    }
}
