using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class SaveProgressDto
    {
        public Guid AssessmentId { get; set; }

        public List<InProgressAnswerDto> Answers { get; set; } = new();

        // Optional: send from frontend to help with time-tracking
        public DateTime? CurrentSessionStart { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
    }
    public class InProgressAnswerDto
    {
        public Guid QuestionId { get; set; }

        // For subjective/coding answers
        public string? AnswerText { get; set; }

        // For MCQ or multi-select
        public List<Guid> SelectedOptionIds { get; set; } = new();
    }
    public class LoadProgressDto
    {
        public Guid StudentId { get; set; }
        public Guid AssessmentId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? LastSavedAt { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public List<InProgressAnswerDto> Answers { get; set; } = new();
    }

}
