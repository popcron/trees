using Scripting;

public static class InterpreterBindings
{
    static InterpreterBindings()
    {
        ScriptingLibrary.RegisterTypeHandler(new ColorTypeHandler());
        ScriptingLibrary.RegisterTypeHandler(new EntityIdTypeHandler());
        ScriptingLibrary.RegisterTypeHandler(new UnitTypeHandler());
        ScriptingLibrary.RegisterTypeHandler(new BaseBehaviourTypeHandler());
    }
}