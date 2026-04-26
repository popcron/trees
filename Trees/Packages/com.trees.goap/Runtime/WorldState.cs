using System;
using System.Collections.Generic;

namespace GOAP
{
    [Serializable]
    public class WorldState
    {
        public List<ulong> facts = new();

        public WorldState()
        {
            facts = new List<ulong>();
        }

        public WorldState(WorldState src)
        {
            facts = new List<ulong>(src.facts);
        }

        public bool Has(ulong factType)
        {
            return facts.Contains(factType);
        }

        public bool Has<T>() where T : Fact
        {
            return facts.Contains(typeof(T).GetID());
        }

        public void Add(ulong factType)
        {
            facts.Add(factType);
        }

        public void Add<T>() where T : Fact
        {
            facts.Add(typeof(T).GetID());
        }

        public void Remove(ulong factType)
        {
            facts.Remove(factType);
        }

        public void Remove<T>() where T : Fact
        {
            facts.Remove(typeof(T).GetID());
        }
    }
}