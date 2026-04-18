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
        internal bool boolValue;

        [FieldOffset(4)]
        internal char characterValue;

        [FieldOffset(16)]
        internal string stringValue;

        [FieldOffset(4)]
        internal double doubleValue;

        [FieldOffset(4)]
        internal long longValue;

        [FieldOffset(16)]
        internal ObjectInstance objectValue;

        public readonly bool IsNull => type == Type.Object && objectValue == null;
        public readonly ObjectInstance Object => objectValue;

        internal Value(Type type)
        {
            this.type = type;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            objectValue = default;
        }

        internal Value(bool value)
        {
            this.type = Type.Boolean;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            objectValue = default;
            boolValue = value;
        }

        internal Value(char value)
        {
            this.type = Type.Character;
            boolValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            objectValue = default;
            characterValue = value;
        }

        internal Value(string value)
        {
            this.type = Type.String;
            boolValue = default;
            characterValue = default;
            doubleValue = default;
            longValue = default;
            objectValue = default;
            stringValue = value;
        }

        internal Value(double value)
        {
            this.type = Type.Double;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            longValue = default;
            objectValue = default;
            doubleValue = value;
        }

        internal Value(long value)
        {
            this.type = Type.Long;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            objectValue = default;
            longValue = value;
        }

        internal Value(ObjectInstance value)
        {
            this.type = Type.Object;
            boolValue = default;
            characterValue = default;
            stringValue = default;
            doubleValue = default;
            longValue = default;
            objectValue = value;
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

        public readonly void Append(StringBuilder stringBuilder)
        {
            if (type == default)
            {
                stringBuilder.Append("default");
            }
            else if (type == Type.Boolean)
            {
                stringBuilder.Append(boolValue ? KeywordMap.True : KeywordMap.False);
            }
            else if (type == Type.Character)
            {
                stringBuilder.Append(characterValue);
            }
            else if (type == Type.String)
            {
                stringBuilder.Append(stringValue);
            }
            else if (type == Type.Double)
            {
                stringBuilder.Append(doubleValue);
            }
            else if (type == Type.Long)
            {
                stringBuilder.Append(longValue);
            }
            else if (type == Type.Object)
            {
                if (objectValue != null)
                {
                    stringBuilder.Append(objectValue.ToString());
                }
            }
            else
            {
                stringBuilder.Append(base.ToString());
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

            if (type == Type.Boolean)
            {
                return boolValue == other.boolValue;
            }
            else if (type == Type.Character)
            {
                return characterValue == other.characterValue;
            }
            else if (type == Type.String)
            {
                return stringValue == other.stringValue;
            }
            else if (type == Type.Double)
            {
                return doubleValue == other.doubleValue;
            }
            else if (type == Type.Long)
            {
                return longValue == other.longValue;
            }
            else if (type == Type.Object)
            {
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
            }
            else
            {
                return true;
            }
        }

        public readonly override int GetHashCode()
        {
            if (type == Type.Boolean)
            {
                return boolValue.GetHashCode();
            }
            else if (type == Type.Character)
            {
                return characterValue.GetHashCode();
            }
            else if (type == Type.String)
            {
                return stringValue != null ? stringValue.GetHashCode() : 0;
            }
            else if (type == Type.Double)
            {
                return doubleValue.GetHashCode();
            }
            else if (type == Type.Long)
            {
                return longValue.GetHashCode();
            }
            else if (type == Type.Object)
            {
                return objectValue.GetHashCode();
            }
            else
            {
                return 0;
            }
        }

        public readonly bool TryDeserialize<T>(out T result)
        {
            if (ScriptingLibrary.TryDeserialize(this, out result))
            {
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public readonly T Deserialize<T>(T defaultValue = default)
        {
            if (ScriptingLibrary.TryDeserialize(this, out T result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        public readonly object Deserialize(System.Type targetType)
        {
            if (ScriptingLibrary.TryDeserialize(targetType, this, out object result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public readonly object Deserialize(System.Type targetType, object defaultValue = null)
        {
            if (ScriptingLibrary.TryDeserialize(targetType, this, out object result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        public readonly bool TryDeserialize(System.Type targetType, out object result)
        {
            if (ScriptingLibrary.TryDeserialize(targetType, this, out result))
            {
                return true;
            }
            else
            {
                result = null;
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
            Boolean = 1,
            Character = 2,
            String = 3,
            Double = 4,
            Long = 5,
            Object = 6
        }
    }
}