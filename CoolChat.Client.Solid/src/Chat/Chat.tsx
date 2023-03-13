import { EmojiPicker } from "solid-emoji-picker";
import { FaSolidCirclePlus, FaSolidFaceSmile } from "solid-icons/fa";
import { Accessor, Component, createEffect, createResource, createSignal, For, onMount, Show, Suspense } from "solid-js";
import { ChatConnectionsManager } from "../ChatConnectionsManager";
import { MessageModel } from "../interfaces/MessageModel";
import { Message } from "../Message/Message";

import styles from "./Chat.module.css";

interface ChatProps {
    id: Accessor<number>;
    cc: ChatConnectionsManager;
    onSendMessage?: (id: number, message: string) => void;
}

export const Chat: Component<ChatProps> = (props: ChatProps) => {
    const username = localStorage.getItem("username")!;
    const [messages, { refetch, mutate }] = createResource(props.id, props.cc.getMessages);

    const [emojiPickerVisible, setEmojiPickerVisible] = createSignal(false);

    let chatInputRef: HTMLTextAreaElement|undefined;
    let scrolledRectRef: HTMLDivElement|undefined;
    let emojiPickerButton: HTMLDivElement|undefined;

    // Scroll to bottom of chat on open
    createEffect(() => {
        props.id();
        scrolledRectRef!.style.scrollBehavior = "initial";
        setTimeout(() => {
            scrolledRectRef!.scrollTop = 999999;
            scrolledRectRef!.style.scrollBehavior = "smooth";
        }, 150);
    });
    
    const pushMessage = (id: number, message: MessageModel) => {
        if (id != props.id())
            return;
        
        refetch();
        mutate(m => new Array(m![0]!));

        window.requestAnimationFrame(() => window.requestAnimationFrame(() => {
            if (scrolledRectRef!.scrollTop >= scrolledRectRef!.getBoundingClientRect().height - 10)
                scrolledRectRef!.scrollTop = 999999999;
        }));
    };

    onMount(() => {
        props.cc.onMessageReceived.push(pushMessage);

        window.requestAnimationFrame(() => {
            scrolledRectRef!.scrollTop = 999999;
        });
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

            props.cc.sendMessage(props.id(), value);
        }
    };

    const toggleEmojiPicker = () => {
        setEmojiPickerVisible(!emojiPickerVisible());
    };

    return (
        <>
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
                <div class={styles.ChatInput} ref={emojiPickerButton} >
                    <textarea ref={chatInputRef} placeholder="Say something funny" onInput={chatInput} rows={1} onkeydown={chatKeyDown}></textarea>
                    <FaSolidFaceSmile size={20} class={styles.ChatInputIcon} onClick={toggleEmojiPicker}/>
                    <FaSolidCirclePlus size={20} class={styles.ChatInputIcon} />
                </div>

            </div>
            <Show when={emojiPickerVisible()}>
                <Suspense>
                    <div class={styles.EmojiPicker} style={{
                        "--e-pos-x": emojiPickerButton!.getBoundingClientRect().left + "px",
                        "--e-pos-y": emojiPickerButton!.getBoundingClientRect().top + "px",
                    }}>
                        <EmojiPicker />
                    </div>
                </Suspense>
            </Show>
        </>
    );
};