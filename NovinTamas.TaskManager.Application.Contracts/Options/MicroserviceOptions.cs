namespace NovinTamas.TaskManager.Application.Contracts.Options
{
    public class MicroserviceOptions
    {
        public string VoipService { get; set; } = string.Empty;
        public string IAMService { get; set; } = string.Empty;
        public string UserProfileService { get; set; } = string.Empty;
    }
}
