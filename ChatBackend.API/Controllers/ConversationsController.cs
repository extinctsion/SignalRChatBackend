using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChatBackend.Services;
using ChatBackend.Models.DTOs.Conversation;
using System.Security.Claims;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly ILogger<ConversationsController> _logger;
        
        public ConversationsController(IConversationService conversationService, ILogger<ConversationsController> logger)
        {
            _conversationService = conversationService;
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<ConversationDto>>> GetUserConversations()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();
                    
                var conversations = await _conversationService.GetUserConversationsAsync(userId.Value);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user conversations");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDto>> GetConversation(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();
                    
                var conversation = await _conversationService.GetConversationByIdAsync(id, userId.Value);
                if (conversation == null)
                    return NotFound();
                    
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation {ConversationId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPost]
        public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationDto conversationDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();
                    
                var conversation = await _conversationService.CreateConversationAsync(userId.Value, conversationDto);
                return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDto memberDto)
        {
            try
            {
                var result = await _conversationService.AddUserToConversationAsync(id, memberDto.UserId, memberDto.Role);
                if (!result)
                    return BadRequest("Failed to add member to conversation");
                    
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member to conversation {ConversationId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
        {
            try
            {
                var result = await _conversationService.RemoveUserFromConversationAsync(id, userId);
                if (!result)
                    return BadRequest("Failed to remove member from conversation");
                    
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member from conversation {ConversationId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPut("{id}/members/{userId}/role")]
        public async Task<IActionResult> UpdateMemberRole(Guid id, Guid userId, [FromBody] UpdateRoleDto roleDto)
        {
            try
            {
                var result = await _conversationService.UpdateUserRoleAsync(id, userId, roleDto.Role);
                if (!result)
                    return BadRequest("Failed to update member role");
                    
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member role in conversation {ConversationId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConversation(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized();
                    
                var result = await _conversationService.DeleteConversationAsync(id, userId.Value);
                if (!result)
                    return BadRequest("Failed to delete conversation or insufficient permissions");
                    
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation {ConversationId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
    
   
}
