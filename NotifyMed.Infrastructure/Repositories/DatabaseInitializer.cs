using NotifyMed.Domain.Interfaces;
using Dapper;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace NotifyMed.Infrastructure.Repositories
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDbConnection _dbConnection;

        public DatabaseInitializer(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void Initialize()
        {
            var createUsersTableQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id SERIAL PRIMARY KEY,
                    Username VARCHAR(50) NOT NULL,
                    Cpf VARCHAR(50) NOT NULL,
                    Crm VARCHAR(50),
                    Email VARCHAR(255) NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Role VARCHAR(50)
                );
            ";

            //var createRegionsTableQuery = @"
            //    CREATE TABLE IF NOT EXISTS Regions (
            //        DDD VARCHAR(3) PRIMARY KEY,
            //        Name VARCHAR(100) NOT NULL
            //    );
            //";

            //var createContactsTableQuery = @"
            //    CREATE TABLE IF NOT EXISTS Contacts (
            //        Id SERIAL PRIMARY KEY,
            //        Name VARCHAR(100) NOT NULL,
            //        Phone VARCHAR(15) NOT NULL,
            //        Email VARCHAR(100) NOT NULL,
            //        DDD VARCHAR(3) NOT NULL,
            //        CONSTRAINT fk_Contacts_Regions FOREIGN KEY (DDD) REFERENCES Regions(DDD)
            //    );
            //";

            _dbConnection.Execute(createUsersTableQuery);
            //_dbConnection.Execute(createRegionsTableQuery);
            //_dbConnection.Execute(createContactsTableQuery);

            // Adicionar usuário admin se não existir
            AddAdminUser();
        }

        private void AddAdminUser()
        {
            var adminUsername = "admin";
            var adminPassword = "123456";
            var adminRole = "Admin";
            var cpf = "00000000000";
            var crm = "";
            var email = "devs@email.com";

            var checkAdminUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            var adminUserCount = _dbConnection.ExecuteScalar<int>(checkAdminUserQuery, new { Username = adminUsername });

            if (adminUserCount == 0)
            {
                var passwordHash = CreatePasswordHash(adminPassword);
                var insertAdminUserQuery = @"
            INSERT INTO Users (Username, Cpf, Crm, Email, PasswordHash, Role)
            VALUES (@Username,@Cpf, @Crm, @Email, @PasswordHash, @Role);
        ";

                var adminUser = new
                {
                    Username = adminUsername,
                    Cpf = cpf,
                    Crm = crm,
                    Email = email,
                    PasswordHash = passwordHash,
                    Role = adminRole
                };

                _dbConnection.Execute(insertAdminUserQuery, adminUser);
            }
        }

        private string CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes("a-secure-key-of-your-choice")))
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var hash = hmac.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

