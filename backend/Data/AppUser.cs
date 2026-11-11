using Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Api.Data;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public Address? Address { get; set; }
}
