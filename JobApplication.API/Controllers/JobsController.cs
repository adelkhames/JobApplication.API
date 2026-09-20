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

   
        [HttpPut("{id:int}/close")]
        public async Task<IActionResult> Close(int id)
        {
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(recruiterId))
                return Unauthorized(new { message = "Could not identify the caller." });

            await _jobService.CloseAsync(id, recruiterId);

            return NoContent(); 
    }
}
