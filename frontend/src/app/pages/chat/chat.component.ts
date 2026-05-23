import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface ChatMessage {
  id: string;
  sender: string;
  content: string;
  timestamp: Date;
  isMine: boolean;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent implements OnInit {
  currentRoom = 'لجنة تقييم بحوث المكامن';
  messages: ChatMessage[] = [];
  newMessage = '';

  ngOnInit(): void {
    // Mock history load from ChatHub
    this.messages = [
      {
        id: '1',
        sender: 'د. علي حسين',
        content: 'السلام عليكم، هل اطلعتم على المرفق الخاص بالبحث الجديد؟',
        timestamp: new Date(Date.now() - 3600000),
        isMine: false
      },
      {
        id: '2',
        sender: 'أنت',
        content: 'وعليكم السلام، نعم اطلعت عليه وسأقوم برفع التقييم اليوم.',
        timestamp: new Date(Date.now() - 1800000),
        isMine: true
      }
    ];
  }

  sendMessage() {
    if (!this.newMessage.trim()) return;

    this.messages.push({
      id: Date.now().toString(),
      sender: 'أنت',
      content: this.newMessage,
      timestamp: new Date(),
      isMine: true
    });
    
    // TODO: Connect to ChatHub.invoke('SendMessage', ...)
    this.newMessage = '';
  }
}
