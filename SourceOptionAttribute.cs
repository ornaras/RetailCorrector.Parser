namespace RetailCorrector
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SourceOptionAttribute(string name) : Attribute
    {
        public string Name { get; set; } = name;
    }
}
