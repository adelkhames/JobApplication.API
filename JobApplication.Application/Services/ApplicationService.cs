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

        /// <inheritdoc/>
        public async Task<int> ApplyAsync(int jobId, string appUserId)
        {
            // 1. Resolve the Candidate domain entity from the JWT subject
            var candidate = await _applicationRepository.GetCandidateByAppUserIdAsync(appUserId)
                            ?? throw new NotFoundException(nameof(Candidate), appUserId);

            // 2. Validate the job exists and is active
            var job = await _jobRepository.GetByIdAsync(jobId)
                      ?? throw new NotFoundException(nameof(Job), jobId);

            if (!job.IsActive)
                throw new BusinessRuleException("Cannot apply to a job that is not active.");

            // 3. Reject duplicate applications
            var alreadyApplied = await _applicationRepository.ExistsAsync(jobId, candidate.Id);
            if (alreadyApplied)
                throw new ConflictException("You have already applied to this job.");

            // 4. Create the application
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

        /// <inheritdoc/>
        public async Task CancelAsync(int applicationId, string appUserId)
        {
            // 1. Fetch the application
            var application = await _applicationRepository.GetByIdAsync(applicationId)
                              ?? throw new NotFoundException(nameof(JobCandidateApplication), applicationId);

            // 2. Verify ownership — resolve caller's Candidate record
            var candidate = await _applicationRepository.GetCandidateByAppUserIdAsync(appUserId)
                            ?? throw new NotFoundException(nameof(Candidate), appUserId);

            if (application.CandidateId != candidate.Id)
                throw new ForbiddenException("You can only cancel your own applications.");

            // 3. Business rule: cannot cancel if already progressed past UnderReview
            if (application.JobApplicationStatus == JobApplicationStatus.Interview ||
                application.JobApplicationStatus == JobApplicationStatus.Accepted ||
                application.JobApplicationStatus == JobApplicationStatus.Rejected)
            {
                throw new BusinessRuleException(
                    $"Cannot cancel an application with status '{application.JobApplicationStatus}'. " +
                    "Only Applied or UnderReview applications can be cancelled.");
            }

            // 4. Cancel
            application.JobApplicationStatus = JobApplicationStatus.Cancelled;
            application.CancelledAt = DateTime.UtcNow;
            application.StatusUpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(application);
            await _applicationRepository.SaveChangesAsync();
        }
    }
}
