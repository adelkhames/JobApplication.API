using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class Job
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        /// <summary>AppUser.Id of the recruiter who posted this job.</summary>
        public string RecruiterId { get; set; } = string.Empty;

        /// <summary>Set when the recruiter closes the job.</summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>AppUser.Id of the recruiter who closed the job.</summary>
        public string? ClosedBy { get; set; }
    }
}
