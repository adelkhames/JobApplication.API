using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        /// <summary>
        /// POST /api/jobs
        /// Recruiter creates a new job posting.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJobDto createJobDto)
        {
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(recruiterId))
                return Unauthorized(new { message = "Could not identify the caller." });

            var id = await _jobService.CreateAsync(createJobDto, recruiterId);

            return Ok(new { id });
        }

        /// <summary>
        /// PUT /api/jobs/{id}/close
        /// Recruiter closes a job. Only the recruiting owner may close it.
        /// Returns 204 on success, 403 if not the owner, 404 if job not found.
        /// </summary>
        [HttpPut("{id:int}/close")]
        public async Task<IActionResult> Close(int id)
        {
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(recruiterId))
                return Unauthorized(new { message = "Could not identify the caller." });

            await _jobService.CloseAsync(id, recruiterId);

            return NoContent(); // 204
        }
    }
}
