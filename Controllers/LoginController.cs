using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWTAuthentication.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult Login([FromBody] UserModel login)
        {
            IActionResult response = Unauthorized();
            var user = AuthenticateUser(login);
            if (user != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString, user = user });
            }

            return response;
        }

        private String GenerateJSONWebToken(UserModel userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Email, userInfo.email),
                new Claim(JwtRegisteredClaimNames.Name , userInfo.userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var exp = DateTime.Now.AddMinutes(60);
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: exp,
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private static string Decryptt(string cipherText)
        {
            string EncryptionKey = "RE358P71305KMCHA8721DFA684ZXCZNCXD0QMVJD4220L";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        private UserModel getUser(UserModel login)
        {
            UserModel user = null;
            using (DataTable Data = new DataTable())
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    try
                    {
                        connection.ConnectionString = "Server=" + "10.190.29.233" + ";Database=" + "jwt-test-310721" + ";User Id=" + "yasmin" + ";Password=" + "Yasmin@123" + ";";
                        string Query = $"select Email, Password, Username from [User] where Email = '{login.email}'";
                        SqlCommand cmd = new SqlCommand(Query, connection);
                        connection.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(Data);
                        connection.Close();
                        da.Dispose();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                    }
                }

                foreach (DataRow row in Data.Rows)
                {
                    user = new UserModel { email = row.ItemArray[0].ToString(), password = row.ItemArray[1].ToString(), userName = row.ItemArray[2].ToString() };
                }

                return user;
            }
        }

        private UserModel AuthenticateUser(UserModel login)
        {
            UserModel user = getUser(login);


            if (user != null && login.password == Decryptt(user.password))
            {

                user.password = "";
                return user;
            }
            else
                return null;


        }

    }
}
