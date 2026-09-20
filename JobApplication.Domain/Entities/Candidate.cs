using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class Candidate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string CvUrl { get; set; } = string.Empty;

        /// <summary>Links this Candidate record to the Identity AppUser (stored as string Id).</summary>
        public string AppUserId { get; set; } = string.Empty;
    }
}
