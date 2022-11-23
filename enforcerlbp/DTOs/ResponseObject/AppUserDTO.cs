using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.ResponseObject
{
    public class AppUserDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public UserType UserType { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
    }
        public enum UserType
        {
            Patient,
            Physiotherapist
        }
    
}
