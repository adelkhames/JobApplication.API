namespace JobApplication.Application.Interfaces
{
    public interface IApplicationService
    {
  
        Task<int> ApplyAsync(int jobId, string appUserId);

  
        Task CancelAsync(int applicationId, string appUserId);
    }
}
