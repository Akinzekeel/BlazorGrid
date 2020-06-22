using System.ComponentModel.DataAnnotations;

namespace BlazorGrid.Demo.Models
{
    public class Employee
    {
        [Display(Name = "#")]
        public int Id { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Email address")]
        public string Email { get; set; }

        public string Avatar { get; set; }
    }
}