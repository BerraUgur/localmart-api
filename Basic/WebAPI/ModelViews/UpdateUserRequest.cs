namespace WebAPI.ModelViews;

public class UpdateUserRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public int Role { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool Status { get; set; }
}