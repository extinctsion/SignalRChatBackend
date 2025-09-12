# SignalR Chat Backend

A scalable real-time chat backend built with ASP.NET Core 8, SignalR, PostgreSQL, and Redis. Supports OAuth authentication, file uploads to S3, and can handle up to 1 million concurrent users.

## Features

- **Real-time messaging** with SignalR
- **OAuth authentication** with JWT tokens
- **PostgreSQL database** with Entity Framework Core
- **Redis backplane** for horizontal scaling
- **File uploads** to AWS S3 (images, documents)
- **Message delivery status** (sent, delivered, read)
- **Online/offline user status**
- **Typing indicators**
- **Group chat support**
- **Message persistence** for 1 year
- **Structured logging** with Serilog
- **Health checks** for monitoring

## Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Client App    │    │   Client App    │    │   Client App    │
│  (Frontend)     │    │  (Frontend)     │    │  (Frontend)     │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                    ┌─────────────┴─────────────┐
                    │     Load Balancer         │
                    └─────────────┬─────────────┘
                                 │
          ┌──────────────────────┼──────────────────────┐
          │                      │                      │
┌─────────┴───────┐    ┌─────────┴───────┐    ┌─────────┴───────┐
│  Chat Backend   │    │  Chat Backend   │    │  Chat Backend   │
│   Instance 1    │    │   Instance 2    │    │   Instance N    │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                    ┌─────────────┴─────────────┐
                    │     Redis Backplane       │
                    └───────────────────────────┘
                                 │
                    ┌─────────────┴─────────────┐
                    │   PostgreSQL Database     │
                    └───────────────────────────┘
```

## Database Schema

### Core Entities
- **Users**: User accounts with authentication info
- **Conversations**: Group chats or direct messages
- **UserConversations**: Many-to-many relationship between users and conversations
- **Messages**: Chat messages with support for text, files, images, emojis
- **MessageStatus**: Delivery tracking (sent, delivered, read)

## Prerequisites

1. **.NET 8 SDK** (for local development)
2. **Docker & Docker Compose** (for containerized deployment)
3. **PostgreSQL** (external - not containerized)
4. **AWS Account** (for S3 file storage)

## Quick Start with Docker

### 1. Using Docker Compose (Recommended)

1. **Clone and navigate to the project**:
   ```bash
   cd d:\EC\SignalR\try4
   ```

2. **Ensure PostgreSQL is running** on your host machine with the database `chatbackend`

3. **Start the application**:
   ```bash
   # Windows PowerShell
   .\docker-helper.ps1 up
   
   # Or manually
   docker-compose up -d
   ```

4. **Access the application**:
   - API: http://localhost:5000
   - HTTPS API: https://localhost:5001
   - SignalR Hub: ws://localhost:5000/hubs/chat
   - Health Check: http://localhost:5000/health

5. **View logs**:
   ```bash
   .\docker-helper.ps1 logs-f
   ```

6. **Stop the application**:
   ```bash
   .\docker-helper.ps1 down
   ```

### 2. Docker Helper Commands

Use the provided helper scripts for easy management:

```bash
# Windows PowerShell
.\docker-helper.ps1 [command]

# Available commands:
# up       - Start the application
# down     - Stop the application  
# restart  - Restart the application
# logs     - Show logs
# logs-f   - Follow logs in real-time
# build    - Build the application
# clean    - Clean up containers and volumes
# status   - Show container status
# tools    - Start with Redis Commander UI
# help     - Show help
```

## Manual Setup (Alternative)

### 1. Database Setup

1. Create a PostgreSQL database named `chatbackend`
2. Update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=chatbackend;Username=postgres;Password=your_password"
   }
   ```

### 2. Redis Setup

Install and start Redis:
```bash
# On Windows (using Chocolatey)
choco install redis-64

# Start Redis
redis-server
```

### 3. AWS S3 Setup

1. Create an S3 bucket for file storage
2. Configure AWS credentials:
   - Set AWS credentials via AWS CLI: `aws configure`
   - Or use environment variables: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`
3. Update bucket name in `appsettings.json`:
   ```json
   "AWS": {
     "S3": {
       "BucketName": "your-chat-files-bucket"
     }
   }
   ```

### 4. JWT Configuration

Update the JWT secret key in `appsettings.json`:
```json
"Jwt": {
  "Key": "your-256-bit-secret-key-here-make-it-very-long-and-secure",
  "Issuer": "ChatBackend",
  "Audience": "ChatBackendUsers"
}
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7xxx`
- HTTP: `http://localhost:5xxx`
- SignalR Hub: `wss://localhost:7xxx/hubs/chat`

## API Endpoints

### Authentication
- All endpoints require JWT authentication except user creation
- Include JWT token in Authorization header: `Bearer <token>`

