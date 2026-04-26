using System;
using System.Collections.Generic;

namespace Scripting
{
    public class Interpreter
    {
        private readonly ReadOnly readOnly = new();
        private readonly List<int> bindingIndices = new();
        private readonly List<Binding> bindings = new();
        private readonly int selfIndex;
        private Scope rootScope = new(null);
        private Module currentModule;

        public ReadOnly ReadOnly => readOnly;
        public IReadOnlyList<Binding> Bindings => bindings;
        public int SelfIndex => selfIndex;

        public bool TryGetBindingIndexByName(int nameIndex, out int index)
        {
            return TryGetBindingIndexByNameIndex(nameIndex, out index);
        }

        public Interpreter()
        {
            selfIndex = readOnly.Add(KeywordMap.Self);
        }

        public bool TryGetBindingIndex(ReadOnlySpan<char> name, out int index)
        {
            if (!readOnly.TryGetIndex(name, out int nameIndex))
            {
                index = -1;
                return false;
            }

            return TryGetBindingIndexByNameIndex(nameIndex, out index);
        }

        private bool TryGetBindingIndexByNameIndex(int nameIndex, out int index)
        {
            for (int i = 0; i < bindingIndices.Count; i++)
            {
                if (bindingIndices[i] == nameIndex)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public Binding GetBinding(int index)
        {
            return bindings[index];
        }

        public bool ContainsVariable(ReadOnlySpan<char> name)
        {
            if (currentModule == null)
            {
                return false;
            }

            if (!readOnly.TryGetIndex(name, out int nameIndex))
            {
                return false;
            }

            return currentModule.localSlots.ContainsKey(nameIndex);
        }

        public bool ContainsBinding(ReadOnlySpan<char> name)
        {
            return TryGetBindingIndex(name, out _);
        }

        public bool ContainsFunctionVariable(ReadOnlySpan<char> name)
        {
            if (currentModule == null)
            {
                return false;
            }

            if (!readOnly.TryGetIndex(name, out int nameIndex))
            {
                return false;
            }

            return currentModule.functionNameIndices.Contains(nameIndex);
        }

        public bool ContainsFunctionBinding(ReadOnlySpan<char> name)
        {
            if (!TryGetBindingIndex(name, out int index))
            {
                return false;
            }

            return bindings[index].read().type == Value.Type.NativeFunction;
        }

        public void AddBindings(Interpreter other)
        {
            foreach (Binding binding in other.Bindings)
            {
                int nameIndex = readOnly.Add(binding.name);
                if (!TryGetBindingIndexByNameIndex(nameIndex, out _))
                {
                    RegisterBinding(nameIndex, binding);
                }
            }
        }

        private void RegisterBinding(int nameIndex, Binding binding)
        {
            bindingIndices.Add(nameIndex);
            bindings.Add(binding);
        }

        public Keyword DeclareBinding<T>(ReadOnlySpan<char> name, Func<T> read, Action<T> write)
        {
            int nameIndex = readOnly.Add(name);
            if (TryGetBindingIndexByNameIndex(nameIndex, out _))
            {
                throw new DuplicateBinding(name.ToString());
            }

            Keyword keyword = new(nameIndex);
            Binding binding = new(readOnly.strings[nameIndex], () =>
            {
                T value = read();
                return Value.Serialize(value);
            },
            value =>
            {
                write(value.Deserialize<T>());
            });

            RegisterBinding(nameIndex, binding);
            return keyword;
        }

        public Keyword DeclareBinding<T>(ReadOnlySpan<char> name, Func<T> read)
        {
            int nameIndex = readOnly.Add(name);
            if (TryGetBindingIndexByNameIndex(nameIndex, out _))
            {
                throw new DuplicateBinding(name.ToString());
            }

            Keyword keyword = new(nameIndex);
            Binding binding = new(readOnly.strings[nameIndex], () =>
            {
                T value = read();
                return Value.Serialize(value);
            }, null);

            RegisterBinding(nameIndex, binding);
            return keyword;
        }

        public Keyword DeclareBinding(ReadOnlySpan<char> name, Func<Value> read, Action<Value> write)
        {
            int nameIndex = readOnly.Add(name);
            if (TryGetBindingIndexByNameIndex(nameIndex, out _))
            {
                throw new DuplicateBinding(name.ToString());
            }

            Keyword keyword = new(nameIndex);
            Binding binding = new(readOnly.strings[nameIndex], read, write);
            RegisterBinding(nameIndex, binding);
            return keyword;
        }

        public Keyword DeclareBinding(ReadOnlySpan<char> name, Func<Value> read)
        {
            int nameIndex = readOnly.Add(name);
            if (TryGetBindingIndexByNameIndex(nameIndex, out _))
            {
                throw new DuplicateBinding(name.ToString());
            }

            Keyword keyword = new(nameIndex);
            Binding binding = new(readOnly.strings[nameIndex], read, null);
            RegisterBinding(nameIndex, binding);
            return keyword;
        }

        public Keyword DeclareFunction(ReadOnlySpan<char> name, Func<Value[], Value> function)
        {
            Value nativeFuncValue = new(function);
            return DeclareBinding(name, () => nativeFuncValue, null);
        }

        public Keyword DeclareFunction(ReadOnlySpan<char> name, Action<Value[]> function)
        {
            return DeclareFunction(name, args => { function(args); return Value.Null; });
        }

        public void ClearBindings()
        {
            bindingIndices.Clear();
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

            Module module = Parser.Parse(sourceCode, readOnly);
            Resolver resolver = new(this);
            resolver.Resolve(module);
            currentModule = module;
            rootScope = new Scope(null, module.localCount);
            Context context = new(this, module, rootScope);
            Result result = Result.Continue(Value.Null);
            for (int i = 0; i < module.statements.Count; i++)
            {
                result = ExecuteStatement(module.statements[i], ref context);
                if (result.returned)
                {
                    break;
                }
            }

            return result.value;
        }

        public Value Call(ReadOnlySpan<char> sourceCode, ReadOnlySpan<char> functionName, Value[] arguments = null)
        {
            if (sourceCode.Length == 0)
            {
                return Value.Null;
            }

            Module module = Parser.Parse(sourceCode, readOnly);
            Resolver resolver = new(this);
            resolver.Resolve(module);
            currentModule = module;
            rootScope = new Scope(null, module.localCount);
            Context context = new(this, module, rootScope);

            for (int i = 0; i < module.statements.Count; i++)
            {
                Result result = ExecuteStatement(module.statements[i], ref context);
                if (result.returned)
                {
                    break;
                }
            }

            if (!readOnly.TryGetIndex(functionName, out int nameIndex) || !module.localSlots.TryGetValue(nameIndex, out int slotIndex))
            {
                throw new UnknownIdentifier(functionName.ToString(), context.State);
            }

            ref Value funcValue = ref rootScope.slots[slotIndex];
            if (funcValue.type != Value.Type.Function || funcValue.functionValue == null)
            {
                throw new NotCallable(funcValue.type, context.State);
            }

            return InvokeFunction(funcValue.functionValue, arguments ?? Array.Empty<Value>(), ref context);
        }

        private Result ExecuteStatement(Statement statement, ref Context context)
        {
            context.current = statement;
            return statement switch
            {
                Return returnStatement => EvaluateReturn(returnStatement, ref context),
                LocalVariable localStatement => EvaluateLocalVariable(localStatement, ref context),
                TypeDefinition structure => EvaluateTypeDefinition(structure, ref context),
                FunctionDefinition function => EvaluateFunctionDefinition(function, ref context),
                If ifStatement => EvaluateIf(ifStatement, ref context),
                Block block => EvaluateBlock(block, ref context),
                ExpressionStatement expression => Result.Continue(EvaluateExpression(expression.expression, ref context)),
                _ => throw new NotImplementedException($"Unhandled statement type '{statement.GetType().Name}'"),
            };
        }

        private Result EvaluateLocalVariable(LocalVariable localStatement, ref Context context)
        {
            context.current = localStatement;
            Value value = Value.Null;
            if (localStatement.initializer is not null)
            {
                value = EvaluateExpression(localStatement.initializer, ref context);
            }

            context.scope.slots[localStatement.slotIndex] = value;
            return Result.Continue(value);
        }

        private Result EvaluateReturn(Return returnStatement, ref Context context)
        {
            context.current = returnStatement;
            Value value = Value.Null;
            if (returnStatement.value is not null)
            {
                value = EvaluateExpression(returnStatement.value, ref context);
            }

            return Result.Returned(value);
        }

        private Result EvaluateFunctionDefinition(FunctionDefinition definition, ref Context context)
        {
            context.current = definition;
            FunctionValue function = new(definition.symbol, context.scope);
            context.scope.slots[definition.slotIndex] = new Value(function);
            return Result.Continue(Value.Null);
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
                LocalIdentifier local => EvaluateLocalIdentifier(local, ref context),
                BindingIdentifier bindingId => EvaluateBindingIdentifier(bindingId),
                Group group => EvaluateExpression(group.expression, ref context),
                Unary unary => EvaluateUnary(unary, ref context),
                Binary binary => EvaluateBinary(binary, ref context),
                Assignment assignment => EvaluateAssignment(assignment, ref context),
                Construction construction => EvaluateConstruction(construction, ref context),
                MemberAccess memberAccess => EvaluateMemberAccess(memberAccess, ref context),
                MemberAssignment memberAssign => EvaluateMemberAssignment(memberAssign, ref context),
                Call call => EvaluateCall(call, ref context),
                _ => throw new NotImplementedException($"Unhandled expression type '{expression.GetType().Name}'"),
            };
        }

        private static Value EvaluateNumber(Number number)
        {
            if (number.isDouble)
            {
                return new(number.doubleValue);
            }
            else
            {
                return new(number.longValue);
            }
        }

        private Value EvaluateAssignment(Assignment assignment, ref Context context)
        {
            context.current = assignment;
            Value value = EvaluateExpression(assignment.value, ref context);
            if (assignment.target == AssignmentTarget.Local)
            {
                Scope ancestor = context.scope.Ancestor(assignment.frameDepth);
                ancestor.slots[assignment.slotIndex] = value;
                return value;
            }
            else if (assignment.target == AssignmentTarget.Binding)
            {
                Binding binding = bindings[assignment.bindingIndex];
                if (binding.write == null)
                {
                    throw new InvalidOperationException($"Binding '{binding.name}' is read-only and cannot be assigned to");
                }

                binding.write(value);
                return value;
            }

            throw new UnknownIdentifier(assignment.name, context.State);
        }

        private Result EvaluateIf(If ifStatement, ref Context context)
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

            return Result.Continue(Value.Null);
        }

