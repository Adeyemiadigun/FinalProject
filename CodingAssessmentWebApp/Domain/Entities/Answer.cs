using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitties
{
    public class Answer
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public string AnswerText { get; set; }
    }
}
