using System.Collections.Generic;

namespace Scripting
{
    public class Resolver
    {
        private readonly Interpreter interpreter;
        private readonly List<LexicalScope> lexicalScopes = new();
        private readonly List<int> frameSizes = new();

        private int CurrentFunctionIndex => lexicalScopes[lexicalScopes.Count - 1].functionIndex;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public void Resolve(Module module)
        {
            int functionIndex = PushFunctionFrame();
            PushLexicalScope(functionIndex, module.localSlots);
            CollectDeclarations(module.statements, module);
            for (int i = 0; i < module.statements.Count; i++)
            {
                ResolveStatement(module.statements[i]);
            }

            module.localCount = frameSizes[functionIndex];
            PopLexicalScope();
            PopFunctionFrame();
        }

        private int PushFunctionFrame()
        {
            int index = frameSizes.Count;
            frameSizes.Add(0);
            return index;
        }

        private void PopFunctionFrame()
        {
            frameSizes.RemoveAt(frameSizes.Count - 1);
        }

        private void PushLexicalScope(int functionIndex, Dictionary<int, int> slots)
        {
            lexicalScopes.Add(new LexicalScope(functionIndex, slots));
        }

        private void PopLexicalScope()
        {
            lexicalScopes.RemoveAt(lexicalScopes.Count - 1);
        }

        private void CollectDeclarations(List<Statement> statements, Module module)
        {
            LexicalScope current = lexicalScopes[^1];
            for (int i = 0; i < statements.Count; i++)
            {
                Statement statement = statements[i];
                if (statement is LocalVariable local)
                {
                    local.slotIndex = DeclareSlot(current, local.nameIndex, local.name, local, module);
                }
                else if (statement is FunctionDefinition function)
                {
                    function.slotIndex = DeclareSlot(current, function.nameIndex, function.name, function, module);
                    module.functionNameIndices.Add(function.nameIndex);
                }
                else if (statement is TypeDefinition type)
                {
                    type.slotIndex = DeclareSlot(current, type.nameIndex, type.name, type, module);
                }
            }
        }

        private int DeclareSlot(LexicalScope scope, int nameIndex, string name, Node node, Module module)
        {
            if (scope.slots.ContainsKey(nameIndex))
            {
                Interpreter.State state = new(interpreter, module, node);
                if (node is TypeDefinition type)
                {
                    throw new DuplicateType(type, state);
                }

                throw new DuplicateLocalVariable(name, state);
            }

            int slot = frameSizes[scope.functionIndex]++;
            scope.slots[nameIndex] = slot;
            return slot;
        }

        private bool TryFindLocal(int nameIndex, out int frameDepth, out int slotIndex)
        {
            int currentFunctionIndex = CurrentFunctionIndex;
            for (int i = lexicalScopes.Count - 1; i >= 0; i--)
            {
                LexicalScope scope = lexicalScopes[i];
                if (scope.slots.TryGetValue(nameIndex, out int slot))
                {
                    frameDepth = currentFunctionIndex - scope.functionIndex;
                    slotIndex = slot;
                    return true;
                }
            }

            frameDepth = -1;
            slotIndex = -1;
            return false;
        }

        private void ResolveStatement(Statement statement)
        {
            if (statement is LocalVariable local)
            {
                if (local.initializer != null)
                {
                    local.initializer = ResolveExpression(local.initializer);
                }
            }
            else if (statement is Return returnStatement)
            {
                if (returnStatement.value != null)
                {
                    returnStatement.value = ResolveExpression(returnStatement.value);
                }
            }
            else if (statement is If ifStatement)
            {
                ifStatement.condition = ResolveExpression(ifStatement.condition);
                ResolveStatement(ifStatement.body);
                if (ifStatement.elseBody != null)
                {
                    ResolveStatement(ifStatement.elseBody);
                }
            }
            else if (statement is Block block)
            {
                PushLexicalScope(CurrentFunctionIndex, new Dictionary<int, int>());
                CollectDeclarations(block.statements, block.module);
                for (int i = 0; i < block.statements.Count; i++)
                {
                    ResolveStatement(block.statements[i]);
                }

                PopLexicalScope();
            }
            else if (statement is ExpressionStatement expressionStatement)
            {
                expressionStatement.expression = ResolveExpression(expressionStatement.expression);
            }
            else if (statement is FunctionDefinition function)
            {
                ResolveFunction(function, false);
            }
            else if (statement is TypeDefinition type)
            {
                ResolveType(type);
            }
        }

