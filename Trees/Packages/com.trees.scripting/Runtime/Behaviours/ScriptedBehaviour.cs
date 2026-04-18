#if UNITY_5_3_OR_NEWER
using System;

namespace Scripting
{
    public class ScriptedBehaviour : ScriptBehaviour
    {
        public TriggerEvent triggerEvent = TriggerEvent.Start;
        public BindEvent bindEvent = BindEvent.OnEnable;
        public SourceCode expression;

        protected virtual void Awake()
        {
            if ((triggerEvent & TriggerEvent.Awake) != 0)
            {
                Evaluate(expression);
            }
        }

        protected virtual void Start()
        {
            if ((triggerEvent & TriggerEvent.Start) != 0)
            {
                Evaluate(expression);
            }
        }

        protected virtual void OnEnable()
        {
            if ((bindEvent & BindEvent.OnEnable) != 0)
            {
                UpdateBindings();
            }

            if ((triggerEvent & TriggerEvent.OnEnable) != 0)
            {
                Evaluate(expression);
            }
        }

        protected virtual void OnDisable()
        {
            if ((triggerEvent & TriggerEvent.OnDisable) != 0)
            {
                Evaluate(expression);
            }
        }

        protected virtual void Update()
        {
            if ((bindEvent & BindEvent.Update) != 0)
            {
                UpdateBindings();
            }

            if ((triggerEvent & TriggerEvent.Update) != 0)
            {
                Evaluate(expression);
            }
        }

        protected virtual void FixedUpdate()
        {
            if ((bindEvent & BindEvent.FixedUpdate) != 0)
            {
                UpdateBindings();
            }

            if ((triggerEvent & TriggerEvent.FixedUpdate) != 0)
            {
                Evaluate(expression);
            }
        }

        protected virtual void LateUpdate()
        {
            if ((bindEvent & BindEvent.LateUpdate) != 0)
            {
                UpdateBindings();
            }

            if ((triggerEvent & TriggerEvent.LateUpdate) != 0)
            {
                Evaluate(expression);
            }
        }

        protected virtual void OnGUI()
        {
            if ((triggerEvent & TriggerEvent.OnGUI) != 0)
            {
                Evaluate(expression);
            }
        }

        protected virtual void OnDestroy()
        {
            if ((triggerEvent & TriggerEvent.OnDestroy) != 0)
            {
                Evaluate(expression);
            }
        }

        [Flags]
        public enum TriggerEvent
        {
            None = 0,
            Start = 1,
            Awake = 2,
            OnEnable = 4,
            OnDisable = 8,
            Update = 16,
            FixedUpdate = 32,
            LateUpdate = 64,
            OnGUI = 128,
            OnDestroy = 256,
        }

        [Flags]
        public enum BindEvent
        {
            None = 0,
            OnEnable = 2,
            Update = 4,
            FixedUpdate = 8,
            LateUpdate = 16,
        }
    }
}
#endif