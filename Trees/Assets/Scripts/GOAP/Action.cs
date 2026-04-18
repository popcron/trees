using System;
using System.Collections.Generic;

namespace GOAP
{
    [Serializable]
    public class Action
    {
        [TypeDropdown(typeof(Fact))]
        public List<ulong> preConditionFacts = new();

        [TypeDropdown(typeof(Fact))]
        public List<ulong> addFacts = new();

        [TypeDropdown(typeof(Fact))]
        public List<ulong> removeFacts = new();

        public float cost;
        public int layer;

        public void AddFact<T>() where T : Fact
        {
            addFacts.Add(typeof(T).GetID());
        }
    }
}
