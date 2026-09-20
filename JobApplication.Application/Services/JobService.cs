using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<int> CreateAsync(CreateJobDto createJobDto, string recruiterId)
        {
            var job = new Job
            {
                Title = createJobDto.Title,
                Description = createJobDto.Description,
                IsActive = createJobDto.IsActive,
                RecruiterId = recruiterId
            };

            await _jobRepository.InsertAsync(job);
            await _jobRepository.SaveChangesAsync();

            return job.Id;
        }

        public async Task CloseAsync(int jobId, string recruiterId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId)
                      ?? throw new NotFoundException(nameof(Job), jobId);

            if (job.RecruiterId != recruiterId)
                throw new ForbiddenException("Only the recruiter who posted this job can close it.");

            if (!job.IsActive && job.ClosedAt.HasValue)
                throw new BusinessRuleException("This job is already closed.");

            job.IsActive = false;
            job.ClosedAt = DateTime.UtcNow;
            job.ClosedBy = recruiterId;

            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();
        }
    }
}