        private Result EvaluateBlock(Block block, ref Context context)
        {
            context.current = block;
            Result result = Result.Continue(Value.Null);
            foreach (Statement statement in block.statements)
            {
                result = ExecuteStatement(statement, ref context);
                if (result.returned)
                {
                    return result;
                }
            }

            return result;
        }

        private Value EvaluateCall(Call call, ref Context context)
        {
            context.current = call;
            Value callee = EvaluateExpression(call.callee, ref context);

            if (callee.type == Value.Type.NativeFunction)
            {
                Value[] nativeArguments = new Value[call.arguments.Count];
                for (int i = 0; i < call.arguments.Count; i++)
                {
                    nativeArguments[i] = EvaluateExpression(call.arguments[i], ref context);
                }

                return callee.nativeFunctionValue(nativeArguments);
            }

            if (callee.type != Value.Type.Function || callee.functionValue == null)
            {
                throw new NotCallable(callee.type, context.State);
            }

            FunctionValue function = callee.functionValue;
            FunctionSymbol symbol = function.symbol;
            if (call.arguments.Count != symbol.ParameterCount)
            {
                throw new ArgumentCountMismatch(symbol.name, symbol.ParameterCount, call.arguments.Count, context.State);
            }

            Value[] arguments = new Value[call.arguments.Count];
            for (int i = 0; i < call.arguments.Count; i++)
            {
                arguments[i] = EvaluateExpression(call.arguments[i], ref context);
            }

            return InvokeFunction(function, arguments, ref context);
        }

