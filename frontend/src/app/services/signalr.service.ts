import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

export interface ChatMessage {
  senderId: string;
  content: string;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private notificationHubConnection!: signalR.HubConnection;
  private chatHubConnection!: signalR.HubConnection;

  // SignalR message and notification streams
  public notifications$ = new Subject<{ title: string; message: string; relatedEntityId?: string }>();
  public chatMessages$ = new Subject<ChatMessage>();
  
  // Real-time status update for research papers
  public paperStatusUpdates$ = new Subject<{ paperId: string; newState: string }>();

  public isConnected = signal<boolean>(false);

  constructor() {}

  public startConnections(token: string) {
    // 1. Notification Hub
    this.notificationHubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7139/notificationHub', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.notificationHubConnection.start()
      .then(() => {
        this.isConnected.set(true);
        console.log('Connected to Notification Hub');
      })
      .catch(err => console.error('Error connecting to Notification Hub:', err));

    this.notificationHubConnection.on('ReceiveNotification', (title: string, message: string, relatedEntityId?: string) => {
      this.notifications$.next({ title, message, relatedEntityId });
    });

    this.notificationHubConnection.on('PaperStateChanged', (paperId: string, newState: string) => {
      this.paperStatusUpdates$.next({ paperId, newState });
    });

    // 2. Chat Hub
    this.chatHubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7139/chatHub', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.chatHubConnection.start()
      .then(() => console.log('Connected to Chat Hub'))
      .catch(err => console.error('Error connecting to Chat Hub:', err));

    this.chatHubConnection.on('ReceiveMessage', (senderId: string, content: string) => {
      this.chatMessages$.next({ senderId, content });
    });
  }

  public joinChatChannel(channelId: string) {
    if (this.chatHubConnection && this.chatHubConnection.state === signalR.HubConnectionState.Connected) {
      this.chatHubConnection.invoke('JoinChannel', channelId)
        .catch(err => console.error('Error joining chat channel:', err));
    }
  }

  public leaveChatChannel(channelId: string) {
    if (this.chatHubConnection && this.chatHubConnection.state === signalR.HubConnectionState.Connected) {
      this.chatHubConnection.invoke('LeaveChannel', channelId)
        .catch(err => console.error('Error leaving chat channel:', err));
    }
  }

  public sendMessage(channelId: string, receiverId: string | null, content: string) {
    if (this.chatHubConnection && this.chatHubConnection.state === signalR.HubConnectionState.Connected) {
      return this.chatHubConnection.invoke('SendMessage', channelId, receiverId, content);
    }
    return Promise.reject('Chat connection not established.');
  }

  public stopConnections() {
    if (this.notificationHubConnection) {
      this.notificationHubConnection.stop();
    }
    if (this.chatHubConnection) {
      this.chatHubConnection.stop();
    }
    this.isConnected.set(false);
  }
}
