using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobRepository _jobRepository;

        public ApplicationService(
            IApplicationRepository applicationRepository,
            IJobRepository jobRepository)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
        }

        public async Task<int> ApplyAsync(int jobId, string appUserId)
        {
            var candidate = await _applicationRepository.GetCandidateByAppUserIdAsync(appUserId)
                            ?? throw new NotFoundException(nameof(Candidate), appUserId);

            var job = await _jobRepository.GetByIdAsync(jobId)
                      ?? throw new NotFoundException(nameof(Job), jobId);

            if (!job.IsActive)
                throw new BusinessRuleException("Cannot apply to a job that is not active.");

            var alreadyApplied = await _applicationRepository.ExistsAsync(jobId, candidate.Id);
            if (alreadyApplied)
                throw new ConflictException("You have already applied to this job.");

            var application = new JobCandidateApplication
            {
                JobId = jobId,
                CandidateId = candidate.Id,
                JobApplicationStatus = JobApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                StatusUpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.InsertAsync(application);
            await _applicationRepository.SaveChangesAsync();

            return application.Id;
        }

        public async Task CancelAsync(int applicationId, string appUserId)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId)
                              ?? throw new NotFoundException(nameof(JobCandidateApplication), applicationId);

            var candidate = await _applicationRepository.GetCandidateByAppUserIdAsync(appUserId)
                            ?? throw new NotFoundException(nameof(Candidate), appUserId);

            if (application.CandidateId != candidate.Id)
                throw new ForbiddenException("You can only cancel your own applications.");

            if (application.JobApplicationStatus == JobApplicationStatus.Interview ||
                application.JobApplicationStatus == JobApplicationStatus.Accepted ||
                application.JobApplicationStatus == JobApplicationStatus.Rejected)
            {
                throw new BusinessRuleException(
                    $"Cannot cancel an application with status '{application.JobApplicationStatus}'. " +
                    "Only Applied or UnderReview applications can be cancelled.");
            }

            application.JobApplicationStatus = JobApplicationStatus.Cancelled;
            application.CancelledAt = DateTime.UtcNow;
            application.StatusUpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(application);
            await _applicationRepository.SaveChangesAsync();
        }
    }
}
