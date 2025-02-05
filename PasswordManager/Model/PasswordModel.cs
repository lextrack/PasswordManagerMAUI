namespace PasswordManager.Model
{
    public class PasswordModel
    {
        public string Service { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string CensoredPassword
        {
            get
            {
                return string.IsNullOrEmpty(Password)
                    ? string.Empty
                    : "*****";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is PasswordModel other)
            {
                return Service == other.Service &&
                       Username == other.Username &&
                       Password == other.Password;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Service, Username, Password);
        }
    }
}