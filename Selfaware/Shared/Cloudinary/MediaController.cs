using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Selfaware.Shared.Cloudinary
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController:ControllerBase
    {
        private readonly IImageService _imageService;

        public MediaController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("upload-signature")]
        public IActionResult GetUploadSignature([FromQuery] string folder)
        {
            var result = _imageService.GenerateUploadSignature(folder);
            return Ok(result);
        }
    }
}
