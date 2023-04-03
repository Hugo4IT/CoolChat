import { Component, JSX } from "solid-js";

import styles from "./Form.module.pcss";

interface FormProps {
    children: JSX.Element;
    class?: string;
    classList?: { [className: string]: boolean }
};

export const Form: Component<FormProps> = (props: FormProps) => {
    return (
        <div class={[props.class ?? "", styles.Form].join(' ')} onLoad={event => event.currentTarget.focus()} classList={props.classList}>
            {props.children}
        </div>
    );
};