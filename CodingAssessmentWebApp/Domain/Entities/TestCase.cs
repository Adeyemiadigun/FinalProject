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
