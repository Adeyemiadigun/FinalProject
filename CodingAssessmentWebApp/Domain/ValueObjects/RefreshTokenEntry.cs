namespace Domain.ValueObjects
{
    public class RefreshTokenEntry
    {
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
