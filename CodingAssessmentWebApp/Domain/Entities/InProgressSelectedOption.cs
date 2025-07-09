namespace Domain.Entities
{
    public class InProgressSelectedOption
    {
        public Guid Id { get; set; }

        public Guid OptionId { get; set; } // Store the Option.Id from the original question

        public Guid InProgressAnswerId { get; set; }
        public InProgressAnswer InProgressAnswer { get; set; }
    }

}
