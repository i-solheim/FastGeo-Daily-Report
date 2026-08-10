namespace DashboardApi.Models;

public class ProjectMembership
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = "";
}