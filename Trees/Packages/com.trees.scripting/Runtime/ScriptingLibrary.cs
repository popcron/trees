using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Scripting
{
    public static class ScriptingLibrary
    {
        public static readonly Interpreter interpreter = new();

        private static readonly List<Type> registeredTypeHandlers = new();
        private static readonly Dictionary<Type, SerializeDelegate> serializeFunctions = new();
        private static readonly Dictionary<Type, DeserializeDelegate> deserializeFunctions = new();

        static ScriptingLibrary()
        {
            OptionalInitialization();
        }

        private static void OptionalInitialization()
        {
            Type initializerType = Type.GetType("ScriptingLoader, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (initializerType != null)
            {
                RuntimeHelpers.RunClassConstructor(initializerType.TypeHandle);
            }
        }

        internal static ulong GetHash(ReadOnlySpan<char> text)
        {
            const ulong FnvOffsetBasis = 14695981039346656037;
            const ulong FnvPrime = 1099511628211;
            ulong result = FnvOffsetBasis;
            for (int i = 0; i < text.Length; i++)
            {
                result ^= text[i];
                result *= FnvPrime;
            }

            return result;
        }

        public static bool IsTypeHandlerRegistered(Type type)
        {
            return registeredTypeHandlers.Contains(type);
        }

        public static void RegisterTypeHandler<T>(ITypeHandler<T> typeHandler)
        {
            Type type = typeof(T);
            if (IsTypeHandlerRegistered(type))
            {
                throw new InvalidOperationException($"A type handler for type {type} is already registered.");
            }

            registeredTypeHandlers.Add(type);
            TypeHandler<T>.registered = true;
            TypeHandler<T>.deserialize = typeHandler.Deserialize;

            serializeFunctions.Add(type, (value) =>
            {
                return typeHandler.Serialize((T)value);
            });

            deserializeFunctions.Add(type, (value) =>
            {
                return typeHandler.Deserialize(value);
            });
        }

        public static void ThrowIfTypeHandlerNotRegistered(Type type)
        {
            if (!IsTypeHandlerRegistered(type))
            {
                throw new InvalidOperationException($"No type handler is registered for type {type}.");
            }
        }

        public static void ThrowIfTypeHandlerNotRegistered<T>()
        {
            if (!TypeHandler<T>.registered)
            {
                throw new InvalidOperationException($"No type handler is registered for type {typeof(T)}.");
            }
        }

        internal static Value Serialize(object value)
        {
            if (value == null)
            {
                return Value.Null;
            }

            Type type = value.GetType();
            ThrowIfTypeHandlerNotRegistered(type);

            return serializeFunctions[type](value);
        }

        internal static object Deserialize(Type type, Value input)
        {
            if (type == typeof(bool))
            {
                return AsBoolean(input);
            }
            else if (type == typeof(byte))
            {
                return (byte)AsInteger64(input);
            }
            else if (type == typeof(sbyte))
            {
                return (sbyte)AsInteger64(input);
            }
            else if (type == typeof(short))
            {
                return (short)AsInteger64(input);
            }
            else if (type == typeof(ushort))
            {
                return (ushort)AsInteger64(input);
            }
            else if (type == typeof(int))
            {
                return (int)AsInteger64(input);
            }
            else if (type == typeof(uint))
            {
                return (uint)AsInteger64(input);
            }
            else if (type == typeof(long))
            {
                return AsInteger64(input);
            }
            else if (type == typeof(ulong))
            {
                return (ulong)AsInteger64(input);
            }
            else if (type == typeof(float))
            {
                return (float)AsFloat64(input);
            }
            else if (type == typeof(double))
            {
                return AsFloat64(input);
            }
            else if (type == typeof(string))
            {
                return input.ToString();
            }
            else if (type == typeof(char))
            {
                return AsCharacter(input);
            }

            ThrowIfTypeHandlerNotRegistered(type);

            return deserializeFunctions[type](input);
        }

        internal static Value Serialize<T>(T value)
        {
            if (value == null)
            {
                return Value.Null;
            }

            switch (value)
            {
                case bool boolValue:
                    return new(boolValue);
                case byte byteValue:
                    return new(byteValue);
                case sbyte sbyteValue:
                    return new(sbyteValue);
                case short shortValue:
                    return new(shortValue);
                case ushort ushortValue:
                    return new(ushortValue);
                case int intValue:
                    return new(intValue);
                case uint uintValue:
                    return new(uintValue);
                case long longValue:
                    return new(longValue);
                case ulong ulongValue:
                    return new((long)ulongValue);
                case float floatValue:
                    return new(floatValue);
                case double doubleValue:
                    return new(doubleValue);
                case string stringValue:
                    return new(stringValue);
                case char charValue:
                    return new(charValue);
            }

            ThrowIfTypeHandlerNotRegistered<T>();

            Type type = value.GetType();
            if (serializeFunctions.TryGetValue(type, out SerializeDelegate serialize))
            {
                return serialize(value);
            }

            // didnt find exact handler, find a handler for a base type if possible and cache it for next time
            Type current = type.BaseType;
            while (current != null && current != typeof(object))
            {
                if (serializeFunctions.TryGetValue(current, out serialize))
                {
                    serializeFunctions[type] = serialize;
                    return serialize(value);
                }

                current = current.BaseType;
            }

            throw new InvalidOperationException($"No type handler is registered for type {type} or any of its base types.");
        }

        internal static T Deserialize<T>(Value input)
        {
            Type type = typeof(T);
            if (type == typeof(bool))
            {
                bool boolValue = AsBoolean(input);
                return Unsafe.As<bool, T>(ref boolValue);
            }
            else if (type == typeof(byte))
            {
                byte byteValue = (byte)AsInteger64(input);
                return Unsafe.As<byte, T>(ref byteValue);
            }
            else if (type == typeof(sbyte))
            {
                sbyte sbyteValue = (sbyte)AsInteger64(input);
                return Unsafe.As<sbyte, T>(ref sbyteValue);
            }
            else if (type == typeof(short))
            {
                short shortValue = (short)AsInteger64(input);
                return Unsafe.As<short, T>(ref shortValue);
            }
            else if (type == typeof(ushort))
            {
                ushort ushortValue = (ushort)AsInteger64(input);
                return Unsafe.As<ushort, T>(ref ushortValue);
            }
            else if (type == typeof(int))
            {
                int intValue = (int)AsInteger64(input);
                return Unsafe.As<int, T>(ref intValue);
            }
            else if (type == typeof(uint))
            {
                uint uintValue = (uint)AsInteger64(input);
                return Unsafe.As<uint, T>(ref uintValue);
            }
            else if (type == typeof(long))
            {
                long longValue = AsInteger64(input);
                return Unsafe.As<long, T>(ref longValue);
            }
            else if (type == typeof(ulong))
            {
                ulong ulongValue = (ulong)AsInteger64(input);
                return Unsafe.As<ulong, T>(ref ulongValue);
            }
            else if (type == typeof(float))
            {
                float floatValue = (float)AsFloat64(input);
                return Unsafe.As<float, T>(ref floatValue);
            }
            else if (type == typeof(double))
            {
                double doubleValue = AsFloat64(input);
                return Unsafe.As<double, T>(ref doubleValue);
            }
            else if (type == typeof(string))
            {
                string stringValue = input.ToString();
                return Unsafe.As<string, T>(ref stringValue);
            }
            else if (type == typeof(char))
            {
                char charValue = AsCharacter(input);
                return Unsafe.As<char, T>(ref charValue);
            }

            ThrowIfTypeHandlerNotRegistered<T>();

            return TypeHandler<T>.deserialize(input);
        }

        private static char AsCharacter(Value value)
        {
            if (value.type == Value.Type.Boolean)
            {
                return value.boolValue ? '1' : '0';
            }
            else if (value.type == Value.Type.Character)
            {
                return value.characterValue;
            }
            else if (value.type == Value.Type.String)
            {
                return value.stringValue[0];
            }
            else if (value.type == Value.Type.Float)
            {
                return (char)(int)value.doubleValue;
            }
            else if (value.type == Value.Type.Integer)
            {
                return (char)value.longValue;
            }
            else if (value.type == Value.Type.Object)
            {
                return value.objectValue != null ? '1' : '0';
            }
            else
            {
                return '\0';
            }
        }

        private static bool AsBoolean(Value value)
        {
            if (value.type == Value.Type.Boolean)
            {
                return value.boolValue;
            }
            else if (value.type == Value.Type.Character)
            {
                return value.characterValue != '\0';
            }
            else if (value.type == Value.Type.String)
            {
                return value.stringValue.Equals("true", StringComparison.OrdinalIgnoreCase) || value.stringValue == "1";
            }
            else if (value.type == Value.Type.Float)
            {
                return value.doubleValue > 0;
            }
            else if (value.type == Value.Type.Integer)
            {
                return value.longValue > 0;
            }
            else if (value.type == Value.Type.Object)
            {
                return value.objectValue != null;
            }
            else
            {
                return false;
            }
        }

        private static long AsInteger64(Value value)
        {
            if (value.type == Value.Type.Boolean)
            {
                return value.boolValue ? 1 : 0;
            }
            else if (value.type == Value.Type.Character)
            {
                return value.characterValue;
            }
            else if (value.type == Value.Type.String)
            {
                if (value.stringValue != null && long.TryParse(value.stringValue, out long result))
                {
                    return result;
                }
                else
                {
                    throw new InvalidOperationException($"Text value '{value.stringValue}' could not be parsed as an integer.");
                }
            }
            else if (value.type == Value.Type.Float)
            {
                return (long)value.doubleValue;
            }
            else if (value.type == Value.Type.Integer)
            {
                return value.longValue;
            }
            else if (value.type == Value.Type.Object)
            {
                return value.objectValue != null ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        private static double AsFloat64(Value value)
        {
            if (value.type == Value.Type.Boolean)
            {
                return value.boolValue ? 1 : 0;
            }
            else if (value.type == Value.Type.Character)
            {
                return value.characterValue;
            }
            else if (value.type == Value.Type.String)
            {
                if (value.stringValue != null && double.TryParse(value.stringValue, out double result))
                {
                    return result;
                }
                else
                {
                    throw new InvalidOperationException($"Text value '{value.stringValue}' could not be parsed as a decimal.");
                }
            }
            else if (value.type == Value.Type.Float)
            {
                return value.doubleValue;
            }
            else if (value.type == Value.Type.Integer)
            {
                return value.longValue;
            }
            else if (value.type == Value.Type.Object)
            {
                return value.objectValue != null ? 1 : 0;
            }
            else
            {
                return 0;
            }
        }

        private static class TypeHandler<T>
        {
            public static bool registered;
            public static DeserializeDelegate<T> deserialize;
        }

        private delegate Value SerializeDelegate<T>(T value);
        private delegate T DeserializeDelegate<T>(Value input);
        private delegate Value SerializeDelegate(object value);
        private delegate object DeserializeDelegate(Value source);
    }
}