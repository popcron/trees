using System;
using System.Collections.Generic;

namespace Scripting
{
    public class Interpreter
    {
        private readonly Dictionary<ulong, TypeSymbol> types = new();
        private readonly Dictionary<ulong, Value> variables = new();
        private readonly Dictionary<ulong, Binding> bindings = new();

        public IReadOnlyCollection<Binding> Bindings => bindings.Values;

        public bool ContainsVariable(ReadOnlySpan<char> name)
        {
            ulong hash = ScriptingLibrary.GetHash(name);
            return variables.ContainsKey(hash);
        }

        public bool ContainsBinding(ReadOnlySpan<char> name)
        {
            ulong hash = ScriptingLibrary.GetHash(name);
            return bindings.ContainsKey(hash);
        }

        public void AddBindings(Interpreter other)
        {
            foreach (Binding binding in other.Bindings)
            {
                ulong hash = ScriptingLibrary.GetHash(binding.name);
                if (!bindings.ContainsKey(hash))
                {
                    bindings.Add(hash, binding);
                }
            }
        }

        public Keyword DeclareBinding<T>(ReadOnlySpan<char> name, Func<T> read, Action<T> write)
        {
            ulong hash = ScriptingLibrary.GetHash(name);
            if (bindings.ContainsKey(hash))
            {
                throw new DuplicateBinding(name.ToString());
            }

            Keyword keyword = new(hash);
            Binding binding = new(name.ToString(), () =>
            {
                T value = read();
                return Value.Serialize(value);
            },
            value =>
            {
                write(value.Deserialize<T>());
            });

            bindings.Add(hash, binding);
            return keyword;
        }

        public Keyword DeclareBinding<T>(ReadOnlySpan<char> name, Func<T> read)
        {
            ulong hash = ScriptingLibrary.GetHash(name);
            if (bindings.ContainsKey(hash))
            {
                throw new DuplicateBinding(name.ToString());
            }

            Keyword keyword = new(hash);
            Binding binding = new(name.ToString(), () =>
            {
                T value = read();
                return Value.Serialize(value);
            }, null);

            bindings.Add(hash, binding);
            return keyword;
        }

        public Keyword DeclareBinding(ReadOnlySpan<char> name, Func<Value> read, Action<Value> write)
        {
            ulong hash = ScriptingLibrary.GetHash(name);
            if (bindings.ContainsKey(hash))
            {
                throw new DuplicateBinding(name.ToString());
            }

            Keyword keyword = new(hash);
            Binding binding = new(name.ToString(), read, write);
            bindings.Add(hash, binding);
            return keyword;
        }

        public void ClearBindings()
        {
            bindings.Clear();
        }

        public Value Evaluate(string sourceCode)
        {
            return Evaluate(sourceCode.AsSpan());
        }

        public Value Evaluate(ReadOnlySpan<char> sourceCode)
        {
            if (sourceCode.Length == 0)
            {
                return Value.Null;
            }

            types.Clear();
            variables.Clear();
            Module module = Parser.Parse(sourceCode);
            Value result = Value.Null;
            Context context = new(this, module);
            for (int i = 0; i < module.statements.Count; i++)
            {
                result = ExecuteStatement(module.statements[i], ref context);
                if (context.returned)
                {
                    return result;
                }
            }

            return result;
        }

        private Value ExecuteStatement(Statement statement, ref Context context)
        {
            context.current = statement;
            return statement switch
            {
                Return returnStatement => EvaluateReturn(returnStatement, ref context),
                LocalVariable localStatement => EvaluateLocalVariable(localStatement, ref context),
                TypeDefinition structure => EvaluateTypeDefinition(structure, ref context),
                If ifStatement => EvaluateIf(ifStatement, ref context),
                Block block => EvaluateBlock(block, ref context),
                ExpressionStatement expression => EvaluateExpression(expression.expression, ref context),
                _ => throw new NotImplementedException($"Unhandled statement type '{statement.GetType().Name}'"),
            };
        }

