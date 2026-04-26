using System;

namespace Scripting.Tests;

public class InterpreterTests : Tests
{
    [Test]
    [TestCase(1, '+', 2)]
    [TestCase(5, '-', 3)]
    [TestCase(3.14, '*', 2)]
    [TestCase(10, '/', 2)]
    public void Addition(double a, char op, double b)
    {
        string code = $$"""
            return {{SourceCode.Create(a)}} {{op}} {{SourceCode.Create(b)}}
            """;

        Value result = Evaluate(code);
        if (op == '+')
        {
            Assert.That(result.Deserialize<double>(), Is.EqualTo(a + b));
        }
        else if (op == '-')
        {
            Assert.That(result.Deserialize<double>(), Is.EqualTo(a - b));
        }
        else if (op == '*')
        {
            Assert.That(result.Deserialize<double>(), Is.EqualTo(a * b));
        }
        else if (op == '/')
        {
            Assert.That(result.Deserialize<double>(), Is.EqualTo(a / b));
        }
    }

    [Test]
    public void DivideByZero()
    {
        string code = """
            return 10/0
            """;

        Assert.Throws<DivideByZeroException>(() => Evaluate(code));
    }

    [Test]
    public void BooleanLogic()
    {
        string code = """
            var a = null
            var b = true
            return a != null || !b
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.Boolean));
        Assert.That(result.Deserialize<bool>(), Is.False);
    }

    [Test]
    public void UninitializedFields()
    {
        string code = """
            struct Point {
                var x
                var y
            }

            var p = new Point()
            return p.x
            """;

        Value result = Evaluate(code);
        Assert.That(result, Is.Default);
    }

    [Test]
    public void ReturnFieldOfStructure()
    {
        double x = 5;
        string code = $$"""
            struct Point {
                var x
                var y
            }

            var p = new Point(x = {{SourceCode.Create(x)}})
            return p.x
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.Integer));
        Assert.That(result.Deserialize<double>(), Is.EqualTo(x));
    }

    [Test]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(9)]
    [TestCase(10)]
    [TestCase(11)]
    public void CheckIfEven(double input)
    {
        string code = $$"""
            var is = {{SourceCode.Create(input)}} % 2 == 0
            if (is) {
                return "Even"
            } else {
                return "Odd"
            }
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.String));
        if (input % 2 == 0)
        {
            Assert.That(result.Deserialize<string>(), Is.EqualTo("Even"));
        }
        else
        {
            Assert.That(result.Deserialize<string>(), Is.EqualTo("Odd"));
        }
    }

    [Test]
    public void PrintHelloWorld()
    {
        string code = """
            return "Hello, world!"
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.String));
        Assert.That(result.Deserialize<string>(), Is.EqualTo("Hello, world!"));
    }

    [Test]
    public void ConcatenateStrings()
    {
        string code = """
            var a = "Hello, "
            var b = "world!"
            return a + b
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.String));
        Assert.That(result.Deserialize<string>(), Is.EqualTo("Hello, world!"));
    }

    [Test]
    public void TooManyArgumentsGivenToConstructor()
    {
        string code = """
            struct Point {
                var x
                var y
            }

            var point = new Point(x = 5, y = 10, z = 15)
            return point.x + point.y + point.z
            """;

        Assert.Throws<TooManyArgumentsForConstructor>(() => Evaluate(code));
    }

    [Test]
    public void UnknownFieldGivenToConstructor()
    {
        string code = """
            struct Point {
                var x
                var y
            }

            var point = new Point(a = "hello")
            return point.x + point.y + point.z
            """;

        Assert.Throws<UnknownField>(() => Evaluate(code));
    }

    [Test]
    [TestCase(-1337, "hi")]
    [TestCase("boosh", 3.14)]
    public void ConcatenateValues(object left, object right)
    {
        string code = $$"""
            var left = {{SourceCode.Create(left)}};
            var right = {{SourceCode.Create(right)}};
            return left + right + ""
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.String));
        Assert.That(result.Deserialize<string>(), Is.EqualTo($"{left}{right}"));
    }

    [Test]
    public void ConcatenateTwoNumbers()
    {
        string code = """
            var actual = 123 + 456 + ""
            return actual
            """;

        var expected = 123 + 456 + "";
        Value result = Evaluate(code);
        Assert.That(result.ToString(), Is.EqualTo(expected.ToString()));
    }

    [Test]
    public void ConcatenateCharacterAndBooleanNotAllowed()
    {
        string code = """
            return 'a' + true + ""
            """;

        Assert.Throws<OperatorNotSupportedBetweenBooleansAndCharacters>(() => Evaluate(code));
    }

    [Test]
    public void DeclareFunction()
    {
        string code = """
            fn add(var a, var b) {
                return a + b
            }

            """;

        Evaluate(code);
    }

    [Test]
    public void CallFunction()
    {
        string code = """
          fn add(var a, var b) {
              return a + b
          }

          return add(2, 3)
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(5));
    }

    [Test]
    public void Recursion()
    {
        string code = """
          fn fact(var n) {
              if (n == 0) {
                  return 1
              }

              return n * fact(n - 1)
          }

          return fact(5)
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(120));
    }

    [Test]
    public void Closures()
    {
        string code = """
          fn makeAdder(var x) {
              fn add(var y) {
                  return x + y
              }

              return add
          }

          var addFive = makeAdder(5)
          return addFive(10)
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(15));
    }

    [Test]
    public void FunctionIsFirstClass()
    {
        string code = """
          fn square(var x) {
              return x * x
          }

          fn apply(var f, var v) {
              return f(v)
          }

          return apply(square, 4)
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(16));
    }

    [Test]
    public void WrongArgumentCountThrows()
    {
        string code = """
          fn add(var a, var b) {
              return a + b
          }

          return add(1)
          """;

        Assert.Throws<ArgumentCountMismatch>(() => Evaluate(code));
    }

    [Test]
    public void CallingNonFunctionThrows()
    {
        string code = """
          var x = 5
          return x(1)
          """;

        Assert.Throws<NotCallable>(() => Evaluate(code));
    }

    [Test]
    public void BlockScopedVariableDoesNotLeak()
    {
        string code = """
          var x = 1
          if (true) {
              var x = 2
          }

          return x
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(1));
    }

    [Test]
    public void EarlyReturnFromNestedBlock()
    {
        string code = """
          fn firstPositive(var a, var b) {
              if (a > 0) {
                  return a
              }

              if (b > 0) {
                  return b
              }

              return 0
          }
          return firstPositive(0, 7)
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(7));
    }

    [Test]
    [TestCase(5, 3, true)]
    [TestCase(3, 5, false)]
    [TestCase(4, 4, false)]
    public void GreaterThan(long a, long b, bool expected)
    {
        string code = $"return {a} > {b}";
        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.Boolean));
        Assert.That(result.Deserialize<bool>(), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(5, 3, false)]
    [TestCase(3, 5, true)]
    [TestCase(4, 4, false)]
    public void LessThan(long a, long b, bool expected)
    {
        string code = $"return {a} < {b}";
        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(5, 3, true)]
    [TestCase(4, 4, true)]
    [TestCase(3, 5, false)]
    public void GreaterOrEqual(long a, long b, bool expected)
    {
        string code = $"return {a} >= {b}";
        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.EqualTo(expected));
    }

    [Test]
    [TestCase(3, 5, true)]
    [TestCase(4, 4, true)]
    [TestCase(5, 3, false)]
    public void LessOrEqual(long a, long b, bool expected)
    {
        string code = $"return {a} <= {b}";
        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.EqualTo(expected));
    }

    [Test]
    public void ComparisonMixesLongAndDouble()
    {
        string code = """
          return 3 < 3.5
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.True);
    }

    [Test]
    public void ComparisonBindsTighterThanEquality()
    {
        string code = """
          return 1 < 2 == true
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.True);
    }

    [Test]
    public void ArithmeticBindsTighterThanComparison()
    {
        string code = """
          return 1 + 2 < 2 * 2
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.True);
    }

    [Test]
    public void ComparisonBetweenNonNumericsThrows()
    {
        string code = """
          return "a" < "b"
          """;

        Assert.Throws<OperatorNotSupportedBetweenNumbers>(() => Evaluate(code));
    }

    [Test]
    public void CallMethodOnInstance()
    {
        string code = """
          struct Counter {
              var value

              fn get() {
                  return self.value
              }
          }

          var c = new Counter(value = 42)
          return c.get()
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(42));
    }

    [Test]
    public void MethodMutatesSelf()
    {
        string code = """
          struct Counter {
              var value

              fn bump() {
                  self.value = self.value + 1
              }
          }

          var c = new Counter(value = 5)
          c.bump()
          c.bump()
          return c.value
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(7));
    }

    [Test]
    public void MethodCallsSiblingMethod()
    {
        string code = """
          struct Math {
              var n

              fn double() {
                  return self.n * 2
              }

              fn quadruple() {
                  return self.double() + self.double()
              }
          }

          var m = new Math(n = 3)
          return m.quadruple()
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(12));
    }

    [Test]
    public void ConstructNestedType()
    {
        string code = """
          struct Outer {
              struct Inner {
                  var x
              }
          }

          var i = new Outer.Inner(x = 99)
          return i.x
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(99));
    }

    [Test]
    public void ConstructingNonTypeThrows()
    {
        string code = """
          var x = 5
          return new x()
          """;

        Assert.Throws<NotATypeToConstruct>(() => Evaluate(code));
    }

    [Test]
    public void TypeDefinedInFunctionDiesWithScope()
    {
        string code = """
          fn make() {
              struct Local {
                  var x
              }

              var v = new Local(x = 1)
              return v.x
          }

          return make() + make()
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(2));
    }

    [Test]
    public void ClosureCapturesByReference()
    {
        string code = """
          fn makeCounter() {
              var x = 0
              fn inc() {
                  x = x + 1
                  return x
              }

              return inc
          }
          var c = makeCounter()
          c()
          c()
          return c()
          """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(3));
    }

    [Test]
    public void CallDeclaredNativeFunction()
    {
        string code = """
          var x = getX()
          var y = getY()
          return do(x, y)
          """;

        interpreter.ClearBindings();
        interpreter.DeclareFunction("getX", (args) =>
        {
            return Value.Serialize(5);
        });

        interpreter.DeclareFunction("getY", (args) =>
        {
            return Value.Serialize(3);
        });

        interpreter.DeclareFunction("do", (args) =>
        {
            Value x = args[0];
            Value y = args[1];
            return Value.Serialize(x.Deserialize<long>() + y.Deserialize<long>());
        });

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(8));
    }

    [Test]
    public void CompoundAssignmentToBoundStructFieldWritesBack()
    {
        TypeSymbol pointType = new("Point", new[]
        {
            new FieldSymbol(Value.Type.Integer, "x"),
            new FieldSymbol(Value.Type.Integer, "y"),
        });

        ObjectInstance host = new(pointType, null);
        host.Set("x", 10L);
        host.Set("y", 20L);

        int writeCount = 0;
        interpreter.ClearBindings();
        interpreter.DeclareBinding("look",
            () => new Value(host),
            value =>
            {
                writeCount++;
                ObjectInstance updated = value.objectValue;
                host.Set("x", updated.Get("x"));
                host.Set("y", updated.Get("y"));
            });

        Evaluate("look.x = look.x + 1");
        Evaluate("look.x += 1");

        Assert.That(writeCount, Is.EqualTo(2));
        Assert.That(host.Get<long>("x"), Is.EqualTo(12));
        Assert.That(host.Get<long>("y"), Is.EqualTo(20));
    }
}