namespace CampusHub.ConfigCenter.Configuration;

public class TextConfigurationSource : IConfigurationSource
{
    public string Path { get; set; } = "";

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new TextConfigurationProvider(Path);
    }
}