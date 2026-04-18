using System;
using System.Collections.Generic;

namespace GOAP
{
    [Serializable]
    public class Goal
    {
        public List<TypeID> wants = new();

        public void Wants<T>() where T : Fact
        {
            wants.Add(typeof(T));
        }
    }
}