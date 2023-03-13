import { Component, JSX, onMount } from "solid-js";

import styles from "./Form.module.css";

interface FormProps {
    children: JSX.Element;
    class?: string,
};

export const Form: Component<FormProps> = (props: FormProps) => {
    let container: HTMLDivElement|undefined;

    onMount(() => {
        container!.querySelector("input")?.focus();
    });
    
    return (
        <div class={[props.class ?? "", styles.Form].join(' ')} ref={container}>
            {props.children}
        </div>
    );
};