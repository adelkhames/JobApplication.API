using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Identity
{
    /// <summary>
    /// Extends IdentityUser with a Role field to distinguish Recruiter vs Candidate.
    /// </summary>
    public class AppUser : IdentityUser
    {
        /// <summary>"Recruiter" or "Candidate"</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Friendly display name (maps to Candidate.Name for candidates,
        /// or a recruiter's name for recruiters).
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
