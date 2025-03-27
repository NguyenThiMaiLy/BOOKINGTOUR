const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
let currentChatUser = null;

// Khi nhận tin nhắn từ người dùng khác
connection.on("ReceiveMessageFromUser", (user, message) => {
    if (currentChatUser === user) {
        const li = document.createElement("li");
        li.className = 'received';
        li.textContent = message;

        // Thêm thời gian tin nhắn
        const timeSpan = document.createElement("span");
        timeSpan.className = "message-time";
        timeSpan.textContent = new Date().toLocaleTimeString();
        li.appendChild(timeSpan);

        document.getElementById("messagesList").appendChild(li);
    } else {
        showNotification(`Bạn có tin nhắn mới từ ${user}`);
    }
});

// Khi có sự kiện thông báo tin nhắn mới
connection.on("NewMessageNotification", () => {
    showNotification("Bạn có tin nhắn mới");
});

// Hàm để tải lại lịch sử trò chuyện
function updateChatHistory(userName) {
    connection.invoke("GetChatHistory", userName).then(history => {
        const messagesList = document.getElementById("messagesList");
        messagesList.innerHTML = ''; // Xóa hết tin nhắn cũ

        // Hiển thị lại toàn bộ lịch sử tin nhắn
        history.forEach(msg => {
            const li = document.createElement("li");
            li.textContent = `${msg.message}`;
            li.className = msg.senderId === "Admin" ? 'sent' : 'received';
            const timeSpan = document.createElement("span");
            timeSpan.className = "message-time";
            timeSpan.textContent = new Date(msg.timestamp).toLocaleTimeString();
            li.appendChild(timeSpan);
            messagesList.appendChild(li);
        });

        // Cuộn xuống cuối cùng
        messagesList.scrollTop = messagesList.scrollHeight;
    }).catch(err => console.error(err.toString()));
}

// Gửi tin nhắn từ admin tới user
function sendMessageToUser() {
    const userName = currentChatUser;
    const message = document.getElementById("messageInput").value;

    if (userName && message) {
        connection.invoke("SendMessageToUser", userName, message)
            .then(() => {
                // Hiển thị tin nhắn đã gửi
                const li = document.createElement("li");
                li.className = 'sent';
                li.textContent = message;
                const timeSpan = document.createElement("span");
                timeSpan.className = "message-time";
                timeSpan.textContent = new Date().toLocaleTimeString();
                li.appendChild(timeSpan);
                document.getElementById("messagesList").appendChild(li);
                document.getElementById("messageInput").value = "";

                // Tải lại lịch sử trò chuyện sau khi gửi tin nhắn
                updateChatHistory(userName);
            })
            .catch(err => console.error("Error sending message: " + err.toString()));
    } else {
        console.error("User name and message cannot be empty.");
    }
}

// Hàm chọn cuộc trò chuyện
function selectChat(userName) {
    currentChatUser = userName;
    console.log("Selected user:", currentChatUser); // Kiểm tra xem giá trị của currentChatUser có đúng không
    document.getElementById('chatWithUser').textContent = `Chat with ${userName}`;
    document.getElementById('messagesList').innerHTML = ''; // Xóa lịch sử tin nhắn cũ

    // Thêm class active cho người dùng đã chọn
    document.querySelectorAll('#userChatList li').forEach(li => {
        li.classList.remove('active');
    });
    document.querySelectorAll('#userChatList li').forEach(li => {
        if (li.querySelector("span").textContent.trim() === userName) {
            li.classList.add('active');
        }
    });

    // Tải lịch sử chat với người dùng đã chọn
    updateChatHistory(userName); // Cập nhật lại lịch sử trò chuyện
}

// Hàm tải danh sách người dùng đã chat
function loadChatUsers() {
    connection.invoke("GetChatUsers")
        .then(users => {
            const userChatList = document.getElementById("userChatList");
            userChatList.innerHTML = '';
            users.forEach(user => {
                const li = document.createElement("li");

                // Tạo thẻ ảnh cho avatar
                const avatarImg = document.createElement("img");
                avatarImg.src = user.avatarUrl || 'default-avatar.png'; // Thêm ảnh mặc định nếu không có avatar
                avatarImg.alt = user.fullname;
                avatarImg.className = "user-avatar";

                li.appendChild(avatarImg);

                // Thêm tên người dùng
                const userNameSpan = document.createElement("span");
                userNameSpan.textContent = user.fullname;
                li.appendChild(userNameSpan);

                // Thêm sự kiện khi nhấp vào để chọn cuộc trò chuyện
                li.onclick = () => selectChat(user.userName);

                userChatList.appendChild(li);
            });
        })
        .catch(err => console.error("Error fetching users: " + err.toString()));
}

// Hàm hiển thị thông báo
// Hàm hiển thị thông báo Toast
function showNotification(message) {
    const notification = document.createElement("div");
    notification.className = "notification";
    notification.textContent = message;
    document.body.appendChild(notification);

    // Hiệu ứng mượt mà khi thông báo xuất hiện
    notification.style.opacity = 0;
    setTimeout(() => {
        notification.style.transition = "opacity 0.5s";
        notification.style.opacity = 1;
    }, 0);

    // Tự động ẩn thông báo sau 3 giây
    setTimeout(() => {
        notification.style.opacity = 0;
        setTimeout(() => {
            notification.remove();
        }, 500); // Sau khi hiệu ứng ẩn xong
    }, 3000);
}

// Khởi động kết nối SignalR
connection.start().then(() => {
    console.log("Connected to SignalR.");
    loadChatUsers(); // Tải người dùng khi kết nối thành công
}).catch(err => console.error("Error connecting to SignalR: " + err.toString()));
