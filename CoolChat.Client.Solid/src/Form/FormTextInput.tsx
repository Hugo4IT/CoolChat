import { IconTypes } from "solid-icons";
import { Component, createEffect, JSX, Ref } from "solid-js";

import styles from "./Form.module.css";

interface FormTextInputProps {
    error?: string;
    valueCallback: (value: string) => void,
    icon: JSX.Element;
    placeholder: string;
    kind?: "text"|"password";
    name: string;
    title: string;
}

export const FormTextInput: Component<FormTextInputProps> = (props: FormTextInputProps) => {
    const iconElement = props.icon as SVGSVGElement;
    iconElement.classList.add(styles.FormTextInputIcon);
    
    const kind = props.kind ?? "text";

    let inputRef: HTMLInputElement|undefined;

    const onChange = () => {
        props.valueCallback(inputRef!.value);
    };

    return (
        <div class={styles.FormSection} classList={{[styles.FormSectionError]: typeof props.error !== "undefined"}}>
            <label for={props.name}>{props.title}</label>
            <div class={styles.FormTextInput}>
                <div class={styles.FormTextInputIconContainer}>
                    {iconElement}
                </div>
                <input type={kind} name={props.name} id={props.name} ref={inputRef} onChange={onChange} placeholder={props.placeholder} />
            </div>
            <span class={styles.FormTextInputError}
                    classList={{
                        [styles.FormTextInputErrorHidden]: typeof props.error === "undefined",
                    }}>
                {props.error}
            </span>
        </div>
    );
};