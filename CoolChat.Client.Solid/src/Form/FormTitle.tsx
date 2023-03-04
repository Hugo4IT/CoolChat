import { Component } from "solid-js";
import { JSX } from "solid-js/jsx-runtime";

import styles from "./Form.module.css";

interface FormTitleProps {
    children: JSX.Element;
}

export const FormTitle: Component<FormTitleProps> = (props: FormTitleProps) => {
    return (
        <span class={styles.FormTitle}>{props.children}</span>
    )
};