        private Value EvaluateLocalVariable(LocalVariable localStatement, ref Context context)
        {
            context.current = localStatement;
            ulong hash = ScriptingLibrary.GetHash(localStatement.name);
            if (variables.ContainsKey(hash))
            {
                throw new DuplicateLocalVariable(localStatement.name, context.State);
            }

            Value value = Value.Null;
            if (localStatement.initializer is not null)
            {
                value = EvaluateExpression(localStatement.initializer, ref context);
            }

            variables.Add(hash, value);
            return value;
        }

        private Value EvaluateReturn(Return returnStatement, ref Context context)
        {
            context.current = returnStatement;
            Value value = Value.Null;
            if (returnStatement.value is not null)
            {
                value = EvaluateExpression(returnStatement.value, ref context);
            }

            context.returned = true;
            return value;
        }

        private Value EvaluateExpression(Expression expression, ref Context context)
        {
            context.current = expression;
            return expression switch
            {
                NullLiteral => Value.Null,
                Number number => EvaluateNumber(number),
                Text text => new(text.value),
                Character character => new(character.value),
                Boolean boolean => new(boolean.value),
                Identifier identifier => EvaluateIdentifier(identifier, ref context),
                Group group => EvaluateExpression(group.expression, ref context),
                Unary unary => EvaluateUnary(unary, ref context),
                Binary binary => EvaluateBinary(binary, ref context),
                Assignment assignment => EvaluateAssignment(assignment, ref context),
                Construction construction => EvaluateConstruction(construction, ref context),
                MemberAccess memberAccess => EvaluateMemberAccess(memberAccess, ref context),
                MemberAssignment memberAssign => EvaluateMemberAssignment(memberAssign, ref context),
                _ => throw new NotImplementedException($"Unhandled expression type '{expression.GetType().Name}'"),
            };
        }

        private Value.Type EstimateType(Expression expression, ref Context context)
        {
            context.current = expression;
            return expression switch
            {
                NullLiteral => Value.Type.Object,
                Number number => long.TryParse(number.value, out _) ? Value.Type.Long : Value.Type.Double,
                Text => Value.Type.String,
                Character => Value.Type.Character,
                Boolean => Value.Type.Boolean,
                Identifier identifier => EstimateIdentifierType(identifier, ref context),
                Group group => EstimateType(group.expression, ref context),
                Unary unary => EstimateUnaryType(unary, ref context),
                Binary binary => EstimateBinaryType(binary, ref context),
                Assignment assignment => EstimateType(assignment.value, ref context),
                Construction construction => Value.Type.Object,
                MemberAccess memberAccess => EstimateMemberAccessType(memberAccess, ref context),
                MemberAssignment memberAssign => EstimateType(memberAssign.value, ref context),
                _ => throw new NotImplementedException($"Unhandled expression type '{expression.GetType().Name}'"),
            };
        }

        private Value.Type EstimateIdentifierType(Identifier identifier, ref Context context)
        {
            context.current = identifier;
            ulong hash = ScriptingLibrary.GetHash(identifier.value);
            if (variables.TryGetValue(hash, out Value value))
            {
                return value.type;
            }
            if (bindings.TryGetValue(hash, out Binding binding))
            {
                return binding.read().type;
            }

            throw new UnknownIdentifier(identifier.value, context.State);
        }

        private Value.Type EstimateUnaryType(Unary unary, ref Context context)
        {
            context.current = unary;
            if (unary.op == UnaryOperator.Not)
            {
                return Value.Type.Boolean;
            }
            else if (unary.op == UnaryOperator.Negate)
            {
                Value.Type operandType = EstimateType(unary.operand, ref context);
                if (operandType == Value.Type.Long || operandType == Value.Type.Double)
                {
                    return operandType;
                }
            }

            throw new NotImplementedException($"Cannot estimate type of unary operator '{unary.op}'");
        }

