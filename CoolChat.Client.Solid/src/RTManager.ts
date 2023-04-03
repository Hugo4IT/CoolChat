import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { createStore, SetStoreFunction } from "solid-js/store";
import { AuthenticationManager } from "./AuthenticationManager";
import { API_ROOT } from "./Globals";
import { GroupDto } from "./interfaces/GroupDto";
import { InviteDto } from "./interfaces/InviteDto";
import { MessageDto } from "./interfaces/MessageDto";
import { IndexRange, LazyLoadedArray } from "./LazyLoadedArray";
import { ValidationResponse } from "./ValidationResponse";

export class RTManager {
    private static instance: RTManager;

    public groups: GroupDto[];
    private setGroups: SetStoreFunction<GroupDto[]>;

    public invites: InviteDto[];
    private setInvites: SetStoreFunction<InviteDto[]>;

    private connection!: HubConnection;
    private chatCache: Map<number, LazyLoadedArray<MessageDto>>;

    public onMessageReceived: ((id: number, message: MessageDto) => void|PromiseLike<void>)[] = [];
    public onGroupInviteReceived: ((invite: InviteDto) => void|PromiseLike<void>)[] = [];
    public onGroupJoined: ((group: GroupDto) => void|PromiseLike<void>)[] = [];

    public constructor() {
        RTManager.instance = this;

        window.addEventListener("close", () => {
            this.connection.stop();
        });

        [this.groups, this.setGroups] = createStore<GroupDto[]>([]);
        [this.invites, this.setInvites] = createStore<InviteDto[]>([]);

        this.chatCache = new Map();
    }

    public load = async () => {
        this.connection = new HubConnectionBuilder()
            .withUrl(`${API_ROOT}/signalr/chathub`, {
                accessTokenFactory: async () => (await AuthenticationManager.get().getToken())!,
            })
            .build();
        
        this.connection.on("ReceiveMessage", (id: number, author: string, content: string, date: Date) => {
            const message: MessageDto = { author: author, content: content, date: date };
            
            this.chatCache.get(id)!.pushBack(message);

            for (const callback of this.onMessageReceived)
                callback(id, message);
        });

        this.connection.on("ReceiveGroupInvite", (invite: InviteDto) => {
            this.pushInvite(invite);

            for (const callback of this.onGroupInviteReceived)
                callback(invite);
        });

        this.connection.on("GroupJoined", async (group: GroupDto) => {
            await this.pushGroup(group);

            for (const callback of this.onGroupJoined)
                callback(group);
        });

        await this.connection.start();
        
        this.setGroups(await this.fetchGroups() ?? []);
        this.setInvites(await this.fetchInvites() ?? []);
        
        // Precache chats
        await this.precacheAllChats();
    };

    public unload = async () => {
        await this.connection.stop();
        this.setGroups([]);
        this.setInvites([]);
        this.chatCache.clear();
    };

    public pushGroup = async (group: GroupDto) => {
        this.setGroups([...this.groups, group]);
        await this.precacheAllChats();
    };

    public pushInvite = (invite: InviteDto) => {
        this.setInvites([...this.invites, invite]);
    };

    public getMessages = async (id: number, start: number, count: number) => {
        if (!this.chatCache.has(id)) {
            this.chatCache.set(id, this.newMessageLoaderFor(id));
        }

        return await this.chatCache.get(id)!.getRange(new IndexRange(start, start + count));
    };

    public sendMessage = (id: number, message: string) =>
        this.connection.send("SendMessage", id, message);

    public sendInvite = async (groupId: number, username: string) =>
        new ValidationResponse(await this.connection.invoke("CreateInvite", groupId, username));

    public acceptInvite = async (invite: InviteDto) =>
        new ValidationResponse(await this.connection.invoke("AcceptInvite", invite.inviteId));

    public rejectInvite = async (invite: InviteDto) =>
        new ValidationResponse(await this.connection.invoke("RejectInvite", invite.inviteId));
    
    public trySubscribe = async (group: GroupDto) =>
        new ValidationResponse(await this.connection.invoke("TrySubscribeToGroup", group.id));

    private precacheAllChats = async () => {
        await Promise.all(this.groups.flatMap(g => g.channels.map(async c => await this.getMessages(c.chatId, 0, 50))));
    };

    private newMessageLoaderFor = (id: number) => 
        new LazyLoadedArray(async (range) => await this.fetchMessages(id, range.start, range.count()) ?? []);

    private fetchMessages = async (id: number, start: number, count: number) =>
        this.authorizedFetch<MessageDto[]>(`/api/Chat/GetMessages?id=${id}&start=${start}&count=${count}`);

    private fetchGroups = async () => this.authorizedFetch<GroupDto[]>("/api/Group/MyGroups");

    private fetchInvites = async () => this.authorizedFetch<InviteDto[]>("/api/Group/MyInvites");

    private authorizedFetch = async<T> (url: string) => {
        const response = await fetch(API_ROOT + url, await AuthenticationManager.authorize()).catch(error => {
            console.error(error);
        });

        if (!response)
            return undefined;
        
        const cast: T = await response.json();
        return cast;
    };

    public static get = () => RTManager.instance;
}