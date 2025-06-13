using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enum;

namespace Domain.Entitties
{
    public class Question
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid AssessmentId {  get; set; }
        public Assessment Assessment { get; set; } = default!;
        public string QuestionText { get; set; } = default!;
        public QuestionType QuestionType { get; set; }
        public short Marks { get; set; }
        public int Order { get; set; }
        public ICollection<Option> Options { get; set; } = [];
        public Answer Answer { get; set; }
        public ICollection<TestCase> Tests { get; set; } = [];
        public ICollection<AnswerSubmission> AnswerSubmissions { get;  set; } = [];
    }
}