        private Value.Type EstimateBinaryType(Binary binary, ref Context context)
        {
            context.current = binary;
            if (binary.op == BinaryOperator.And || binary.op == BinaryOperator.Or)
            {
                return Value.Type.Boolean;
            }
            else if (binary.op == BinaryOperator.Equal || binary.op == BinaryOperator.NotEqual)
            {
                return Value.Type.Boolean;
            }
            else if (binary.op == BinaryOperator.Add || binary.op == BinaryOperator.Subtract ||
                     binary.op == BinaryOperator.Multiply || binary.op == BinaryOperator.Divide ||
                     binary.op == BinaryOperator.Modulus)
            {
                Value.Type leftType = EstimateType(binary.left, ref context);
                Value.Type rightType = EstimateType(binary.right, ref context);
                if (leftType == Value.Type.String || rightType == Value.Type.String)
                {
                    return Value.Type.String;
                }
                else if (leftType == Value.Type.Double || rightType == Value.Type.Double)
                {
                    return Value.Type.Double;
                }
                else if (leftType == Value.Type.Long && rightType == Value.Type.Long)
                {
                    return Value.Type.Long;
                }
            }

            throw new NotImplementedException($"Cannot estimate type of binary operator '{binary.op}'");
        }

        private Value.Type EstimateMemberAccessType(MemberAccess memberAccess, ref Context context)
        {
            context.current = memberAccess;
            Value target = EvaluateExpression(memberAccess.target, ref context);
            if (target.type == Value.Type.Object)
            {
                ObjectInstance structure = target.objectValue;
                if (structure == null)
                {
                    throw new ReadingFieldOfNullInstance(memberAccess, target, context.State);
                }
                else if (!structure.type.ContainsField(memberAccess.member))
                {
                    throw new UnknownField(structure.type, memberAccess.member, context.State);
                }
                else
                {
                    return structure.type.GetField(memberAccess.member).type;
                }
            }
            else
            {
                throw new AccessMemberNotSupported(memberAccess, target, context.State);
            }
        }

        private static Value EvaluateNumber(Number number)
        {
            if (long.TryParse(number.value, out long longValue))
            {
                return new(longValue);
            }
            else
            {
                return new(double.Parse(number.value));
            }
        }

        private Value EvaluateAssignment(Assignment assignment, ref Context context)
        {
            context.current = assignment;
            Value value = EvaluateExpression(assignment.value, ref context);
            ulong hash = ScriptingLibrary.GetHash(assignment.name);
            if (variables.ContainsKey(hash))
            {
                variables[hash] = value;
                return value;
            }

            if (bindings.TryGetValue(hash, out Binding binding))
            {
                if (binding.write == null)
                {
                    //throw new CannotWriteToReadOnlyBinding(assignment.name, context.State);
                    throw new InvalidOperationException($"Binding '{binding.name}' is read-only and cannot be assigned to");
                }

                binding.write(value);
                return value;
            }

            throw new UnknownIdentifier(assignment.name, context.State);
        }

        private Value EvaluateIf(If ifStatement, ref Context context)
        {
            context.current = ifStatement;
            Value conditionValue = EvaluateExpression(ifStatement.condition, ref context);
            if (conditionValue.boolValue)
            {
                return ExecuteStatement(ifStatement.body, ref context);
            }
            else if (ifStatement.elseBody != null)
            {
                return ExecuteStatement(ifStatement.elseBody, ref context);
            }

            return Value.Null;
        }

        private Value EvaluateBlock(Block block, ref Context context)
        {
            context.current = block;
            Value result = Value.Null;
            foreach (Statement statement in block.statements)
            {
                result = ExecuteStatement(statement, ref context);
                if (context.returned)
                {
                    return result;
                }
            }

            return result;
        }

