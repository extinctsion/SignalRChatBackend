using Microsoft.EntityFrameworkCore;
using ChatBackend.Data;
using ChatBackend.Models.Enums;
using ChatBackend.Models.Entities;
using ChatBackend.Models.DTOs.User;

namespace ChatBackend.Services
{
    public class UserService : IUserService
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<UserService> _logger;
        
        public UserService(ChatDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<UserDto> CreateUserAsync(CreateUserDto userDto)
        {
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                AvatarUrl = userDto.AvatarUrl,
                Status = UserStatus.Offline
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User created with ID: {UserId}", user.Id);
            
            return MapToDto(user);
        }
        
        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null ? MapToDto(user) : null;
        }
        
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null ? MapToDto(user) : null;
        }
        
        public async Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 50)
        {
            var users = await _context.Users
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return users.Select(MapToDto).ToList();
        }
        
        public async Task<UserDto> UpdateUserStatusAsync(Guid userId, UserStatus status)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found", nameof(userId));
                
            user.Status = status;
            user.LastSeen = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return MapToDto(user);
        }
        
        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
                
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User deleted: {UserId}", userId);
            return true;
        }
        
        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Status = user.Status,
                LastSeen = user.LastSeen,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
