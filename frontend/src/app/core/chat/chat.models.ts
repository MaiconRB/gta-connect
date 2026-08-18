export interface ConversationSummary {
  conversationId: string;
  otherProfileId: string;
  otherDisplayName: string;
  otherAvatarPath: string | null;
  lastMessageContent: string;
  lastMessageAtUtc: string;
  unreadCount: number;
}

export interface ChatMessage {
  id: string;
  conversationId: string;
  senderProfileId: string;
  content: string;
  sentAtUtc: string;
}
