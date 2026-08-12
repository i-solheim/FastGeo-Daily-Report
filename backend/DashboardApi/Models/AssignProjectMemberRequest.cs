namespace DashboardApi.Models;

public class AssignProjectMemberRequest
{
    public int UserId { get; set; }
    public string Role { get; set; } = "";
}