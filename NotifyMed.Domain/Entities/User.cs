using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyMed.Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string Cpf { get; set; }
        public string Crm { get; set; }
        [Required]
        public string Email { get; set; }

        public string Role { get; set; }
    }
}

