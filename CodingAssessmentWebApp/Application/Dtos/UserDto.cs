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
    public class StudentDetail
    {

        public Guid Id { get; set; }
        public string FullName { get; set; }
        public DateTime DateCreated { get; set; }
        public required string Email { get; set; }
        public bool Status { get; set; }
        public string BatchName { get; set; }
}
    public class StudentAnalytics
    {
        public int TotalAssessments { get; set; }
        public int Attempted { get;set; }
        public double AvgScore { get; set; }
        public double PassRate { get; set; }

    }
    public class ReAssignBatch
    {
         public Guid BatchId { get; set; }
    }
    public class RegisterUserRequestModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public Guid BatchId { get; set; }
    }
    public class RegisterIstructorRequestModel
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
    public class LeaderboardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Batch { get; set; }
        public double AvgScore { get; set; }
        public double HighestScore { get; set; }
        public int CompletedAssessments { get; set; }
    }
    public class UpdateStudentStatusDto
    {
        public string Status { get; set; } // "Active" or "Inactive"
    }


}

