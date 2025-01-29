using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Model
{
    public class PasswordModel
    {
        public string Service { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CensoredPassword => new string('*', Password.Length);
    }
}
