#if UNITY_5_3_OR_NEWER
using UnityEngine;

namespace Scripting
{
    public class SetActiveScript : ScriptBehaviour
    {
        public GameObject target;
        public SourceCode<bool> expression = true;

        private void Update()
        {
            if (target != null)
            {
                UpdateBindings();
                Value value = interpreter.Evaluate(expression.content);
                target.SetActive(value.Deserialize<bool>());
            }
        }
    }
}
#endif