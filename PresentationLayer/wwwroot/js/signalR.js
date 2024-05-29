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

// ������������ hubConnection ��� ������������� � ������ ������
export default hubConnection;
