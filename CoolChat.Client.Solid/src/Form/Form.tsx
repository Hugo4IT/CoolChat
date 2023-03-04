import { Component, JSX } from "solid-js";

import styles from "./Form.module.css";

interface FormProps {
    children: JSX.Element[];
    class?: string,
};

export const Form: Component<FormProps> = (props: FormProps) => {
    return (
        <div class={typeof props.class !== 'undefined' ? `${props.class} ${styles.Form}` : styles.Form}>
            {...props.children}
        </div>
    );
};