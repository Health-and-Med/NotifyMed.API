using NotifyMed.Domain.Entities;
using NotifyMed.Domain.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyMed.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _dbConnection;

        public UserRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<User> GetUserByUsernameAsync(string email)
        {
            string sql = "SELECT * FROM Users WHERE Email = @email";
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task AddUserAsync(User user)
        {
            string sql = "INSERT INTO Users (Username, Cpf, Crm, Email, PasswordHash, Role) VALUES (@Username, @Cpf, @Crm, @Email, @PasswordHash, @Role)";
            await _dbConnection.ExecuteAsync(sql, user);
        }
    }
}