        private Value EvaluateBinary(Binary binary, ref Context context)
        {
            context.current = binary;
            Value left;
            Value right;
            if (binary.op == BinaryOperator.And)
            {
                left = EvaluateExpression(binary.left, ref context);
                if (!left.boolValue)
                {
                    return left;
                }

                return EvaluateExpression(binary.right, ref context);
            }
            else if (binary.op == BinaryOperator.Or)
            {
                left = EvaluateExpression(binary.left, ref context);
                if (left.boolValue)
                {
                    return left;
                }

                return EvaluateExpression(binary.right, ref context);
            }

            // equality
            left = EvaluateExpression(binary.left, ref context);
            right = EvaluateExpression(binary.right, ref context);
            if (binary.op == BinaryOperator.Equal)
            {
                return new(left.Equals(right));
            }
            else if (binary.op == BinaryOperator.NotEqual)
            {
                return new(!left.Equals(right));
            }

            // disallow between character and boolean
            if ((left.type == Value.Type.Boolean && right.type == Value.Type.Character) ||
                (left.type == Value.Type.Character && right.type == Value.Type.Boolean))
            {
                throw new OperatorNotSupportedBetweenBooleansAndCharacters(binary, context.State);
            }

            // arithmetic with numerics
            if (left.type == Value.Type.Long && right.type == Value.Type.Long)
            {
                long leftNumber = left.longValue;
                long rightNumber = right.longValue;
                Value result = binary.op switch
                {
                    BinaryOperator.Add => new(leftNumber + rightNumber),
                    BinaryOperator.Subtract => new(leftNumber - rightNumber),
                    BinaryOperator.Multiply => new(leftNumber * rightNumber),
                    BinaryOperator.Divide => new(leftNumber / rightNumber),
                    BinaryOperator.Modulus => new(leftNumber % rightNumber),
                    _ => throw new OperatorNotSupportedBetweenNumbers(binary, context.State)
                };

                return result;
            }

            if ((left.type == Value.Type.Double || left.type == Value.Type.Long) &&
                (right.type == Value.Type.Double || right.type == Value.Type.Long))
            {
                double leftNumber = left.type == Value.Type.Double ? left.doubleValue : left.longValue;
                double rightNumber = right.type == Value.Type.Double ? right.doubleValue : right.longValue;
                Value result = binary.op switch
                {
                    BinaryOperator.Add => new(leftNumber + rightNumber),
                    BinaryOperator.Subtract => new(leftNumber - rightNumber),
                    BinaryOperator.Multiply => new(leftNumber * rightNumber),
                    BinaryOperator.Divide => new(leftNumber / rightNumber),
                    BinaryOperator.Modulus => new(leftNumber % rightNumber),
                    _ => throw new OperatorNotSupportedBetweenNumbers(binary, context.State)
                };

                return result;
            }

            if (binary.op == BinaryOperator.Add && (left.type == Value.Type.String || right.type == Value.Type.String))
            {
                return new(left.ToString() + right.ToString());
            }

            throw new NotImplementedException($"Binary operator '{binary.op}' not implemented for types '{left.type}' and '{right.type}'");
        }

        private Value EvaluateUnary(Unary unary, ref Context context)
        {
            context.current = unary;
            string name = null;
            if (unary.operand is Identifier identifier)
            {
                name = identifier.value;
            }

            Value operand = EvaluateExpression(unary.operand, ref context);
            if (unary.op == UnaryOperator.Not)
            {
                if (operand.type == Value.Type.Boolean)
                {
                    return new Value(!operand.boolValue);
                }
                else
                {
                    throw new CannotInvertNonBoolean(operand, name, context.State);
                }
            }
            else if (unary.op == UnaryOperator.Negate)
            {
                if (operand.type == Value.Type.Double)
                {
                    return new(-operand.doubleValue);
                }
                else if (operand.type == Value.Type.Long)
                {
                    return new(-operand.longValue);
                }
                else
                {
                    throw new CannotNegateNonNumber(operand, name, context.State);
                }
            }
            else
            {
                throw new InvalidOperationException($"Unknown unary operator '{unary.op}'");
            }
        }

        private Value EvaluateIdentifier(Identifier identifier, ref Context context)
        {
            context.current = identifier;
            ulong hash = ScriptingLibrary.GetHash(identifier.value);
            if (variables.TryGetValue(hash, out Value value))
            {
                return value;
            }

            if (bindings.TryGetValue(hash, out Binding binding))
            {
                return binding.read();
            }

            throw new UnknownIdentifier(identifier.value, context.State);
        }

        private Value EvaluateTypeDefinition(TypeDefinition definition, ref Context context)
        {
            context.current = definition;
            ulong hash = ScriptingLibrary.GetHash(definition.name);
            if (types.ContainsKey(hash))
            {
                throw new DuplicateType(definition, context.State);
            }

            int fieldCount = definition.fields.Length;
            FieldSymbol[] fields = new FieldSymbol[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                FieldDefinition fieldDefinition = definition.fields[i];
                fields[i] = new(fieldDefinition.type, fieldDefinition.name);
            }

            TypeSymbol type = new(definition.name, fields);
            types.Add(hash, type);
            return default;
        }

