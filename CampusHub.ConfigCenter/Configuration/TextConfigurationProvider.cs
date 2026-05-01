namespace CampusHub.ConfigCenter.Configuration;

public class TextConfigurationProvider : ConfigurationProvider
{
    private readonly string _filePath;

    public TextConfigurationProvider(string filePath)
    {
        _filePath = filePath;
    }

    public override void Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        
        if (File.Exists(_filePath))
        {
            var lines = File.ReadAllLines(_filePath);
            for (int i = 0; i < lines.Length; i += 2)
            {
                if (i + 1 < lines.Length)
                {
                    data[lines[i].Trim()] = lines[i + 1].Trim();
                }
            }
        }

        Data = data;
    }
}