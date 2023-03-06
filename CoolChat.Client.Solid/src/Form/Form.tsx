import { Component, JSX } from "solid-js";

import styles from "./Form.module.css";

interface FormProps {
    children: JSX.Element[];
    class?: string,
};

export const Form: Component<FormProps> = (props: FormProps) => {
    return (
        <div class={[props.class ?? "", styles.Form].join(' ')}>
            {...props.children}
        </div>
    );
};