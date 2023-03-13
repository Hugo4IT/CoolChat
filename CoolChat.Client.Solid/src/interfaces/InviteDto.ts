import { Resource } from "./Resource";

export interface InviteDto {
    inviteId: number;
    groupId: number;
    groupName: string;
    memberCount: number;
    groupIcon: Resource;
    senderName: string;
    senderId: number;
}