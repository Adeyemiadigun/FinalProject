namespace Application.Dtos
{
    public class BatchDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public short BatchNumber { get; set; }

    }
    public class BatchSummaryDto
    {
        public string BatchName { get; set; }
        public int StudentCount { get; set; }
        public int AssessmentCount { get; set; }
        public double PassRate { get; set; } // percentage
    }


    public class CreateBatchRequestModel
    {
        public string Name { get; set; }
        public int BatchNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // Nullable for EndDate
    }
    public class UpdateBatchRequestModel
    {
        public string Name { get; set; }
        public string BatchNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // Nullable for EndDate
    }
    public class BatchAnalyticsMetricsDto
    {
        public double PassRate { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
        public string AverageCompletionTime { get; set; } // e.g. "00:16:20"
    }

    public class BatchStudentCountDto
    {
        public Guid BatchId { get; set; }
        public string BatchName { get; set; }
        public int StudentCount { get; set; }
    }

}
