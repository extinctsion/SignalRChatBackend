using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChatBackend.Services;
using ChatBackend.DTOs;
using System.Security.Claims;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;
        
        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<MessageDto>>> GetMessages(
            [FromQuery] Guid conversationId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var messages = await _messageService.GetMessagesAsync(conversationId, page, pageSize);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for conversation {ConversationId}", conversationId);
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<MessageDto>> GetMessage(Guid id)
        {
            try
            {
                var message = await _messageService.GetMessageByIdAsync(id);
                if (message == null)
                    return NotFound();
                    
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving message {MessageId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage([FromBody] CreateMessageDto messageDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();
                    
                var message = await _messageService.CreateMessageAsync(userId.Value, messageDto);
                return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating message");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();
                    
                var result = await _messageService.DeleteMessageAsync(id, userId.Value);
                if (!result)
                    return BadRequest("Failed to delete message or insufficient permissions");
                    
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateMessageStatus(Guid id, [FromBody] UpdateMessageStatusDto statusDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();
                    
                await _messageService.UpdateMessageStatusAsync(id, userId.Value, statusDto.Status);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message status for message {MessageId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
    
    public class UpdateMessageStatusDto
    {
        public ChatBackend.Models.DeliveryStatus Status { get; set; }
    }
}
