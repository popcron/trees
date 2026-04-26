using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Scripting
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Value : IEquatable<Value>
    {
        public static readonly Value Null = new(Type.Object);
        public static readonly Value True = new(true);
        public static readonly Value False = new(false);

        [FieldOffset(0)]
        public readonly Type type;

        [FieldOffset(4)]
        public bool boolValue;

        [FieldOffset(4)]
        public char characterValue;

        [FieldOffset(4)]
        public double doubleValue;

        [FieldOffset(4)]
        public long longValue;

        [FieldOffset(16)]
        public string stringValue;

        [FieldOffset(16)]
        public ObjectInstance objectValue;

        [FieldOffset(16)]
        public FunctionValue functionValue;

        [FieldOffset(16)]
        public TypeValue typeValue;

        [FieldOffset(16)]
        public Func<Value[], Value> nativeFunctionValue;

        public Value(Type type)
        {
            this.type = type;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            functionValue = default;
            typeValue = default;
            objectValue = default;
            nativeFunctionValue = default;
        }

        public Value(bool value)
        {
            this.type = Type.Boolean;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            functionValue = default;
            typeValue = default;
            objectValue = default;
            nativeFunctionValue = default;
            boolValue = value;
        }

        public Value(char value)
        {
            this.type = Type.Character;
            boolValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            functionValue = default;
            typeValue = default;
            objectValue = default;
            nativeFunctionValue = default;
            characterValue = value;
        }

        public Value(string value)
        {
            this.type = Type.String;
            boolValue = default;
            characterValue = default;
            doubleValue = default;
            longValue = default;
            functionValue = default;
            typeValue = default;
            objectValue = default;
            nativeFunctionValue = default;
            stringValue = value;
        }

        public Value(double value)
        {
            this.type = Type.Float;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            longValue = default;
            functionValue = default;
            typeValue = default;
            objectValue = default;
            nativeFunctionValue = default;
            doubleValue = value;
        }

        public Value(long value)
        {
            this.type = Type.Integer;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            functionValue = default;
            typeValue = default;
            objectValue = default;
            nativeFunctionValue = default;
            longValue = value;
        }

        public Value(ObjectInstance value)
        {
            this.type = Type.Object;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            functionValue = default;
            typeValue = default;
            nativeFunctionValue = default;
            objectValue = value;
        }

        public Value(TypeValue value)
        {
            this.type = Type.Type;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            objectValue = default;
            functionValue = default;
            nativeFunctionValue = default;
            typeValue = value;
        }

        public Value(FunctionValue value)
        {
            this.type = Type.Function;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            objectValue = default;
            typeValue = default;
            nativeFunctionValue = default;
            functionValue = value;
        }

        public Value(Func<Value[], Value> value)
        {
            this.type = Type.NativeFunction;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            objectValue = default;
            typeValue = default;
            functionValue = default;
            nativeFunctionValue = value;
        }

        public readonly void ThrowIfNot(Type expectedType)
        {
            if (type != expectedType)
            {
                throw new InvalidOperationException($"Value was expected to be of type {expectedType}, but was {type}");
            }
        }

        public readonly override string ToString()
        {
            StringBuilder stringBuilder = StringBuilderPool.Rent();
            Append(stringBuilder);
            return StringBuilderPool.ToStringAndReturn(stringBuilder);
        }

        public readonly void Append(StringBuilder stringBuilder, int depth = 0)
        {
            switch (type)
            {
                case Type.Boolean:
                    stringBuilder.Append(boolValue ? KeywordMap.True : KeywordMap.False);
                    break;
                case Type.Character:
                    stringBuilder.Append(characterValue);
                    break;
                case Type.String:
                    stringBuilder.Append(stringValue);
                    break;
                case Type.Float:
                    stringBuilder.Append(doubleValue);
                    break;
                case Type.Integer:
                    stringBuilder.Append(longValue);
                    break;
                case Type.Object:
                    if (objectValue != null)
                    {
                        objectValue.Append(stringBuilder, depth);
                    }
                    else
                    {
                        stringBuilder.Append(KeywordMap.Null);
                    }
                    break;
                case Type.Function:
                    stringBuilder.Append(KeywordMap.FunctionDeclaration);
                    stringBuilder.Append(' ');
                    stringBuilder.Append(functionValue.symbol.name);
                    break;
                case Type.Type:
                    stringBuilder.Append(KeywordMap.TypeDeclaration);
                    stringBuilder.Append(' ');
                    stringBuilder.Append(typeValue.symbol.name);
                    break;
                case Type.NativeFunction:
                    stringBuilder.Append(nativeFunctionValue.ToString());
                    break;
                default:
                    stringBuilder.Append(base.ToString());
                    break;
            }
        }

        public readonly override bool Equals(object obj)
        {
            return obj is Value variable && Equals(variable);
        }

        public readonly bool Equals(Value other)
        {
            if (type != other.type)
            {
                return false;
            }

            switch (type)
            {
                case Type.Boolean:
                    return boolValue == other.boolValue;
                case Type.Character:
                    return characterValue == other.characterValue;
                case Type.String:
                    return stringValue == other.stringValue;
                case Type.Float:
                    return doubleValue == other.doubleValue;
                case Type.Integer:
                    return longValue == other.longValue;
                case Type.Object:
                    if (objectValue != null && other.objectValue != null)
                    {
                        return objectValue.Equals(other.objectValue);
                    }
                    else if (objectValue == null && other.objectValue == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case Type.Function:
                    return ReferenceEquals(functionValue, other.functionValue);
                case Type.Type:
                    return ReferenceEquals(typeValue, other.typeValue);
                case Type.NativeFunction:
                    return ReferenceEquals(nativeFunctionValue, other.nativeFunctionValue);
                default:
                    return true;
            }
        }

        public readonly override int GetHashCode()
        {
            return type switch
            {
                Type.Boolean => boolValue.GetHashCode(),
                Type.Character => characterValue.GetHashCode(),
                Type.String => stringValue != null ? stringValue.GetHashCode() : 0,
                Type.Float => doubleValue.GetHashCode(),
                Type.Integer => longValue.GetHashCode(),
                Type.Object => objectValue != null ? objectValue.GetHashCode() : 0,
                Type.Function => functionValue != null ? functionValue.GetHashCode() : 0,
                Type.Type => typeValue != null ? typeValue.GetHashCode() : 0,
                Type.NativeFunction => nativeFunctionValue != null ? nativeFunctionValue.GetHashCode() : 0,
                _ => 0
            };
        }

        public readonly bool TryDeserialize<T>(out T result)
        {
            try
            {
                result = ScriptingLibrary.Deserialize<T>(this);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public readonly bool TryDeserialize(out ReadOnlySpan<char> result)
        {
            if (type == Type.String)
            {
                result = stringValue.AsSpan();
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public readonly T Deserialize<T>()
        {
            return ScriptingLibrary.Deserialize<T>(this);
        }

        public readonly object Deserialize(System.Type targetType, object defaultValue = null)
        {
            try
            {
                return ScriptingLibrary.Deserialize(targetType, this);
            }
            catch
            {
                return defaultValue;
            }
        }

        public readonly ReadOnlySpan<char> Deserialize(ReadOnlySpan<char> defaultValue = default)
        {
            if (type == Type.String)
            {
                return stringValue.AsSpan();
            }
            else
            {
                return defaultValue;
            }
        }

        public readonly bool TryDeserialize(System.Type targetType, out object result)
        {
            try
            {
                result = ScriptingLibrary.Deserialize(targetType, this);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static Value Serialize(ObjectInstance objectInstance)
        {
            return new(objectInstance);
        }

        public static Value Serialize<T>(T value)
        {
            return ScriptingLibrary.Serialize(value);
        }

        public static Value Serialize(bool value)
        {
            return new(value);
        }

        public static Value Serialize(char value)
        {
            return new(value);
        }

        public static Value Serialize(byte value)
        {
            return new(value);
        }

        public static Value Serialize(sbyte value)
        {
            return new(value);
        }

        public static Value Serialize(short value)
        {
            return new(value);
        }

        public static Value Serialize(ushort value)
        {
            return new(value);
        }

        public static Value Serialize(int value)
        {
            return new(value);
        }

        public static Value Serialize(uint value)
        {
            return new(value);
        }

        public static Value Serialize(long value)
        {
            return new(value);
        }

        public static Value Serialize(ulong value)
        {
            return new((long)value);
        }

        public static Value Serialize(float value)
        {
            return new(value);
        }

        public static Value Serialize(double value)
        {
            return new(value);
        }

        public static Value Serialize(ReadOnlySpan<char> value)
        {
            return new(value.ToString());
        }

        public static Value Serialize(string value)
        {
            return new(value);
        }

        public static Value Serialize(object value)
        {
            return ScriptingLibrary.Serialize(value);
        }

        public static bool operator ==(Value left, Value right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Value left, Value right)
        {
            return !left.Equals(right);
        }

        public enum Type
        {
            Uninitialized = 0,
            Boolean,
            Character,
            String,
            Float,
            Integer,
            Object,
            Function,
            Type,
            NativeFunction
        }
    }
}