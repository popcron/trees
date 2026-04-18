#if UNITY_5_3_OR_NEWER
using System;
using System.Reflection;
using UnityEngine;

namespace Scripting
{
    [ExecuteAlways]
    public abstract class ScriptBehaviour : MonoBehaviour, IContainsInterpreter
    {
        [SerializeField]
        private ObjectBinding[] objectBindings = { };

        private readonly Interpreter interpreter = new();

        Interpreter IContainsInterpreter.Interpreter => interpreter;

        public Value Evaluate(ReadOnlySpan<char> sourceCode)
        {
            return interpreter.Evaluate(sourceCode);
        }

        public Value Evaluate(SourceCode sourceCode)
        {
            return interpreter.Evaluate(sourceCode.content);
        }

        public T Evaluate<T>(SourceCode<T> sourceCode, T defaultValue = default)
        {
            Value value = interpreter.Evaluate(sourceCode.content);
            return value.Deserialize(defaultValue);
        }

        public void UpdateBindings()
        {
            interpreter.ClearBindings();
            interpreter.AddBindings(ScriptingLibrary.interpreter);
            for (int i = 0; i < objectBindings.Length; i++)
            {
                ObjectBinding objectBinding = objectBindings[i];
                if (objectBinding.target != null)
                {
                    System.Type targetType = objectBinding.target.GetType();
                    interpreter.DeclareBinding(objectBinding.name, () =>
                    {
                        MemberInfo memberInfo = objectBinding.GetMember();
                        if (memberInfo is FieldInfo field)
                        {
                            object fieldValue = field.GetValue(objectBinding.target);
                            return Value.Serialize(fieldValue);
                        }
                        else if (memberInfo is PropertyInfo property)
                        {
                            object propertyValue = property.GetValue(objectBinding.target);
                            return Value.Serialize(propertyValue);
                        }
                        else if (memberInfo is MethodInfo method)
                        {
                            object methodValue = method.Invoke(objectBinding.target, null);
                            return Value.Serialize(methodValue);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unsupported member type '{memberInfo.MemberType}' for member '{objectBinding.member}' on object '{targetType}'");
                        }
                    },
                    (value) =>
                    {
                        MemberInfo memberInfo = objectBinding.GetMember();
                        if (memberInfo is FieldInfo field)
                        {
                            object deserializedValue = value.Deserialize(field.FieldType);
                            field.SetValue(objectBinding.target, deserializedValue);
                        }
                        else if (memberInfo is PropertyInfo property)
                        {
                            object deserializedValue = value.Deserialize(property.PropertyType);
                            property.SetValue(objectBinding.target, deserializedValue);
                        }
                        else if (memberInfo is MethodInfo method)
                        {
                            ParameterInfo[] parameters = method.GetParameters();
                            System.Type parameterType = parameters[0].ParameterType;
                            object deserializedValue = value.Deserialize(parameterType);
                            method.Invoke(objectBinding.target, new object[] { deserializedValue });
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unsupported member type '{memberInfo.MemberType}' for member '{objectBinding.member}' on object '{targetType}'");
                        }
                    });
                }
            }
        }

        [Serializable]
        public struct ObjectBinding
        {
            private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            public string name;
            public UnityEngine.Object target;
            public string member;

            public readonly MemberInfo GetMember()
            {
                if (target != null)
                {
                    System.Type targetType = target.GetType();
                    FieldInfo field = targetType.GetField(member, Flags);
                    if (field != null)
                    {
                        return field;
                    }
                    else
                    {
                        PropertyInfo property = targetType.GetProperty(member, Flags);
                        if (property != null)
                        {
                            return property;
                        }
                        else
                        {
                            MethodInfo method = targetType.GetMethod(member, Flags);
                            if (method != null)
                            {
                                return method;
                            }
                        }
                    }

                    throw new NullReferenceException($"Failed to find member '{member}' on object '{targetType}'");
                }
                else
                {
                    throw new NullReferenceException($"Object binding '{name}' is null");
                }
            }
        }
    }
}
#endif