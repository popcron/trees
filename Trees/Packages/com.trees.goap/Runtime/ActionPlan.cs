using System;
using System.Collections.Generic;

namespace GOAP
{
    [Serializable]
    public class ActionPlan
    {
        public int step;
        public List<Action> plan = new();

        public bool IsActive => plan.Count > 0 && step < plan.Count;
        public Action Current => IsActive ? plan[step] : default;

        public void Reset()
        {
            step = 0;
            plan.Clear();
        }

        public void Load(IEnumerable<Action> actions)
        {
            step = 0;
            plan.Clear();
            plan.AddRange(actions);
        }

        public void Advance()
        {
            step++;
        }
    }
}