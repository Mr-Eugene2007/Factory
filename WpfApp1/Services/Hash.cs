using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    internal class Hash
    {
        /// <summary>
        /// Хэширует заданный пароль с использованием алгоритма SHA256.
        /// </summary>
        /// <param name="password">Пароль, который необходимо хэшировать.</param>
        /// <returns>Строковое представление хэша пароля в шестнадцатеричном формате.</returns>
        public static string ComputeSha256Hash(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = ComputeSha256Hash(password);
            return hashedInput.Equals(hashedPassword);
        }

    }
}