        private Value EvaluateConstruction(Construction construction, ref Context context)
        {
            context.current = construction;
            ulong hash = ScriptingLibrary.GetHash(construction.type);
            if (!types.TryGetValue(hash, out TypeSymbol type))
            {
                throw new TryingToConstructUnknownType(construction.type, context.State);
            }

            if (construction.arguments.Count > type.fields.Count)
            {
                throw new TooManyArgumentsForConstructor(type, construction, context.State);
            }

            // extra arguments are not allowed
            for (int i = 0; i < construction.arguments.Count; i++)
            {
                (string field, Expression value) argument = construction.arguments[i];
                if (type.TryGetField(argument.field, out FieldSymbol field))
                {
                    // Value.Type expectedType = field.type;
                    // Value.Type actualType = EstimateType(argument.value, ref context);
                    // todo: check argument types match field types, which requires the ability to declare field types. later
                }
                else
                {
                    throw new UnknownField(type, argument.field, context.State);
                }
            }

            // todo: throw an exception if construction isnt possible
            ObjectInstance instance = new(type);
            foreach ((string field, Expression value) in construction.arguments)
            {
                instance.values[type.IndexOfField(field)] = EvaluateExpression(value, ref context);
            }

            return new(instance);
        }

        private Value EvaluateMemberAccess(MemberAccess memberAccess, ref Context context)
        {
            context.current = memberAccess;
            Value target = EvaluateExpression(memberAccess.target, ref context);
            if (target.type == Value.Type.Object)
            {
                ObjectInstance structure = target.objectValue;
                if (structure == null)
                {
                    throw new ReadingFieldOfNullInstance(memberAccess, target, context.State);
                }

                if (!structure.type.ContainsField(memberAccess.member))
                {
                    throw new UnknownField(structure.type, memberAccess.member, context.State);
                }

                return structure.values[structure.type.IndexOfField(memberAccess.member)];
            }
            else
            {
                throw new AccessMemberNotSupported(memberAccess, target, context.State);
            }
        }

        private Value EvaluateMemberAssignment(MemberAssignment memberAssign, ref Context context)
        {
            context.current = memberAssign;
            Value target = EvaluateExpression(memberAssign.target, ref context);
            if (target.type == Value.Type.Object)
            {
                ObjectInstance instance = target.objectValue;
                if (instance == null)
                {
                    throw new WritingFieldOfNullInstance(memberAssign, target, context.State);
                }

                if (instance.type.ContainsField(memberAssign.member))
                {
                    Value value = EvaluateExpression(memberAssign.value, ref context);
                    instance.values[instance.type.IndexOfField(memberAssign.member)] = value;
                    if (instance.callbacks.TryGetValue(ScriptingLibrary.GetHash(memberAssign.member), out Action<Value> callback))
                    {
                        callback(value);
                    }

                    return value;
                }

                throw new UnknownField(instance.type, memberAssign.member, context.State);
            }
            else
            {
                throw new AccessMemberNotSupported(memberAssign, target, context.State);
            }
        }

        public readonly ref struct State
        {
            public readonly Interpreter interpreter;
            public readonly Module module;
            public readonly Node node;

            public State(Interpreter interpreter, Module module, Node node)
            {
                this.interpreter = interpreter;
                this.module = module;
                this.node = node;
            }
        }

        private ref struct Context
        {
            public readonly Interpreter interpreter;
            public readonly Module module;
            public bool returned;
            public Node current;

            public readonly State State => new(interpreter, module, current);

            public Context(Interpreter interpreter, Module module)
            {
                this.interpreter = interpreter;
                this.module = module;
                this.returned = false;
                current = null;
            }
        }

        public readonly struct Binding
        {
            public readonly string name;
            public readonly Func<Value> read;
            public readonly Action<Value> write;

            public Binding(string name, Func<Value> read, Action<Value> write)
            {
                this.name = name;
                this.read = read;
                this.write = write;
            }
        }
    }
}