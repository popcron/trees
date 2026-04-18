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

            List<Action> result = Search(current, need, 0, maxDepth);
            if (result == null)
            {
                return false;
            }

            plan.AddRange(result);
            return true;
        }

        private static List<Action> Search(WorldState current, HashSet<ulong> need, int depth, int maxDepth)
        {
            if (need.Count == 0)
            {
                return new();
            }

            if (depth >= maxDepth)
            {
                return null;
            }

            List<Action> best = null;
            float bestCost = float.MaxValue;
            ReadOnlySpan<Action> actions = ActionRegistry.Actions;
            for (int i = 0; i < actions.Length; i++)
            {
                Action action = actions[i];
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

                List<Action> sub = Search(current, newNeed, depth + 1, maxDepth);
                if (sub == null)
                {
                    continue;
                }

                sub.Add(action);
                float total = 0f;
                for (int k = 0; k < sub.Count; k++)
                {
                    total += sub[k].cost;
                }

                if (total < bestCost)
                {
                    bestCost = total;
                    best = sub;
                }
            }

            return best;
        }
    }
}