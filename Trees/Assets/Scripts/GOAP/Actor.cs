using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public class Actor : MonoBehaviour
    {
        private static readonly List<Action> planList = new();

        public WorldState state = new();
        public List<Layer> layers = new();

        public void Reset()
        {
            layers.Clear();
            layers.Add(null);
        }

        public void Act(float delta)
        {
            Goal activeGoal = null;
            for (int i = 0; i < layers.Count; i++)
            {
                Layer layer = layers[i];
                int safety = 10;
                while (layer.IsActive && safety-- > 0)
                {
                    ActionPlan plan = layer.TopPlan;
                    if (!plan.IsActive)
                    {
                        layer.Pop();
                        continue;
                    }

                    Action action = plan.Current;
                    if (!ActionRegistry.TryGetHandler(action.GetType(), out ActionHandler handler))
                    {
                        layer.Clear();
                        break;
                    }

                    activeGoal = layer.TopGoal;
                    if (handler.TryComplete(this, layer, activeGoal, action, delta))
                    {
                        foreach (ulong t in action.removeFacts)
                        {
                            state.Remove(t);
                        }

                        foreach (ulong t in action.addFacts)
                        {
                            state.Add(t);
                        }

                        plan.Advance();
                        continue;
                    }

                    break;
                }
            }
        }

        public bool SubmitGoal<T>(T goal) where T : Goal
        {
            foreach (ulong wants in goal.wants)
            {
                state.Remove(wants);
            }

            if (Planner.TryPlan(state, goal.wants, planList))
            {
                Span<int> planLayers = stackalloc int[planList.Count];
                Planner.GetLayers(planList, planLayers);

                // find max layer in plan
                int maxLayer = 0;
                for (int i = 0; i < planLayers.Length; i++)
                {
                    maxLayer = Math.Max(maxLayer, planLayers[i]);
                }

                List<Action> layerActions = new();
                for (int i = 0; i <= maxLayer; i++)
                {
                    // create if missing
                    if (layers.Count <= i)
                    {
                        layers.Add(new());
                    }

                    for (int j = 0; j < planList.Count; j++)
                    {
                        if (planLayers[j] == i)
                        {
                            layerActions.Add(planList[j]);
                        }
                    }

                    if (layerActions.Count > 0)
                    {
                        layers[i].Set(goal, layerActions);
                        layerActions.Clear();
                    }
                }

                planList.Clear();
                return true;
            }
            else
            {
                planList.Clear();
                return false;
            }
        }

        public bool DispatchSubGoal<T>(Layer layer, T goal) where T : Goal
        {
            foreach (ulong wants in goal.wants)
            {
                state.Remove(wants);
            }

            if (Planner.TryPlan(state, goal.wants, planList))
            {
                layer.Push(goal, planList);
                planList.Clear();
                return true;
            }
            else
            {
                planList.Clear();
                return false;
            }
        }
    }
}