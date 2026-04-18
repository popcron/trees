using System;

namespace Scripting
{
    [Serializable]
    public struct SourceCode
    {
        public string content;

        public SourceCode(string content)
        {
            this.content = content;
        }

        public readonly override string ToString()
        {
            return content;
        }

        public readonly Value Evaluate(Interpreter interpreter)
        {
            return interpreter.Evaluate(content);
        }

        public readonly T Evaluate<T>(Interpreter interpreter, T defaultValue = default)
        {
            Value value = interpreter.Evaluate(content);
            if (value.TryDeserialize(out T result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        public static SourceCode<T> Create<T>(T value)
        {
            return new SourceCode<T>(value);
        }
    }

    [Serializable]
    public struct SourceCode<T>
    {
        public string content;

        public SourceCode(string content)
        {
            this.content = content;
        }

        public SourceCode(T value)
        {
            switch (value)
            {
                case bool b:
                    content = b ? KeywordMap.True : KeywordMap.False; return;
                case char c:
                    content = $"'{c}'"; return;
                case string s:
                    content = $"\"{s}\""; return;
                case byte by:
                    content = by.ToString(); return;
                case sbyte sb:
                    content = sb.ToString(); return;
                case short sh:
                    content = sh.ToString(); return;
                case ushort us:
                    content = us.ToString(); return;
                case int i:
                    content = i.ToString(); return;
                case uint ui:
                    content = ui.ToString(); return;
                case long l:
                    content = l.ToString(); return;
                case ulong ul:
                    content = ul.ToString(); return;
                case float f:
                    content = f.ToString(); return;
                case double d:
                    content = d.ToString(); return;
            }

            Value serialized = Value.Serialize(value);
            if (serialized.type == Value.Type.Object)
            {
                content = serialized.objectValue == null ? KeywordMap.Null : serialized.objectValue.ToString();
            }
            else if (serialized.type == Value.Type.String)
            {
                content = $"\"{serialized.stringValue}\"";
            }
            else
            {
                content = serialized.ToString();
            }
        }

        public readonly override string ToString()
        {
            return content;
        }

        public readonly bool TryEvaluate(Interpreter interpreter, out T result)
        {
            Value value = interpreter.Evaluate(content);
            return value.TryDeserialize(out result);
        }

        public readonly T Evaluate(Interpreter interpreter, T defaultValue = default)
        {
            Value value = interpreter.Evaluate(content);
            if (value.TryDeserialize(out T result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        public static implicit operator SourceCode<T>(T value)
        {
            return new SourceCode<T>(value);
        }

        public static implicit operator SourceCode(SourceCode<T> sourceCode)
        {
            return new SourceCode(sourceCode.content);
        }
    }
}