        private Value InvokeFunction(FunctionValue function, Value[] arguments, ref Context context)
        {
            FunctionSymbol symbol = function.symbol;
            Scope callScope = new(function.closure, symbol.frameSize);
            int parameterSlotOffset = 0;
            if (symbol.isMethod)
            {
                callScope.slots[0] = function.boundSelf;
                parameterSlotOffset = 1;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                callScope.slots[parameterSlotOffset + i] = arguments[i];
            }

            Scope outerScope = context.scope;
            context.scope = callScope;
            try
            {
                return EvaluateBlock(symbol.body, ref context).value;
            }
            finally
            {
                context.scope = outerScope;
            }
        }

        private Value EvaluateBinary(Binary binary, ref Context context)
        {
            context.current = binary;
            if (binary.op == BinaryOperator.And)
            {
                return EvaluateLogicalAnd(binary, ref context);
            }
            else if (binary.op == BinaryOperator.Or)
            {
                return EvaluateLogicalOr(binary, ref context);
            }

            Value left = EvaluateExpression(binary.left, ref context);
            Value right = EvaluateExpression(binary.right, ref context);
            if (binary.op == BinaryOperator.Equal)
            {
                return new(left.Equals(right));
            }
            else if (binary.op == BinaryOperator.NotEqual)
            {
                return new(!left.Equals(right));
            }

            if (IsComparisonOperator(binary.op))
            {
                return EvaluateComparison(binary, left, right, ref context);
            }

            if ((left.type == Value.Type.Boolean && right.type == Value.Type.Character) ||
                (left.type == Value.Type.Character && right.type == Value.Type.Boolean))
            {
                throw new OperatorNotSupportedBetweenBooleansAndCharacters(binary, context.State);
            }

            if (left.type == Value.Type.Integer && right.type == Value.Type.Integer)
            {
                return EvaluateLongArithmetic(binary, left.longValue, right.longValue, ref context);
            }

            if (IsNumeric(left.type) && IsNumeric(right.type))
            {
                return EvaluateDoubleArithmetic(binary, ToDouble(left), ToDouble(right), ref context);
            }

            if (binary.op == BinaryOperator.Add && (left.type == Value.Type.String || right.type == Value.Type.String))
            {
                return new(left.ToString() + right.ToString());
            }

            throw new NotImplementedException($"Binary operator '{binary.op}' not implemented for types '{left.type}' and '{right.type}'");
        }

