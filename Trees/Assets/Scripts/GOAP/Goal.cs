using System;
using System.Collections.Generic;

namespace GOAP
{
    [Serializable]
    public class Goal
    {
        [TypeDropdown(typeof(Fact))]
        public List<ulong> wants = new();

        public void Wants<T>() where T : Fact
        {
            wants.Add(typeof(T).GetID());
        }
    }
}