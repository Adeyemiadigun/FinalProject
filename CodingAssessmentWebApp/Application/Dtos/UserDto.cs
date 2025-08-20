using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entitties;
using Domain.Enum;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public DateTime DateCreated { get; set; }
        public required string Email { get; set; }
        public Role Role { get; set; }
        public bool Status { get; set; }
    }
    public class UploadFileDto
    {
        public IFormFile StudentFile { get; set; }
        public Guid BatchId { get; set; }
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
        public int Attempted { get; set; }
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
    public class UpdateUserRequestModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        // Password change
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    public class LeaderboardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid BatchId { get; set; }
        public string Batch { get; set; }
        public double AvgScore { get; set; }
        public double HighestScore { get; set; }
        public int CompletedAssessments { get; set; }
    }
    public class UpdateStudentStatusDto
    {
        public string Status { get; set; } // "Active" or "Inactive"
    }

    public class InstructorDetailsDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool Status { get; set; }
        public DateTime JoinedDate { get; set; }
        public int TotalAssessment { get; set; }
        public double AverageScore { get; set; }
    }
    public class StudentProfileMetrics
    {

        public double AverageScore { get; set; }
        public double PassRate { get; set; }
        public double SubmmittedCount { get; set; }
        public int Rank { get; set; }
    }
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
    public class StudentScoreByTypeDto
    {
        public string Type { get; set; } = default!;
        public double AverageScore { get; set; }

        public int AttemptCount { get; set; }
    }

}

