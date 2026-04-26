using Scripting;

// initialized by Scripting.ScriptingLibrary
public static class ScriptingLoader
{
    static ScriptingLoader()
    {
        ScriptingLibrary.RegisterTypeHandler(new Vector2TypeHandler());
        ScriptingLibrary.RegisterTypeHandler(new ColorTypeHandler());
        ScriptingLibrary.RegisterTypeHandler(new EntityIdTypeHandler());
        ScriptingLibrary.RegisterTypeHandler(new UnitTypeHandler());
        ScriptingLibrary.RegisterTypeHandler(new BaseBehaviourTypeHandler());
    }
}