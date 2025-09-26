const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

const toastElement = document.getElementById('liveToast');
const toastBody = document.getElementById('LiveToastBody');
let toastBootstrap; // Declare it here so it's accessible after DOMContentLoaded

// Initialize Bootstrap Toast once the DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    if (toastElement) {
        toastBootstrap = new bootstrap.Toast(toastElement);
    }
});


connection.on("ReceiveNotification", function (senderUserId, message, link) { // Note: added senderUserId based on your Hub
    // 1. Display as Toast
    console.log("Notifikacija");
    if (toastBody && toastBootstrap) {
        toastBody.innerHTML = `New notification from ${senderUserId}: <a href="${link}" class="text-white">${message}</a>`;
        toastBootstrap.show();
    } else {
        console.warn("Toast elements not found or not initialized.");
    }

    // 2. Add to Sidebar Notifications List
    const container = document.getElementById("notifications-container");
    if (container) {
        const item = document.createElement("li");
        // For the sidebar, you might want a simpler display, or the full link
        item.classList.add("list-unstyled-item", "mb-1"); // Add some styling
        item.innerHTML = `<a href="${link}" class="text-dark"><strong>${senderUserId}:</strong> ${message}</a>`;
        container.prepend(item); // Add to top
    } else {
        console.warn("#notifications-container not found.");
    }

    console.log("Received Notification:", message, "Link:", link);
});

connection.start()
    .then(() => console.log("SignalR Connected!"))
    .catch(err => console.error("Error connecting to SignalR:", err.toString()));