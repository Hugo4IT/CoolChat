import { createStore } from "solid-js/store";
import { EmojiPicker } from "solid-emoji-picker";
import { FaSolidCirclePlus, FaSolidFaceSmile } from "solid-icons/fa";
import { Accessor, Component, createEffect, createSignal, For, onCleanup, onMount, Show, Suspense } from "solid-js";
import { MessageDto } from "../interfaces/MessageDto";
import { Message } from "../Message/Message";

import styles from "./Chat.module.css";
import { AuthenticationManager } from "../AuthenticationManager";
import { RTManager } from "../RTManager";

interface ChatProps {
    id: Accessor<number>;
    onSendMessage?: (id: number, message: string) => void;
}

export const Chat: Component<ChatProps> = (props: ChatProps) => {
    const username = AuthenticationManager.get().username();

    const [messages, setMessages] = createStore<MessageDto[]>([]);

    const [emojiPickerVisible, setEmojiPickerVisible] = createSignal(false);

    const rt = RTManager.get();

    let chatInputRef: HTMLTextAreaElement|undefined;
    let scrolledRectRef: HTMLDivElement|undefined;
    let emojiPickerButton: HTMLDivElement|undefined;

    // Scroll to bottom of chat on open
    createEffect(async () => {
        const id = props.id();

        setMessages([]);

        const m = await rt.getMessages(id, 0, 100);

        if (m == messages)
            return;
        
        setMessages(m);

        scrolledRectRef!.scrollTop = 999999;
    });
    
    const pushMessage = async (id: number, message: MessageDto) => {
        if (id != props.id())
            return;
        
        const height = scrolledRectRef!.scrollHeight - scrolledRectRef!.getBoundingClientRect().height;

        setMessages([...messages, message]);

        window.requestAnimationFrame(() => window.requestAnimationFrame(() => {
            if (scrolledRectRef!.scrollTop >= height - 50)
                scrolledRectRef!.scrollTop = 999999999;
        }));
    };

    onMount(() => {
        rt.onMessageReceived.push(pushMessage);

        window.requestAnimationFrame(() => {
            scrolledRectRef!.scrollTop = 999999;
        });
    });

    onCleanup(() => {
        rt.onMessageReceived.splice(rt.onMessageReceived.indexOf(pushMessage));
    })

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

            rt.sendMessage(props.id(), value);
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
                        <For each={messages}>{(message, i) => {
                            const grouped = i() > 0 && messages[i() - 1].author == message.author;

                            return (
                                <Message author={message.author}
                                         content={message.content}
                                         date={new Date(message.date)}
                                         sent={message.author == username}
                                         grouped={grouped} />
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