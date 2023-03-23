import hljs from "highlight.js";

import { Component, onMount, Show } from "solid-js";
import { AuthenticationManager } from "../AuthenticationManager";
import { DateFormatter } from "../DateFormatter";

import styles from "./Message.module.pcss";

interface MessageProps {
    author: string;
    content: string;
    date: Date;
    sent: boolean;
    grouped: boolean;
}

export const Message: Component<MessageProps> = (props: MessageProps) => {
    let dateString = DateFormatter.Format(props.date);
    let content: HTMLDivElement|undefined;

    onMount(() => {
        hljs.configure({ })
        content!.querySelectorAll("pre code").forEach(el => hljs.highlightElement(el as HTMLElement));
    })

    return (
        <div class={styles.MessageContainer}
             classList={{
                [styles.Sent]: props.sent,
                [styles.Grouped]: props.grouped,
                [styles.Ping]: props.content.includes(`@${AuthenticationManager.get().username()}`),
            }}>
            <Show when={!props.grouped}>
                <div class={styles.MessageHeader}>
                    <span class={styles.Author}>{props.author}</span>
                    <span class={styles.Date}>{dateString}</span>
                </div>
            </Show>
            <div class={styles.Message} innerHTML={props.content} ref={content}>
            </div>
        </div>
    );
};