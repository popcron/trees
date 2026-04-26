#if UNITY_5_3_OR_NEWER
using System;
using System.Reflection;
using UnityEngine;

namespace Scripting
{
    [ExecuteAlways]
    public class ScriptBehaviour : MonoBehaviour
    {
        [SerializeField]
        private ObjectBinding[] objectBindings = { };

        public readonly Interpreter interpreter = new();

        public virtual void UpdateBindings()
        {
            interpreter.ClearBindings();
            interpreter.AddBindings(ScriptingLibrary.interpreter);
            for (int i = 0; i < objectBindings.Length; i++)
            {
                ObjectBinding objectBinding = objectBindings[i];
                if (objectBinding.target != null)
                {
                    interpreter.DeclareBinding(objectBinding.name, Read(i), Write(i));
                }
            }
        }

        private Action<Value> Write(int i)
        {
            return (value) =>
            {
                ref ObjectBinding objectBinding = ref objectBindings[i];
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
                    Type parameterType = parameters[0].ParameterType;
                    object deserializedValue = value.Deserialize(parameterType);
                    method.Invoke(objectBinding.target, new object[] { deserializedValue });
                }
                else
                {
                    Type targetType = objectBinding.target.GetType();
                    throw new InvalidOperationException($"Unsupported member type '{memberInfo.MemberType}' for member '{objectBinding.member}' on object '{targetType}'");
                }
            };
        }

        private Func<Value> Read(int i)
        {
            return () =>
            {
                ref ObjectBinding objectBinding = ref objectBindings[i];
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
                    Type targetType = objectBinding.target.GetType();
                    throw new InvalidOperationException($"Unsupported member type '{memberInfo.MemberType}' for member '{objectBinding.member}' on object '{targetType}'");
                }
            };
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
                    Type targetType = target.GetType();
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