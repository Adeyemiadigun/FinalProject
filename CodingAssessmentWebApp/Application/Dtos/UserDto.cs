using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public DateTime DateCreated { get; set; }
        public required string Email { get; set; }
        public Role Role { get; set; }
    }

    public class RegisterUserRequestModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    } 
    public class BulkRegisterUserRequestModel
    {
        public List<RegisterUserRequestModel> Users { get; set; } = [];
    }
    public class UpdateUserRequsteModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}

