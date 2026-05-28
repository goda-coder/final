using final.Application.DTOs;
using final.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace final.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrustedContactsController : ControllerBase
    {
        private readonly ITrustedContactService _service;

        public TrustedContactsController(ITrustedContactService service)
        {
            _service = service;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddTrustedContact(AddTrustedContactDto dto)
        {
            var result = await _service.AddTrustedContactAsync(dto);

            return Ok(result);
        }

        [HttpPost("quick-transfer")]
        public async Task<IActionResult> QuickTransfer(QuickTransferDto dto)
        {
            var result = await _service.QuickTransferAsync(dto);

            return Ok(result);
        }

        [HttpGet("{userId}/list")]
        public async Task<IActionResult> GetTrustedContacts(int userId)
        {
            var result = await _service.GetTrustedContactsAsync(userId);

            return Ok(result);
        }
    }
}