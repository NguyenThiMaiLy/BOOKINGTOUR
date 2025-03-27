const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.on("ReceiveMessageFromUser", (userName, message) => {
    const chatBox = document.getElementById("chatBox");
    const newMessage = document.createElement("div");
    newMessage.classList.add("message", "user-message");
    newMessage.textContent = `${userName}: ${message}`;
    chatBox.appendChild(newMessage);
});

connection.on("NewMessageNotification", () => {
    // Hiển thị thông báo tin nhắn mới
    alert("Bạn có tin nhắn mới từ khách hàng!");
});

connection.start().catch(err => console.error(err.toString()));

document.getElementById("sendButton").addEventListener("click", async () => {
    const userId = document.getElementById("userId").value;
    const message = document.getElementById("messageInput").value;

    await connection.invoke("SendMessageToUserFromHost", userId, message);
    document.getElementById("messageInput").value = "";
});
