using System;

namespace Scripting
{
    public abstract class ObjectTypeHandler
    {
        public readonly TypeSymbol typeSymbol;

        public abstract Type Type { get; }

        public ObjectTypeHandler()
        {
            typeSymbol = new(Type.Name);
            CreateTypeSymbol(typeSymbol);
        }

        protected abstract void CreateTypeSymbol(TypeSymbol typeSymbol);
    }

    public abstract class ObjectTypeHandler<T> : ObjectTypeHandler, ITypeHandler<T>
    {
        public override Type Type => typeof(T);

        public Value Serialize(T obj)
        {
            ObjectInstance newInstance = new(typeSymbol, null);
            Serialize(newInstance, obj);
            return Value.Serialize(newInstance);
        }

        public T Deserialize(Value value)
        {
            return Deserialize(value.objectValue);
        }

        protected abstract void Serialize(ObjectInstance instance, T value);
        protected abstract T Deserialize(ObjectInstance instance);
    }
}