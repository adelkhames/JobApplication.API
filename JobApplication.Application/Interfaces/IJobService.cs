using JobApplication.Application.DTOs;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces
{
    public interface IJobService
    {
        Task<int> CreateAsync(CreateJobDto createJobDto, string recruiterId);
        Task CloseAsync(int jobId, string recruiterId);
    }
}