        private Value EvaluateLogicalAnd(Binary binary, ref Context context)
        {
            Value left = EvaluateExpression(binary.left, ref context);
            if (!left.boolValue)
            {
                return left;
            }

            return EvaluateExpression(binary.right, ref context);
        }

        private Value EvaluateLogicalOr(Binary binary, ref Context context)
        {
            Value left = EvaluateExpression(binary.left, ref context);
            if (left.boolValue)
            {
                return left;
            }

            return EvaluateExpression(binary.right, ref context);
        }

        private Value EvaluateComparison(Binary binary, Value left, Value right, ref Context context)
        {
            if (!IsNumeric(left.type) || !IsNumeric(right.type))
            {
                throw new OperatorNotSupportedBetweenNumbers(binary, context.State);
            }

            double leftNumber = ToDouble(left);
            double rightNumber = ToDouble(right);
            return binary.op switch
            {
                BinaryOperator.Greater => new(leftNumber > rightNumber),
                BinaryOperator.Less => new(leftNumber < rightNumber),
                BinaryOperator.GreaterEqual => new(leftNumber >= rightNumber),
                BinaryOperator.LessEqual => new(leftNumber <= rightNumber),
                _ => throw new OperatorNotSupportedBetweenNumbers(binary, context.State)
            };
        }

        private Value EvaluateLongArithmetic(Binary binary, long left, long right, ref Context context)
        {
            return binary.op switch
            {
                BinaryOperator.Add => new(left + right),
                BinaryOperator.Subtract => new(left - right),
                BinaryOperator.Multiply => new(left * right),
                BinaryOperator.Divide => new(left / right),
                BinaryOperator.Modulus => new(left % right),
                _ => throw new OperatorNotSupportedBetweenNumbers(binary, context.State)
            };
        }

        private Value EvaluateDoubleArithmetic(Binary binary, double left, double right, ref Context context)
        {
            return binary.op switch
            {
                BinaryOperator.Add => new(left + right),
                BinaryOperator.Subtract => new(left - right),
                BinaryOperator.Multiply => new(left * right),
                BinaryOperator.Divide => new(left / right),
                BinaryOperator.Modulus => new(left % right),
                _ => throw new OperatorNotSupportedBetweenNumbers(binary, context.State)
            };
        }

