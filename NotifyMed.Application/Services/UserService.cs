using NotifyMed.Domain.Entities;
using NotifyMed.Domain.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NotifyMed.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly byte[] _key;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            // Usar uma chave fixa para HMACSHA512 (isso é para simplificação, para produção você deve usar um salt único para cada senha)
            _key = Encoding.UTF8.GetBytes("a-secure-key-of-your-choice");
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {

            if(string.IsNullOrEmpty(email))
                throw new ArgumentNullException("email");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            var user = await _userRepository.GetUserByUsernameAsync(email);
            if (user == null || !VerifyPasswordHash(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User> GetUser(string email)
        {
            var user = await _userRepository.GetUserByUsernameAsync(email);
            return user;
        }



        public async Task RegisterAsync(string username, string cpf, string crm, string email, string password, string role)
        {

            if (string.IsNullOrEmpty(username))
                throw new ValidationException("username");

            if (string.IsNullOrEmpty(role))
                throw new ValidationException("role");

            if (string.IsNullOrEmpty(email))
                throw new ValidationException("email");

            if (string.IsNullOrEmpty(password))
                throw new ValidationException("password");

            User userExists = await GetUser(email);
            if (userExists != null)
                throw new ValidationException("Usuário já existe na base.");

            if (role == "doctor")
            {
                if (string.IsNullOrEmpty(crm))
                    throw new Exception("Informe o CRM.");
            }
            else 
            {
                if (!string.IsNullOrEmpty(crm))
                    throw new ValidationException("Esse perfil não possui CRM.");
            }

            var user = new User
            {
                Username = username,
                PasswordHash = CreatePasswordHash(password),
                Cpf = cpf,
                Crm = crm,
                Email = email,
                Role = role
            };

            await _userRepository.AddUserAsync(user);
        }

        private string CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512(_key))
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var hash = hmac.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            using (var hmac = new HMACSHA512(_key))
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var hash = hmac.ComputeHash(passwordBytes);
                var hashString = Convert.ToBase64String(hash);
                return hashString == storedHash;
            }
        }
    }
}

