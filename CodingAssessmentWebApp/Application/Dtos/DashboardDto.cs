using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class StudentDashBoardDto
    {
        public int TotalAssessments { get; set; }
        public int ActiveAssessments { get; set; }
        public int TotalStudentsInvited { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
        public List<TopStudentDto> TopStudents { get; set; }
        public List<WeakAreaDto> WeakAreas { get; set; }
    }

    public class TopStudentDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public double AverageScore { get; set; }
    }

    public class WeakAreaDto
    {
        public string Topic { get; set; }
        public double AverageScore { get; set; }
    }

}
