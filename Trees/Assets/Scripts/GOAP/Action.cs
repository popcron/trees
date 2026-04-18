using System;
using System.Collections.Generic;

namespace GOAP
{
    [Serializable]
    public class Action
    {
        public List<TypeID> preConditionFacts = new();
        public List<TypeID> addFacts = new();
        public List<TypeID> removeFacts = new();
        public float cost;
        public int layer;

        public void AddFact<T>() where T : Fact
        {
            addFacts.Add(typeof(T));
        }
    }
}
