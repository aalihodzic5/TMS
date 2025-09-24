const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("ReceiveNotification", (message, link) => {
    const container = document.getElementById("notifications-container");
    if (!container) return;

    const item = document.createElement("li");
    item.innerHTML = `<a href="${link}">${message}</a>`;
    container.prepend(item);

    console.log("Nova notifikacija:", message, link);
});

connection.start()
    .then(() => console.log("SignalR connected"))
    .catch(err => console.error(err));
