namespace Scripting
{
    public readonly struct Result
    {
        public readonly Value value;
        public readonly bool returned;

        public Result(Value value, bool returned)
        {
            this.value = value;
            this.returned = returned;
        }

        public static Result Continue(Value value)
        {
            return new Result(value, false);
        }

        public static Result Returned(Value value)
        {
            return new Result(value, true);
        }
    }
}
