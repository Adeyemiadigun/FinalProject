namespace Infrastructure.Configurations
{
    public class HuggingFaceSettings
    {
        public string ApiToken { get; set; }
        public string ApiUrl { get; set; }
        public HuggingFaceModelSettings Models { get; set; }
    }

    public class HuggingFaceModelSettings
    {
        public string MCQ { get; set; }
        public string Objective { get; set; }
        public string Coding { get; set; }
    }

}
