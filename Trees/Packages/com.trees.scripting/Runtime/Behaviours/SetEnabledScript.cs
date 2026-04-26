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
                Value value = interpreter.Evaluate(expression.content);
                target.enabled = value.Deserialize<bool>();
            }
        }
    }
}
#endif