        private void ResolveType(TypeDefinition type)
        {
            int fieldCount = type.fields.Length;
            List<FieldSymbol> fields = new(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                FieldDefinition fieldDefinition = type.fields[i];
                fields.Add(new FieldSymbol(fieldDefinition.type, fieldDefinition.name, i));
            }

            TypeSymbol symbol = new(type.name, fields);
            for (int i = 0; i < type.methods.Length; i++)
            {
                ResolveFunction(type.methods[i], true);
                symbol.methods.Add(type.methods[i].symbol);
            }

            for (int i = 0; i < type.types.Length; i++)
            {
                ResolveType(type.types[i]);
                symbol.nestedTypes.Add(type.types[i].symbol);
            }

            type.symbol = symbol;
        }

        private void ResolveFunction(FunctionDefinition function, bool isMethod)
        {
            int functionIndex = PushFunctionFrame();
            Dictionary<int, int> parametersScope = new();
            PushLexicalScope(functionIndex, parametersScope);

            if (isMethod)
            {
                int slot = frameSizes[functionIndex]++;
                parametersScope[interpreter.SelfIndex] = slot;
            }

            for (int i = 0; i < function.parameterIndices.Length; i++)
            {
                int nameIndex = function.parameterIndices[i];
                if (parametersScope.ContainsKey(nameIndex))
                {
                    Interpreter.State state = new(interpreter, function.module, function);
                    throw new DuplicateLocalVariable(function.parameters[i], state);
                }

                int slot = frameSizes[functionIndex]++;
                parametersScope[nameIndex] = slot;
            }

            ResolveStatement(function.body);
            int frameSize = frameSizes[functionIndex];
            PopLexicalScope();
            PopFunctionFrame();
            function.symbol = new FunctionSymbol(function, isMethod, frameSize);
        }

        private Expression ResolveExpression(Expression expression)
        {
            if (expression is Identifier identifier)
            {
                return ResolveIdentifier(identifier);
            }
            else if (expression is Binary binary)
            {
                binary.left = ResolveExpression(binary.left);
                binary.right = ResolveExpression(binary.right);
                return binary;
            }
            else if (expression is Unary unary)
            {
                unary.operand = ResolveExpression(unary.operand);
                return unary;
            }
            else if (expression is Group group)
            {
                group.expression = ResolveExpression(group.expression);
                return group;
            }
            else if (expression is Call call)
            {
                call.callee = ResolveExpression(call.callee);
                for (int i = 0; i < call.arguments.Count; i++)
                {
                    call.arguments[i] = ResolveExpression(call.arguments[i]);
                }

                return call;
            }
            else if (expression is Assignment assignment)
            {
                assignment.value = ResolveExpression(assignment.value);
                ResolveAssignmentTarget(assignment);
                return assignment;
            }
            else if (expression is MemberAccess memberAccess)
            {
                memberAccess.target = ResolveExpression(memberAccess.target);
                return memberAccess;
            }
            else if (expression is MemberAssignment memberAssignment)
            {
                memberAssignment.target = ResolveExpression(memberAssignment.target);
                memberAssignment.value = ResolveExpression(memberAssignment.value);
                return memberAssignment;
            }
            else if (expression is Construction construction)
            {
                construction.type = ResolveExpression(construction.type);
                for (int i = 0; i < construction.arguments.Count; i++)
                {
                    (string field, Expression value) argument = construction.arguments[i];
                    construction.arguments[i] = (argument.field, ResolveExpression(argument.value));
                }

                return construction;
            }

            return expression;
        }

        private Identifier ResolveIdentifier(Identifier identifier)
        {
            if (identifier is LocalIdentifier || identifier is BindingIdentifier)
            {
                return identifier;
            }

            int nameIndex = identifier.valueIndex;
            if (TryFindLocal(nameIndex, out int frameDepth, out int slotIndex))
            {
                return new LocalIdentifier(identifier, frameDepth, slotIndex);
            }

            if (interpreter.TryGetBindingIndexByName(nameIndex, out int bindingIndex))
            {
                return new BindingIdentifier(identifier, bindingIndex);
            }

            Interpreter.State state = new(interpreter, identifier.module, identifier);
            throw new UnknownIdentifier(identifier.value, state);
        }

        private void ResolveAssignmentTarget(Assignment assignment)
        {
            if (TryFindLocal(assignment.nameIndex, out int frameDepth, out int slotIndex))
            {
                assignment.target = AssignmentTarget.Local;
                assignment.bindingIndex = -1;
                assignment.frameDepth = frameDepth;
                assignment.slotIndex = slotIndex;
                return;
            }

            if (interpreter.TryGetBindingIndexByName(assignment.nameIndex, out int bindingIndex))
            {
                assignment.target = AssignmentTarget.Binding;
                assignment.bindingIndex = bindingIndex;
            }
        }

        private readonly struct LexicalScope
        {
            public readonly int functionIndex;
            public readonly Dictionary<int, int> slots;

            public LexicalScope(int functionIndex, Dictionary<int, int> slots)
            {
                this.functionIndex = functionIndex;
                this.slots = slots;
            }
        }
    }
}
