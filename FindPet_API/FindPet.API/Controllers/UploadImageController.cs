//using System.Net.Http.Headers;
//using FindPet.Core.Services.MLService;
//using FindPet.Domain.Entities;
//using FindPet.Infrastructure.Interfaces.IImageService;
//using FindPet.Infrastructure.Interfaces.IMLService;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace FindPet.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UploadImageController : ControllerBase
//    {
//        private readonly IManageImage<User> _manageImage;

//        public UploadImageController(IManageImage<User> manageImage)
//        {
//            _manageImage = manageImage;
//        }

//        [HttpPost, DisableRequestSizeLimit]
//        public async Task<IActionResult> UploadImage()
//        {
//            try
//            {
//                var formCollection = await Request.ReadFormAsync();
//                var file = formCollection.Files.First();
//                if (file.Length > 0)
//                {
//                    // Сохранить изображение
//                    var uniqueId = Guid.NewGuid();
//                    var filePath = await _manageImage.UploadPhotoAsync(file, uniqueId);

//                    // Вы можете добавить здесь обработку предсказания, например, сохранить результат в базу данных и т.д.

//                    return Ok(new { filePath });
//                }
//                else
//                {
//                    return BadRequest();
//                }
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex}");
//            }
//        }
//    }
//}
