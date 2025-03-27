const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.on("ReceiveMessageFromHost", (hostName, message) => {
    const chatBox = document.getElementById("chatBox");
    const newMessage = document.createElement("div");
    newMessage.classList.add("message", "host-message");
    newMessage.textContent = `${hostName}: ${message}`;
    chatBox.appendChild(newMessage);
});

connection.on("NewMessageNotification", () => {
    // Hiển thị thông báo tin nhắn mới
    alert("Bạn có tin nhắn mới từ host!");
});

connection.start().catch(err => console.error(err.toString()));

document.getElementById("sendButton").addEventListener("click", async () => {
    const message = document.getElementById("messageInput").value;
    const tourId = document.getElementById("tourId").value;

    await connection.invoke("SendMessageToHostDirectly", tourId, message);
    document.getElementById("messageInput").value = "";
});
