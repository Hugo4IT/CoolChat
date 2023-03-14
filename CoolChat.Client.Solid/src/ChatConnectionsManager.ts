import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { API_ROOT } from "./Globals";
import { GetMessagesResponse } from "./interfaces/GetMessagesResponse";
import { GroupDto } from "./interfaces/GroupDto";
import { InviteDto } from "./interfaces/InviteDto";
import { MessageModel } from "./interfaces/MessageModel";
import { Resource } from "./interfaces/Resource";
import { getToken } from "./JwtHelper";

const fetchMessages = async (id: number, start: number, count: number) =>
    await (await fetch(`${API_ROOT}/api/Chat/GetMessages?id=${id}&start=${start}&count=${count}`, {
        headers: { "Authorization": "Bearer " + await getToken() }
    })).json() as GetMessagesResponse;

export class ChatConnectionsManager {
    private connection: HubConnection;
    private chatCache: Map<number, MessageModel[]>;

    public onMessageReceived: ((id: number, message: MessageModel) => void|PromiseLike<void>)[] = [];
    public onGroupInviteReceived: ((invite: InviteDto) => void|PromiseLike<void>)[] = [];
    public onGroupJoined: ((group: GroupDto) => void|PromiseLike<void>)[] = [];

    public constructor() {
        this.connection = new HubConnectionBuilder()
            .withUrl("http://localhost:5010/signalr/chathub", { accessTokenFactory: () => localStorage.getItem("jwt")! })
            .build();
        
        this.connection.on("ReceiveMessage", (id: number, author: string, content: string, date: Date) => {
            this.messageReceived(id, { author: author, content: content, date: date });
        });

        this.connection.on("ReceiveGroupInvite", (invite: InviteDto) => {
            for (const callback of this.onGroupInviteReceived)
                callback(invite);
        });

        this.connection.on("GroupJoined", (group: GroupDto) => {
            for (const callback of this.onGroupJoined)
                callback(group);
        });

        this.chatCache = new Map();
    }

    public start = async () => {
        await this.connection.start();
    }
    
    public stop = async () => {
        await this.connection.stop();

        this.chatCache.clear();
        this.onMessageReceived = [];
    }

    public sendMessage = (id: number, message: string) => {
        this.connection.send("SendMessage", id, message);
    }

    public sendInvite = async (groupId: number, username: string) => {
        return await this.connection.invoke("CreateInvite", groupId, username);
    }

    public acceptInvite = (invite: InviteDto) => {
        this.connection.send("AcceptInvite", invite.inviteId);
    };

    public rejectInvite = (invite: InviteDto) => {
        this.connection.send("RejectInvite", invite.inviteId);
    };

    public getMessages = async (id: number) => {
        if (!this.chatCache.has(id)) {
            this.chatCache.set(id, (await fetchMessages(id, 0, 50)).items);
        }

        return this.chatCache.get(id)!;
    }

    private messageReceived = (id: number, message: MessageModel) => {
        this.chatCache.get(id)?.push(message);

        for (const callback of this.onMessageReceived)
            callback(id, message);
    }
}