        private static bool IsComparisonOperator(BinaryOperator op)
        {
            return op == BinaryOperator.Greater || op == BinaryOperator.Less || op == BinaryOperator.GreaterEqual || op == BinaryOperator.LessEqual;
        }

        private static bool IsNumeric(Value.Type type)
        {
            return type == Value.Type.Integer || type == Value.Type.Float;
        }

        private static double ToDouble(Value value)
        {
            return value.type == Value.Type.Float ? value.doubleValue : value.longValue;
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
                if (operand.type == Value.Type.Float)
                {
                    return new(-operand.doubleValue);
                }
                else if (operand.type == Value.Type.Integer)
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

        private Value EvaluateLocalIdentifier(LocalIdentifier identifier, ref Context context)
        {
            context.current = identifier;
            Scope ancestor = context.scope.Ancestor(identifier.frameDepth);
            return ancestor.slots[identifier.slotIndex];
        }

        private Value EvaluateBindingIdentifier(BindingIdentifier identifier)
        {
            return bindings[identifier.bindingIndex].read();
        }

        private Result EvaluateTypeDefinition(TypeDefinition definition, ref Context context)
        {
            context.current = definition;
            TypeValue value = new(definition.symbol, context.scope);
            context.scope.slots[definition.slotIndex] = new Value(value);
            return Result.Continue(Value.Null);
        }

        private Value EvaluateConstruction(Construction construction, ref Context context)
        {
            context.current = construction;
            Value typeValueWrapper = EvaluateExpression(construction.type, ref context);
            if (typeValueWrapper.type != Value.Type.Type || typeValueWrapper.typeValue == null)
            {
                string typeName = construction.type is Identifier id ? id.value : construction.type.ToString();
                throw new NotATypeToConstruct(typeName, typeValueWrapper.type, context.State);
            }

            TypeValue typeValue = typeValueWrapper.typeValue;
            TypeSymbol type = typeValue.symbol;
            if (construction.arguments.Count > type.fields.Count)
            {
                throw new TooManyArgumentsForConstructor(type, construction, context.State);
            }

            ObjectInstance instance = new(type, typeValue.declaringScope);
            for (int i = 0; i < construction.arguments.Count; i++)
            {
                (string field, Expression value) = construction.arguments[i];
                int fieldIndex = type.IndexOfField(field);
                if (fieldIndex == -1)
                {
                    throw new UnknownField(type, field, context.State);
                }

                instance.fields[fieldIndex] = EvaluateExpression(value, ref context);
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

                if (structure.typeSymbol.ContainsField(memberAccess.member))
                {
                    return structure.fields[structure.typeSymbol.IndexOfField(memberAccess.member)];
                }

                if (structure.typeSymbol.TryGetMethod(memberAccess.member, out FunctionSymbol method))
                {
                    return new Value(new FunctionValue(method, structure.declaringScope, target));
                }

                throw new UnknownField(structure.typeSymbol, memberAccess.member, context.State);
            }
            else if (target.type == Value.Type.Type)
            {
                TypeValue container = target.typeValue;
                if (container == null)
                {
                    throw new AccessMemberNotSupported(memberAccess, target, context.State);
                }

                if (container.symbol.TryGetNestedType(memberAccess.member, out TypeSymbol nested))
                {
                    return new Value(new TypeValue(nested, container.declaringScope));
                }

                throw new UnknownField(container.symbol, memberAccess.member, context.State);
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

                if (instance.typeSymbol.ContainsField(memberAssign.member))
                {
                    Value value = EvaluateExpression(memberAssign.value, ref context);
                    instance.fields[instance.typeSymbol.IndexOfField(memberAssign.member)] = value;
                    if (instance.TryGetCallback(memberAssign.member, out Action<Value> callback))
                    {
                        callback(value);
                    }

                    if (memberAssign.target is BindingIdentifier bindingId)
                    {
                        Binding binding = bindings[bindingId.bindingIndex];
                        if (binding.write != null)
                        {
                            binding.write(target);
                        }
                    }

                    return value;
                }

                throw new UnknownField(instance.typeSymbol, memberAssign.member, context.State);
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
            public Scope scope;
            public Node current;

            public readonly State State => new(interpreter, module, current);

            public Context(Interpreter interpreter, Module module, Scope scope)
            {
                this.interpreter = interpreter;
                this.module = module;
                this.scope = scope;
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
