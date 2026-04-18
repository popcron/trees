using System;
using System.Collections.Generic;

namespace GOAP
{
    [Serializable]
    public class WorldState
    {
        public List<TypeID> facts = new();

        public WorldState()
        {
            facts = new List<TypeID>();
        }

        public WorldState(WorldState src)
        {
            facts = new List<TypeID>(src.facts);
        }

        public bool Has(TypeID factType)
        {
            return facts.Contains(factType);
        }

        public bool Has<T>() where T : Fact
        {
            return facts.Contains(typeof(T));
        }

        public void Add(TypeID factType)
        {
            facts.Add(factType);
        }

        public void Add<T>() where T : Fact
        {
            facts.Add(typeof(T));
        }

        public void Remove(TypeID factType)
        {
            facts.Remove(factType);
        }

        public void Remove<T>() where T : Fact
        {
            facts.Remove(typeof(T));
        }
    }
}