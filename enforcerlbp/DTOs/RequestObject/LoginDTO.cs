using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
    }
}
