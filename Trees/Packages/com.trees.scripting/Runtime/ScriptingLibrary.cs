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
        private static readonly Dictionary<Type, TryDeserializeDelegate> tryDeserialize = new();

        static ScriptingLibrary()
        {
            OptionalInitialization();
        }

        private static void OptionalInitialization()
        {
            Type initializerType = Type.GetType("InterpreterBindings, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
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
            TypeHandler<T>.tryDeserialize = typeHandler.TryDeserialize;

            serializeFunctions.Add(type, (value) =>
            {
                return typeHandler.Serialize((T)value);
            });

            tryDeserialize.Add(type, (Value input, out object output) =>
            {
                bool success = typeHandler.TryDeserialize(input, out T typedResult);
                output = typedResult;
                return success;
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

        internal static bool TryDeserialize(Type type, Value input, out object result)
        {
            if (type == typeof(bool))
            {
                result = AsBoolean(input);
                return true;
            }
            else if (type == typeof(byte))
            {
                result = (byte)AsLong(input);
                return true;
            }
            else if (type == typeof(sbyte))
            {
                result = (sbyte)AsLong(input);
                return true;
            }
            else if (type == typeof(short))
            {
                result = (short)AsLong(input);
                return true;
            }
            else if (type == typeof(ushort))
            {
                result = (ushort)AsLong(input);
                return true;
            }
            else if (type == typeof(int))
            {
                result = (int)AsLong(input);
                return true;
            }
            else if (type == typeof(uint))
            {
                result = (uint)AsLong(input);
                return true;
            }
            else if (type == typeof(long))
            {
                result = AsLong(input);
                return true;
            }
            else if (type == typeof(ulong))
            {
                result = (ulong)AsLong(input);
                return true;
            }
            else if (type == typeof(float))
            {
                result = (float)AsDouble(input);
                return true;
            }
            else if (type == typeof(double))
            {
                result = AsDouble(input);
                return true;
            }
            else if (type == typeof(string))
            {
                result = input.ToString();
                return true;
            }
            else if (type == typeof(char))
            {
                result = AsCharacter(input);
                return true;
            }

            ThrowIfTypeHandlerNotRegistered(type);

            return tryDeserialize[type](input, out result);
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

        internal static bool TryDeserialize<T>(Value input, out T result)
        {
            Type type = typeof(T);
            if (type == typeof(bool))
            {
                bool boolValue = AsBoolean(input);
                result = Unsafe.As<bool, T>(ref boolValue);
                return true;
            }
            else if (type == typeof(byte))
            {
                byte byteValue = (byte)AsLong(input);
                result = Unsafe.As<byte, T>(ref byteValue);
                return true;
            }
            else if (type == typeof(sbyte))
            {
                sbyte sbyteValue = (sbyte)AsLong(input);
                result = Unsafe.As<sbyte, T>(ref sbyteValue);
                return true;
            }
            else if (type == typeof(short))
            {
                short shortValue = (short)AsLong(input);
                result = Unsafe.As<short, T>(ref shortValue);
                return true;
            }
            else if (type == typeof(ushort))
            {
                ushort ushortValue = (ushort)AsLong(input);
                result = Unsafe.As<ushort, T>(ref ushortValue);
                return true;
            }
            else if (type == typeof(int))
            {
                int intValue = (int)AsLong(input);
                result = Unsafe.As<int, T>(ref intValue);
                return true;
            }
            else if (type == typeof(uint))
            {
                uint uintValue = (uint)AsLong(input);
                result = Unsafe.As<uint, T>(ref uintValue);
                return true;
            }
            else if (type == typeof(long))
            {
                long longValue = AsLong(input);
                result = Unsafe.As<long, T>(ref longValue);
                return true;
            }
            else if (type == typeof(ulong))
            {
                ulong ulongValue = (ulong)AsLong(input);
                result = Unsafe.As<ulong, T>(ref ulongValue);
                return true;
            }
            else if (type == typeof(float))
            {
                float floatValue = (float)AsDouble(input);
                result = Unsafe.As<float, T>(ref floatValue);
                return true;
            }
            else if (type == typeof(double))
            {
                double doubleValue = AsDouble(input);
                result = Unsafe.As<double, T>(ref doubleValue);
                return true;
            }
            else if (type == typeof(string))
            {
                string stringValue = input.ToString();
                result = Unsafe.As<string, T>(ref stringValue);
                return true;
            }
            else if (type == typeof(char))
            {
                char charValue = AsCharacter(input);
                result = Unsafe.As<char, T>(ref charValue);
                return true;
            }

            ThrowIfTypeHandlerNotRegistered<T>();

            return TypeHandler<T>.tryDeserialize(input, out result);
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
            else if (value.type == Value.Type.Double)
            {
                return (char)(int)value.doubleValue;
            }
            else if (value.type == Value.Type.Long)
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
            else if (value.type == Value.Type.Double)
            {
                return value.doubleValue > 0;
            }
            else if (value.type == Value.Type.Long)
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

        private static long AsLong(Value value)
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
            else if (value.type == Value.Type.Double)
            {
                return (long)value.doubleValue;
            }
            else if (value.type == Value.Type.Long)
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

        private static double AsDouble(Value value)
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
            else if (value.type == Value.Type.Double)
            {
                return value.doubleValue;
            }
            else if (value.type == Value.Type.Long)
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
            public static TryDeserializeDelegate<T> tryDeserialize;
        }

        private delegate Value SerializeDelegate<T>(T value);
        private delegate bool TryDeserializeDelegate<T>(Value input, out T output);
        private delegate Value SerializeDelegate(object value);
        private delegate bool TryDeserializeDelegate(Value source, out object result);
    }
}