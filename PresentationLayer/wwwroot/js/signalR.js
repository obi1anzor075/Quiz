// signalr.js
import * as signalR from "@microsoft/signalr";

const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/gamehub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

hubConnection.onclose((error) => {
    console.error('Connection closed:', error);
});

// Ёкспортируем hubConnection дл€ использовани€ в других файлах
export default hubConnection;
