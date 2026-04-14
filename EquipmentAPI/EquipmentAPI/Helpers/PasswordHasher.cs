using System.Security.Cryptography;
using System.Text;

namespace EquipmentAPI.Helpers
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(bytes).ToLower();
            }
        }
    }
}
