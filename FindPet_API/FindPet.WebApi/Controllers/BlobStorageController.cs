using FindPet.BusinessLogicLayer.CQRS.Commands.BlobStorage;
using FindPet.BusinessLogicLayer.CQRS.Queries.BlobStorage;
using FindPet.Domain.DTOs.FileDTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FindPet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlobStorageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BlobStorageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Upload a single file to blob storage
        /// </summary>
        /// <param name="uploadDto">File upload data</param>
        /// <returns>Upload result with file URL</returns>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(200, Type = typeof(UploadFileResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(413)] // Payload Too Large
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto uploadDto)
        {
            var command = new UploadFileCommand(uploadDto.File, uploadDto.Subfolder, uploadDto.EntityId);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Upload multiple files to blob storage
        /// </summary>
        /// <param name="uploadDto">Multiple files upload data</param>
        /// <returns>Upload results with file URLs and status</returns>
        [HttpPost("upload/multiple")]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(200, Type = typeof(UploadMultipleFilesResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(413)] // Payload Too Large
        public async Task<IActionResult> UploadMultipleFiles([FromForm] MultipleFileUploadDto uploadDto)
        {
            var command = new UploadMultipleFilesCommand(uploadDto.Files, uploadDto.Subfolder, uploadDto.EntityId);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Download a file from blob storage
        /// </summary>
        /// <param name="fileUrl">The URL of the file to download</param>
        /// <returns>File stream</returns>
        [AllowAnonymous]
        [HttpGet("download")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DownloadFile([FromQuery] string fileUrl)
        {
            var query = new GetFileQuery(fileUrl);
            var result = await _mediator.Send(query);

            return File(result.FileStream, result.ContentType, result.FileName);
        }

        /// <summary>
        /// Get file stream for viewing (inline display)
        /// </summary>
        /// <param name="fileUrl">The URL of the file to view</param>
        /// <returns>File stream for inline display</returns>
        [AllowAnonymous]
        [HttpGet("view")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ViewFile([FromQuery] string fileUrl)
        {
            var query = new GetFileQuery(fileUrl);
            var result = await _mediator.Send(query);

            Response.Headers.Add("Content-Disposition", "inline");
            return File(result.FileStream, result.ContentType);
        }

        /// <summary>
        /// Delete a file from blob storage
        /// </summary>
        /// <param name="fileUrl">The URL of the file to delete</param>
        /// <returns>Deletion result</returns>
        [HttpDelete]
        [ProducesResponseType(200, Type = typeof(DeleteFileResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl)
        {
            var command = new DeleteFileCommand(fileUrl);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        /// <summary>
        /// Check if a file exists in blob storage
        /// </summary>
        /// <param name="fileUrl">The URL of the file to check</param>
        /// <returns>File existence status</returns>
        [AllowAnonymous]
        [HttpHead("exists")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CheckFileExists([FromQuery] string fileUrl)
        {
            var query = new CheckFileExistsQuery(fileUrl);
            var result = await _mediator.Send(query);

            return result.Exists ? Ok() : NotFound();
        }

        /// <summary>
        /// Get file existence status with detailed response
        /// </summary>
        /// <param name="fileUrl">The URL of the file to check</param>
        /// <returns>File existence details</returns>
        [AllowAnonymous]
        [HttpGet("exists")]
        [ProducesResponseType(200, Type = typeof(CheckFileExistsResponse))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFileExistsStatus([FromQuery] string fileUrl)
        {
            var query = new CheckFileExistsQuery(fileUrl);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get file URL for a given filename and subfolder
        /// </summary>
        /// <param name="fileUrlDto">File URL request data</param>
        /// <returns>File URL</returns>
        [AllowAnonymous]
        [HttpPost("url")]
        [ProducesResponseType(200, Type = typeof(GetFileUrlResponse))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFileUrl([FromBody] FileUrlDto fileUrlDto)
        {
            var query = new GetFileUrlQuery(fileUrlDto.FileName, fileUrlDto.Subfolder);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        /// <summary>
        /// Get storage statistics (if needed for admin purposes)
        /// </summary>
        /// <returns>Storage usage statistics</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("stats")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetStorageStats()
        {
            // This could be implemented if you need storage statistics
            // For now, return a placeholder response
            return Ok(new { message = "Storage statistics endpoint - implement as needed" });
        }
    }
}
