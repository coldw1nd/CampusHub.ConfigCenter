using CampusHub.ConfigCenter.Configuration;
using CampusHub.ConfigCenter.Middleware;
using CampusHub.ConfigCenter.Models;
using Microsoft.Extensions.Options;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddXmlFile("portal.xml", optional: false, reloadOnChange: true);
builder.Configuration.AddIniFile("notifications.ini", optional: false, reloadOnChange: true);

var inMemorySettings = new Dictionary<string, string?>
{
    {"Notifications:Sender", "InMemory-System"}
};
builder.Configuration.AddInMemoryCollection(inMemorySettings);

builder.Configuration.AddTextFile("customsettings.txt");

builder.Configuration.AddEnvironmentVariables();
if (args.Length > 0)
{
    builder.Configuration.AddCommandLine(args);
}

builder.Services.Configure<PortalOptions>(builder.Configuration.GetSection("Portal"));
builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("Notifications"));

var app = builder.Build();

app.UseMiddleware<PortalHeaderMiddleware>();

app.MapGet("/", () => Results.Text(@"Y
    <h1>CampusHub.ConfigCenter</h1>
    <ul>
        <li><a href='/config/raw'>/config/raw</a></li>
        <li><a href='/config/section/portal'>/config/section/portal</a></li>
        <li><a href='/config/tree'>/config/tree (all)</a></li>
        <li><a href='/config/tree?section=Portal'>/config/tree?section=Portal</a></li>
        <li><a href='/config/connection'>/config/connection</a></li>
        <li><a href='/config/providers'>/config/providers</a></li>
        <li><a href='/config/custom'>/config/custom</a></li>
        <li><a href='/config/bind'>/config/bind</a></li>
        <li><a href='/config/options'>/config/options</a></li>
        <li><a href='/config/effective'>/config/effective</a></li>
    </ul>", "text/html"));

app.MapGet("/config/raw", (IConfiguration config) => new
{
    PortalTitle = config["Portal:Title"],
    SupportEmail = config["Portal:SupportEmail"],
    NotificationSender = config["Notifications:Sender"]
});

app.MapGet("/config/section/portal", (IConfiguration config) => 
    config.GetSection("Portal").GetChildren().ToDictionary(x => x.Key, x => x.Value));

app.MapGet("/config/tree", (IConfiguration config, string? section) =>
{
    var targetSection = string.IsNullOrEmpty(section) ? (IConfiguration)config : config.GetSection(section);
    
    object BuildTree(IConfiguration current)
    {
        var children = current.GetChildren().ToList();
        if (!children.Any()) return current is IConfigurationSection sec ? sec.Value : null;
        
        var dict = new Dictionary<string, object>();
        foreach (var child in children)
        {
            dict[child.Key] = BuildTree(child);
        }
        return dict;
    }

    return BuildTree(targetSection);
});

app.MapGet("/config/connection", (IConfiguration config) => 
    new { DefaultConnection = config.GetConnectionString("DefaultConnection") });

app.MapGet("/config/providers", (IConfiguration config) =>
{
    var root = (IConfigurationRoot)config;
    var html = new StringBuilder();
    html.Append("<table border='1'><tr><th>Order</th><th>Provider Type</th><th>Description</th></tr>");
    
    int order = 1;
    foreach (var provider in root.Providers)
    {
        html.Append($"<tr><td>{order++}</td><td>{provider.GetType().Name}</td><td>{provider}</td></tr>");
    }
    html.Append("</table>");
    
    return Results.Text(html.ToString(), "text/html");
});

app.MapGet("/config/custom", (IConfiguration config) => new
{
    CustomMessage = config["CustomSetting:Message"],
    CustomVersion = config["CustomSetting:Version"]
});

app.MapGet("/config/bind", (IConfiguration config) =>
{
    var portalOptions = config.GetSection("Portal").Get<PortalOptions>();
    return portalOptions;
});

app.MapGet("/config/options", (IOptions<PortalOptions> portalOptions, IOptions<NotificationOptions> notificationOptions) => new
{
    Portal = portalOptions.Value,
    Notifications = notificationOptions.Value
});

app.MapGet("/config/effective", (IConfiguration config) => new
{
    Environment = config["ASPNETCORE_ENVIRONMENT"],
    Conflicts = new
    {
        PortalTitle = new
        {
            Value = config["Portal:Title"],
            Explanation = "Победил CommandLineArgs, так как он добавлен последним (наивысший приоритет)."
        },
        PortalSupportEmail = new
        {
            Value = config["Portal:SupportEmail"],
            Explanation = "Победил Environment Variables (переопределил appsettings.json)."
        },
        NotificationSender = new
        {
            Value = config["Notifications:Sender"],
            Explanation = "Победил In-Memory Collection (добавлен после notifications.ini)."
        }
    }
});

app.Run();