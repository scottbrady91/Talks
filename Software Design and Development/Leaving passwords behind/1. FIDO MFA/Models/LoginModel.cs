using System.ComponentModel.DataAnnotations;

namespace MyWebsite.Models;

public class LoginModel
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
    
    public string ReturnUrl { get; set; }
    
}