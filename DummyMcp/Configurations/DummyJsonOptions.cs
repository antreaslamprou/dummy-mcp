namespace DummyMcp.Configurations;

public sealed class DummyJsonOptions
{
    public const string SectionName = "DummyJson";
    public string BaseUrl { get; set; } = "https://dummyjson.com/";
    public int TimeoutSeconds { get; set; } = 30;
}