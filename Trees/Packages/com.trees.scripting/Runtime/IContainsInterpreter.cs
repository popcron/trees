#if UNITY_5_3_OR_NEWER
namespace Scripting
{
    public interface IContainsInterpreter
    {
        Interpreter Interpreter { get; }
    }
}
#endif