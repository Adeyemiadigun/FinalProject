using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitties
{
    public class TestCase
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public short Weight { get; set; }
    }
}
