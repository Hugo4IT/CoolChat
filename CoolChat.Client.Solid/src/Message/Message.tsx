import hljs from "highlight.js";
// import "highlight.js/scss/base16/";

import { Component, onMount, Show } from "solid-js";

import styles from "./Message.module.css";

interface MessageProps {
    author?: string;
    content: string;
    date?: Date;
    sent: boolean;
}

export const Message: Component<MessageProps> = (props: MessageProps) => {
    const hideAuthor = props.author == undefined;
    const hideDate = props.date == undefined;

    let dateString = undefined;
    if (!hideDate) {
        const now = new Date(Date.now());
        const today = props.date!.getUTCDay() == now.getUTCDay() &&
                      props.date!.getUTCMonth() == now.getUTCMonth() &&
                      props.date!.getUTCFullYear() == now.getUTCFullYear();
    
        dateString = today ? props.date!.toLocaleTimeString()
                           : `${props.date!.toLocaleDateString()} ${props.date!.toLocaleTimeString()}`;
    }

    let content: HTMLDivElement|undefined;

    onMount(() => {
        hljs.configure({ })
        content!.querySelectorAll("pre code").forEach(el => hljs.highlightElement(el as HTMLElement));
    })

    return (
        <div class={styles.MessageContainer}
             classList={{
                [styles.Sent]: props.sent,
                [styles.AuthorHidden]: hideAuthor,
                [styles.DateHidden]: hideDate,
                [styles.Ping]: props.content.includes(`@${localStorage.getItem("username")}`),
            }}>
            <Show when={!hideAuthor}>
                <span class={styles.Author}>{props.author}</span>
            </Show>
            <div class={styles.Message} innerHTML={props.content} ref={content}>
            </div>
            <Show when={!hideDate}>
                <span class={styles.Date}>{dateString}</span>
            </Show>
        </div>
    );
};