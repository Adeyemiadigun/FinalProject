using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class StudentDashboardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double AverageScore { get; set; }
        public int CompletedAssessmentsCount { get; set; }
        public int TotalAssessmentsCount { get; set; }
        public double CompletionRate { get; set; }
        public List<AssessmentDto> RecentAssessments { get; set; } // Optional
        public List<ScoreTrendDto> ScoreTrends { get; set; } // Optional
    }
    public class ScoreTrendDto
    {
        public DateTime AssessmentDate { get; set; }
        public string AssessmentTitle { get; set; }
        public double Score { get; set; }
    }

}
