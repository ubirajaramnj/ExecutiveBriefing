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

        /// <summary>
        /// Generates a structured executive briefing for a company.
        /// </summary>
        /// <param name="companyName">The name of the company.</param>
        /// <param name="market">The primary market or country of operation (optional).</param>
        /// <param name="websiteUrl">The official website URL (optional).</param>
        /// <param name="irPageUrl">The investor relations page URL (optional).</param>
        /// <param name="additionalLinks">List of additional source links (optional).</param>
        /// <param name="attachments">PDF/document uploads (optional).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The generated briefing with sections and sources.</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Briefing), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
