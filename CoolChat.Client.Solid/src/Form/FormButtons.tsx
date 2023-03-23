import { Component } from "solid-js";
import { JSX } from "solid-js/jsx-runtime";

import styles from "./Form.module.pcss";

interface FormButtonsProps {
    children: JSX.Element;
}

export const FormButtons: Component<FormButtonsProps> = (props: FormButtonsProps) => {

    return (
        <div class={styles.FormButtons}>
            {props.children}
        </div>
    )
};