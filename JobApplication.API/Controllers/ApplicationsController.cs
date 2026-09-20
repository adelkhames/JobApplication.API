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
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// POST /api/applications
        /// Candidate applies for a job.
        /// Business rules enforced: job must be active, no duplicate application.
        /// Returns 201 with the new application id.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Apply([FromBody] CreateApplicationDto dto)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(appUserId))
                return Unauthorized(new { message = "Could not identify the caller." });

            var id = await _applicationService.ApplyAsync(dto.JobId, appUserId);

            return CreatedAtAction(nameof(Apply), new { id }, new { id });
        }

        /// <summary>
        /// DELETE /api/applications/{id}
        /// Candidate cancels their own application.
        /// Returns 204 on success.
        /// Returns 403 if the caller does not own the application.
        /// Returns 400 if the application status is Interview, Accepted, or Rejected.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(appUserId))
                return Unauthorized(new { message = "Could not identify the caller." });

            await _applicationService.CancelAsync(id, appUserId);

            return NoContent(); // 204
        }
    }
}
