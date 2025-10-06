using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBackend.Models.Enums
{
    public enum MessageType
    {
        Text,
        Image,
        File,
        Emoji,
        System // For system messages like "User joined", "User left", etc.
    }
}
