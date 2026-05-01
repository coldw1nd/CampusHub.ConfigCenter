namespace CampusHub.ConfigCenter.Configuration;

public static class TextConfigurationExtensions
{
    public static IConfigurationBuilder AddTextFile(this IConfigurationBuilder builder, string path)
    {
        return builder.Add(new TextConfigurationSource { Path = path });
    }
}