using System;
using System.Collections.Generic;

namespace GOAP
{
    public static class Planner
    {
        public static void GetLayers(IEnumerable<Action> plan, Span<int> layers)
        {
            int index = 0;
            foreach (Action layer in plan)
            {
                layers[index++] = layer.layer;
            }
        }

        public static bool TryPlan(WorldState current, IEnumerable<ulong> goalFacts, List<Action> plan, int maxDepth = 20)
        {
            HashSet<ulong> need = new();
            foreach (ulong f in goalFacts)
            {
                if (!current.Has(f))
                {
                    need.Add(f);
                }
            }

            if (need.Count == 0)
            {
                return true;
            }

            List<Action> result = new();
            if (!Search(current, need, 0, maxDepth, result))
            {
                return false;
            }

            plan.AddRange(result);
            return true;
        }

        private static bool Search(WorldState current, HashSet<ulong> need, int depth, int maxDepth, List<Action> result)
        {
            if (need.Count == 0)
            {
                return true;
            }

            if (depth >= maxDepth)
            {
                return false;
            }

            List<Action> candidate = new();
            float bestCost = float.MaxValue;
            bool found = false;
            for (int i = 0; i < ActionRegistry.actions.Count; i++)
            {
                Action action = ActionRegistry.actions[i];
                bool helps = false;
                foreach (ulong eff in action.addFacts)
                {
                    if (need.Contains(eff))
                    {
                        helps = true;
                        break;
                    }
                }

                if (!helps)
                {
                    continue;
                }

                bool conflicts = false;
                foreach (ulong rem in action.removeFacts)
                {
                    if (need.Contains(rem))
                    {
                        conflicts = true;
                        break;
                    }
                }

                if (conflicts)
                {
                    continue;
                }

                HashSet<ulong> newNeed = new(need);
                foreach (ulong eff in action.addFacts)
                {
                    newNeed.Remove(eff);
                }

                foreach (ulong pre in action.preConditionFacts)
                {
                    if (!current.Has(pre))
                    {
                        newNeed.Add(pre);
                    }
                }

                candidate.Clear();
                if (!Search(current, newNeed, depth + 1, maxDepth, candidate))
                {
                    continue;
                }

                candidate.Add(action);
                float total = 0f;
                for (int k = 0; k < candidate.Count; k++)
                {
                    total += candidate[k].cost;
                }

                if (total < bestCost)
                {
                    bestCost = total;
                    result.Clear();
                    result.AddRange(candidate);
                    found = true;
                }
            }

            return found;
        }
    }
}