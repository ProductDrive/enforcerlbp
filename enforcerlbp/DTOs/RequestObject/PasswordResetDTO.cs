using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.RequestObject
{
    public class PasswordResetDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ResetToken { get; set; }
    }
}
