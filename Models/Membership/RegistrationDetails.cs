using System.ComponentModel.DataAnnotations;

namespace Formula.SimpleMembership
{
    public class RegistrationDetails
    {
        public string Username { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public bool StartFreeTrial {get; set;}
        
        public bool IsAdmin {get; set;}
    }
}
