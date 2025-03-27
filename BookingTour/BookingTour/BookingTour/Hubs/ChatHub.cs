using BookingTour.Data;
using BookingTour.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookingTour.Hubs
{
    public class ChatHub : Hub
    {
        private readonly YourExistingDbContextName _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(YourExistingDbContextName context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SendMessageToAdmin(string message)
        {
            var userName = Context.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            // Tạo tin nhắn mới
            var chatMessage = new ChatMessage
            {
                SenderId = user.Id,
                ReceiverId = "Admin",  // admin cần được lấy từ hệ thống (Id = "Admin" hoặc theo IdRole)
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            // Lưu tin nhắn vào cơ sở dữ liệu
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Gửi tin nhắn đến group Admin
            await Clients.Group("Admins").SendAsync("ReceiveMessageFromUser", userName, message);

            // Thông báo cho admin có tin nhắn mới
            await NotifyNewMessage("Admin");
        }

        public async Task SendMessageToUser(string userName, string message)
        {
            var user = await _userManager.FindByNameAsync(userName);

            // Tạo tin nhắn mới
            var chatMessage = new ChatMessage
            {
                SenderId = "Admin",  // Từ admin gửi đến người dùng
                ReceiverId = user.Id,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            // Lưu tin nhắn vào cơ sở dữ liệu
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Gửi tin nhắn đến người dùng
            await Clients.User(user.Id).SendAsync("ReceiveMessageFromAdmin", "Admin", message);

            // Thông báo cho người dùng có tin nhắn mới
            await NotifyNewMessage(user.Id);
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User.IsInRole("Admin"))
            {
                // Khi admin kết nối, thêm vào nhóm Admins
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            await base.OnConnectedAsync();
        }
        public async Task<List<ApplicationUser>> GetChatUsers()
        {
            var chatUserIds = await _context.ChatMessages
                .Where(msg => msg.ReceiverId != "Admin" || msg.SenderId != "Admin")
                .Select(msg => msg.SenderId == "Admin" ? msg.ReceiverId : msg.SenderId)
                .Distinct()
                .ToListAsync();

            var chatUsers = await _userManager.Users
                .Where(user => chatUserIds.Contains(user.Id))
                .Select(user => new ApplicationUser
                {fullname = user.fullname,
                    UserName = user.UserName,
                    AvatarUrl = user.AvatarUrl,
                })
                .ToListAsync();
            foreach (var user in chatUsers)
            {
                Console.WriteLine($"User: {user.fullname}, AvatarUrl: {user.AvatarUrl}");
            }
            return chatUsers;
        }
        public async Task<List<ChatMessage>> GetChatHistory(string userName)
        {
            ApplicationUser user;
            if (String.IsNullOrEmpty(userName))
            {
                userName = Context.User.Identity.Name;
                user = await _userManager.FindByNameAsync(userName);
            }
            else
            {
                user = await _userManager.FindByNameAsync(userName);
            }

            if (user == null)
            {
                return new List<ChatMessage>();
            }

            var messages = await _context.ChatMessages
                .Where(msg => (msg.ReceiverId == user.Id && msg.SenderId == "Admin") ||
                              (msg.SenderId == user.Id && msg.ReceiverId == "Admin"))
                .OrderBy(msg => msg.Timestamp)
                .ToListAsync();

            return messages;
        }
        public async Task SendMessageToHost(long tourId, string message)
        {
            var userName = Context.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            // Get the host of the tour
            var tour = await _context.Tours.Include(t => t.IdUserNavigation)
                                           .FirstOrDefaultAsync(t => t.IdTour == tourId);
            if (tour == null || tour.IdUser == null)
            {
                return;
            }

            var host = tour.IdUserNavigation;
            if (host == null)
            {
                return;
            }

            // Create new chat message
            var chatMessage = new ChatMessage
            {
                SenderId = user.Id,
                ReceiverId = "Host",
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            // Save message to the database
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Send message to the host
            await Clients.User(host.Id).SendAsync("ReceiveMessageFromUser", userName, message);

            // Notify host of new message
            await NotifyNewMessage(host.Id);
        }

        // Method for host to reply to a user
        public async Task SendMessageToUserFromHost(string userId, string message)
        {
            var hostName = Context.User.Identity.Name;
            var host = await _userManager.FindByNameAsync(hostName);

            // Create new chat message from host
            var chatMessage = new ChatMessage
            {
                SenderId = "Host",
                ReceiverId = userId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            // Save message to the database
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Send message to the user
            await Clients.User(userId).SendAsync("ReceiveMessageFromHost", host.UserName, message);

            // Notify user of new message
            await NotifyNewMessage(userId);
        }
        public async Task<List<ChatMessage>> GetHostyChatHistory(string userId)
        {
            var hostId = Context.User.Identity.Name;
            var host = await _userManager.FindByNameAsync(hostId);

            if (host == null || string.IsNullOrEmpty(userId))
            {
                return new List<ChatMessage>();
            }

            var messages = await _context.ChatMessages
                .Where(msg => (msg.SenderId == host.Id && msg.ReceiverId == userId) ||
                              (msg.SenderId == userId && msg.ReceiverId == host.Id))
                .OrderBy(msg => msg.Timestamp)
                .ToListAsync();

            return messages;
        }
        public async Task SendMessageToBoth(long tourId, string message)
        {
            var userName = Context.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            // Get the tour and host
            var tour = await _context.Tours.Include(t => t.IdUserNavigation)
                                           .FirstOrDefaultAsync(t => t.IdTour == tourId);
            if (tour == null || tour.IdUser == null)
            {
                return;
            }

            var host = tour.IdUserNavigation;
            if (host == null)
            {
                return;
            }

            // Create new chat message
            var chatMessage = new ChatMessage
            {
                SenderId = user.Id,
                ReceiverId = host.Id,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            // Save message to the database
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Send message to both user and host
            await Clients.User(user.Id).SendAsync("ReceiveMessageFromUser", userName, message);
            await Clients.User(host.Id).SendAsync("ReceiveMessageFromHost", userName, message);

            // Notify both user and host of new messages
            await NotifyNewMessage(user.Id);
            await NotifyNewMessage(host.Id);
        }

        // Method for user to send message to the host directly
        public async Task SendMessageToHostDirectly(long tourId, string message)
        {
            var userName = Context.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            // Get the host of the tour
            var tour = await _context.Tours.Include(t => t.IdUserNavigation)
                                           .FirstOrDefaultAsync(t => t.IdTour == tourId);
            if (tour == null || tour.IdUser == null)
            {
                return;
            }

            var host = tour.IdUserNavigation;
            if (host == null)
            {
                return;
            }

            // Create new chat message from user to host
            var chatMessage = new ChatMessage
            {
                SenderId = user.Id,
                ReceiverId = host.Id,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            // Save message to the database
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Send the message to the host
            await Clients.User(host.Id).SendAsync("ReceiveMessageFromUser", user.UserName, message);

            // Notify host of new message
            await NotifyNewMessage(host.Id);
        }
        public async Task NotifyNewMessage(string receiverId)
        {
            // Gửi thông báo tin nhắn mới đến người nhận
            await Clients.User(receiverId).SendAsync("NewMessageNotification");
        }
    }
}
