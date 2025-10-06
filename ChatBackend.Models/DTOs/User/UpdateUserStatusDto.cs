using ChatBackend.Models.Enums;

namespace ChatBackend.Models.DTOs.User
{
    public class UpdateUserStatusDto
    {
        public UserStatus Status { get; set; }
    }
}
