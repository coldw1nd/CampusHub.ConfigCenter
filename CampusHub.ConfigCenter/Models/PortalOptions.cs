namespace CampusHub.ConfigCenter.Models;

public class PortalOptions
{
    public string Title { get; set; } = "";
    public string Semester { get; set; } = "";
    public string SupportEmail { get; set; } = "";
    public string CampusName { get; set; } = "";
    public AdminOptions Admin { get; set; } = new();
    public List<string> Modules { get; set; } = new();
}