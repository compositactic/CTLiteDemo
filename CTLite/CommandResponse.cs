namespace CTLite
{
    public class CommandResponse
    {
        public object ReturnValue { get; set; }
        public CompositeRootHttpContext Context { get; internal set; }
    }
}