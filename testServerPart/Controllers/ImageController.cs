using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using testServerPart.Contracts;
using testServerPart.Extensions;
using testServerPart.Models;

namespace testServerPart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpGet("getAllImages")]
        public async Task<ConcurrentQueue<ImageData>> Get()
        {
            try
            {
                return await _imageService.Get();
            }
            catch (Exception)
            {
                return new ConcurrentQueue<ImageData>() { };
            }
        }

        [HttpPost("saveImg")]
        public async Task<IActionResult> Save([FromBody] ImageData data)
        {
            try
            {
                await _imageService.Save(data);
                return Ok();
            }
            catch (ImageException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }

        [HttpPut("editImg")]
        public async Task<IActionResult> Edit([FromBody] ImageData data)
        {
            try
            {
                await _imageService.Edit(data);
                return Ok();
            }
            catch (ImageException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }

        [HttpDelete("deleteImg")]
        public async Task<IActionResult> Delete(string imageDataPath)
        {
            try
            {
                await _imageService.Delete(imageDataPath);
                return Ok();
            }
            catch (ImageException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }
    }
}
