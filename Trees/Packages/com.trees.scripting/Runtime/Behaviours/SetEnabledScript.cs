#if UNITY_5_3_OR_NEWER
using UnityEngine;

namespace Scripting
{
    public class SetEnabledScript : ScriptBehaviour
    {
        public Behaviour target;
        public SourceCode<bool> expression = true;

        private void Update()
        {
            if (target != null)
            {
                UpdateBindings();
                target.enabled = Evaluate(expression);
            }
        }
    }
}
#endif