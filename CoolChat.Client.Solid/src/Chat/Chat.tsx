import { HubConnectionBuilder } from "@microsoft/signalr";
import { FaSolidPaperPlane } from "solid-icons/fa";
import { Component, createResource, For, onCleanup, onMount } from "solid-js";
import { API_ROOT } from "../Globals";
import { GetMessagesResponse } from "../interfaces/GetMessagesResponse";
import { getToken } from "../JwtHelper";
import { Message } from "../Message/Message";

import styles from "./Chat.module.css";

const fetchMessages = async (args: {id: number, start: number, count: number}) =>
    await (await fetch(`${API_ROOT}/api/Chat/GetMessages?id=${args.id}&start=${args.start}&count=${args.count}`, {
        headers: { "Authorization": "Bearer " + await getToken() }
    })).json() as GetMessagesResponse;

interface ChatProps {
    id: number;
}

export const Chat: Component<ChatProps> = (props: ChatProps) => {
    const username = localStorage.getItem("username")!;
    const [messages, { mutate }] = createResource(() => ({id: props.id, start: 0, count: 50}), fetchMessages);
    
    const pushMessage = (message: { author: string, content: string, date: Date }) => {
        mutate(oldMessages => {
            const copy = oldMessages!.items;

            // Update second-last message for message grouping
            if (copy.length > 0)
                copy[copy.length - 1] = {
                    author: copy[copy.length - 1].author,
                    content: copy[copy.length - 1].content,
                    date: copy[copy.length - 1].date,
                };

            copy.push({ author: message.author, content: message.content, date: message.date });
            return { items: copy };
        });
    };


    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5010/signalr/chathub", { accessTokenFactory: () => localStorage.getItem("jwt")! })
        .build();
    connection.on("ReceiveMessage", (id: number, author: string, content: string, date: Date) => {
        pushMessage({ author: author, content: content, date: date });
    });
    connection.start();

    let chatInputRef: HTMLTextAreaElement|undefined;

    const chatInput = () => {
        const value = chatInputRef!.value;
        const lines = value.split("\n").length;

        chatInputRef!.rows = Math.min(5, lines);
    };

    const chatKeyDown = async (keyEvent: KeyboardEvent) => {
        if (!keyEvent.shiftKey && keyEvent.key == "Enter") {
            keyEvent.preventDefault();

            const value = chatInputRef!.value;
            
            if (value.trim().length == 0)
                return;
            
            chatInputRef!.value = "";
            chatInputRef!.rows = 1;

            connection.send("SendMessage", props.id, value);
            
            // await fetch(`${API_ROOT}/api/Chat/AddMessage?id=${props.id}`, {
            //     headers: { "Authorization": "Bearer " + await getToken() },
            //     method: "post",
            //     body: value,
            // })
            //     .then(res => res.json())
            //     .then(res => {
            //         // pushMessage(res);
            //     })
            //     .catch(res => console.error(res));
        }
    };

    return (
        <div class={styles.Chat}>
            <div class={styles.ChatMessages}>
                <div class={styles.ScrolledRect}>
                    <For each={messages()?.items ?? []}>{(message, i) => {
                        const hideAuthor = i() > 0 && messages()!.items[i() - 1].author == message.author;
                        const hideDate = i() < messages()!.items.length - 1 && messages()!.items[i() + 1].author == message.author;

                        return (
                            <Message author={hideAuthor ? undefined : message.author}
                                     content={message.content}
                                     date={hideDate ? undefined : new Date(message.date)}
                                     sent={message.author == username}/>
                        );
                    }}</For>
                </div>
            </div>
            <div class={styles.ChatInput}>
                <textarea ref={chatInputRef} placeholder="Say something funny" onInput={chatInput} rows={1} onkeydown={chatKeyDown}></textarea>
                <FaSolidPaperPlane size={16} />
            </div>
        </div>
    );
};