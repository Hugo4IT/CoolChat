import { FaSolidPaperPlane } from "solid-icons/fa";
import { Component, createResource, For, onMount } from "solid-js";
import { ChatConnectionsManager } from "../ChatConnectionsManager";
import { MessageModel } from "../interfaces/MessageModel";
import { Message } from "../Message/Message";

import styles from "./Chat.module.css";

interface ChatProps {
    id: number;
    cc: ChatConnectionsManager;
    onSendMessage?: (id: number, message: string) => void;
}

export const Chat: Component<ChatProps> = (props: ChatProps) => {
    const username = localStorage.getItem("username")!;
    const [messages, { mutate }] = createResource(() => props.id, props.cc.getMessages);

    let chatInputRef: HTMLTextAreaElement|undefined;
    let scrolledRectRef: HTMLDivElement|undefined;

    const pushMessage = (id: number, message: MessageModel) => {
        if (id != props.id)
            return;
        
        mutate(messages => {
            const toUpdate = [];

            // Update last for message grouping
            if (messages!.length > 0) {
                const lastMessage = messages!.pop()!;

                toUpdate.push({
                    author: lastMessage.author,
                    content: lastMessage.content,
                    date: lastMessage.date
                });
            }

            toUpdate.push({ author: message.author, content: message.content, date: message.date });

            return [...messages!, ...toUpdate];
        });

        if (scrolledRectRef!.scrollTop >= scrolledRectRef!.getBoundingClientRect().height - 10)
            scrolledRectRef!.scrollTop = scrolledRectRef!.scrollHeight;
    };

    onMount(() => {
        props.cc.onMessageReceived.push(pushMessage);

        setTimeout(() => {
            scrolledRectRef!.scrollTop = 9999;
        }, 300);
    });

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

            await props.cc.sendMessage(props.id, value);
        }
    };

    return (
        <div class={styles.Chat}>
            <div class={styles.ChatMessages} ref={scrolledRectRef}>
                <div class={styles.ScrolledRect}>
                    <For each={messages() ?? []}>{(message, i) => {
                        const hideAuthor = i() > 0 && messages()![i() - 1].author == message.author;
                        const hideDate = i() < messages()!.length - 1 && messages()![i() + 1].author == message.author;

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