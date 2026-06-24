using Microsoft.AspNetCore.Mvc;
using ExecutiveBriefing.ApplicationServices.Services;
using ExecutiveBriefing.Domain.Aggregates;

namespace ExecutiveBriefing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BriefingsController : ControllerBase
    {
        private readonly BriefingService _briefingService;

        public BriefingsController(BriefingService briefingService)
        {
            _briefingService = briefingService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> GenerateBriefing(
            [FromForm] string companyName,
            [FromForm] string? market,
            [FromForm] string? websiteUrl,
            [FromForm] string? irPageUrl,
            [FromForm] List<string>? additionalLinks,
            [FromForm] List<IFormFile>? attachments,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return BadRequest("Company name is required.");
            }

            var attachmentStreams = new List<(string Filename, Stream Content)>();

            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        var stream = file.OpenReadStream();
                        attachmentStreams.Add((file.FileName, stream));
                    }
                }
            }

            try
            {
                var briefing = await _briefingService.GenerateBriefingAsync(
                    companyName,
                    market,
                    websiteUrl,
                    irPageUrl,
                    additionalLinks ?? new List<string>(),
                    attachmentStreams,
                    cancellationToken
                );

                return Ok(briefing);
            }
            finally
            {
                // Clean up streams
                foreach (var attachment in attachmentStreams)
                {
                    await attachment.Content.DisposeAsync();
                }
            }
        }
    }
}