### Users
- `GET /api/users` - Get all users (paginated)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/status` - Update user status
- `DELETE /api/users/{id}` - Delete user

### Conversations
- `GET /api/conversations` - Get user's conversations
- `GET /api/conversations/{id}` - Get conversation details
- `POST /api/conversations` - Create new conversation
- `POST /api/conversations/{id}/members` - Add member to conversation
- `DELETE /api/conversations/{id}/members/{userId}` - Remove member
- `PUT /api/conversations/{id}/members/{userId}/role` - Update member role
- `DELETE /api/conversations/{id}` - Delete conversation

### Messages
- `GET /api/messages?conversationId={id}` - Get messages (paginated)
- `GET /api/messages/{id}` - Get message by ID
- `POST /api/messages` - Send new message
- `PUT /api/messages/{id}/status` - Update message status
- `DELETE /api/messages/{id}` - Delete message

### Files
- `POST /api/files/upload` - Upload file to S3
- `DELETE /api/files?fileUrl={url}` - Delete file from S3
- `GET /api/files/presigned-url?fileKey={key}` - Get presigned URL

### Health
- `GET /health` - Health check endpoint

## SignalR Hub Events

### Client to Server
- `SendMessage(CreateMessageDto)` - Send a message
- `JoinConversation(Guid)` - Join a conversation room
- `LeaveConversation(Guid)` - Leave a conversation room
- `StartTyping(Guid)` - Indicate user is typing
- `StopTyping(Guid)` - Indicate user stopped typing
- `MarkMessageAsRead(Guid)` - Mark message as read
- `UpdateStatus(UserStatus)` - Update user online status

### Server to Client
- `ReceiveMessage(MessageDto)` - New message received
- `UserStatusChanged(object)` - User went online/offline
- `UserStartedTyping(object)` - User started typing
- `UserStoppedTyping(object)` - User stopped typing
- `MessageStatusUpdated(object)` - Message delivery status changed
- `Error(string)` - Error message

## Scaling for 1 Million Users

### Horizontal Scaling
1. **Multiple Backend Instances**: Deploy multiple instances behind a load balancer
2. **Redis Backplane**: Ensures SignalR messages are distributed across all instances
3. **Database Connection Pooling**: Configure appropriate connection pool sizes
4. **Sticky Sessions**: Configure load balancer for WebSocket persistence

### Performance Optimizations
1. **Message Pagination**: Limit message history loaded at once
2. **Database Indexing**: Optimized indexes on frequently queried columns
3. **Caching**: Redis caching for frequently accessed data
4. **File Storage**: S3 for scalable file storage
5. **Message Cleanup**: Background job to clean up old messages (1+ year)

### Infrastructure Recommendations
- **Load Balancer**: AWS ALB or NGINX
- **Database**: AWS RDS PostgreSQL with read replicas
- **Redis**: AWS ElastiCache Redis cluster
- **File Storage**: AWS S3 with CloudFront CDN
- **Monitoring**: Application Insights or Datadog

## Docker Configuration

### Services Included:
- **Chat Backend**: The main SignalR API application
- **Redis**: For SignalR backplane and caching
- **Redis Commander** (optional): Web UI for Redis management

### Environment Variables:
The application uses these environment variables (configurable in docker-compose.yml):

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection
- `ConnectionStrings__Redis`: Redis connection  
- `Jwt__Key`: JWT secret key
- `AWS_ACCESS_KEY_ID`: AWS credentials
- `AWS_SECRET_ACCESS_KEY`: AWS credentials
- `AWS__S3__BucketName`: S3 bucket name

### Production Deployment:
1. Copy `.env.example` to `.env` and update values
2. Use `docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d`
3. Configure reverse proxy (nginx/traefik) for SSL termination
4. Set up monitoring and log aggregation

## Development

### Adding New Features
1. Update models in `/Models`
2. Update DTOs in `/DTOs`
3. Update services in `/Services`
4. Update controllers in `/Controllers`
5. Update SignalR hub in `/Hubs`
6. Add database migrations if needed

### Testing
```bash
# Run unit tests
dotnet test

# Test SignalR connection
# Use Postman or a SignalR client to test the WebSocket connection
```

## Security Considerations

1. **JWT Token Security**: Use strong, unique secret keys
2. **Input Validation**: All inputs are validated
3. **SQL Injection Prevention**: Entity Framework provides protection
4. **File Upload Security**: File type and size validation
5. **CORS Configuration**: Configure appropriately for production
6. **HTTPS**: Always use HTTPS in production
7. **Rate Limiting**: Consider implementing rate limiting for message sending

## Deployment

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChatBackend.csproj", "."]
RUN dotnet restore "ChatBackend.csproj"
COPY . .
RUN dotnet build "ChatBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatBackend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatBackend.dll"]
```

### Environment Variables
```bash
ConnectionStrings__DefaultConnection=Host=db;Database=chatbackend;Username=postgres;Password=password
ConnectionStrings__Redis=redis:6379
Jwt__Key=your-production-secret-key
AWS_ACCESS_KEY_ID=your-aws-key
AWS_SECRET_ACCESS_KEY=your-aws-secret
```

## License

This project is licensed under the MIT License.
