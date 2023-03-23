import { Component, JSX } from "solid-js";

import styles from "./Form.module.pcss";

interface FormTextInputProps {
    error?: string;
    valueCallback: (value: string) => void,
    onSubmit?: (value: string) => void,
    icon: JSX.Element;
    placeholder: string;
    kind?: "text"|"password";
    name: string;
    title: string;
    ref?: (ref: HTMLInputElement) => void;
}

export const FormTextInput: Component<FormTextInputProps> = (props: FormTextInputProps) => {
    const iconElement = props.icon as SVGSVGElement;
    iconElement.classList.add(styles.FormTextInputIcon);
    
    const kind = props.kind ?? "text";

    let inputRef: HTMLInputElement|undefined;

    // onMount(() => {
    //     window.requestAnimationFrame(() => {
    //         if (props.ref != undefined)
    //             props.ref.value = inputRef;
    //     });
    // })

    const onInput = () => {
        props.valueCallback(inputRef!.value);
    };

    const onKeyDown = (event: KeyboardEvent) => {
        if (event.key == "Enter" && props.onSubmit != undefined) {
            event.preventDefault();
            event.stopPropagation();
            props.onSubmit(inputRef!.value);
        }
    };

    return (
        <div class={styles.FormSection} classList={{[styles.FormSectionError]: typeof props.error !== "undefined"}}>
            <label for={props.name}>{props.title}</label>
            <div class={styles.FormTextInput}>
                <div class={styles.FormTextInputIconContainer}>
                    {iconElement}
                </div>
                <input type={kind}
                       name={props.name}
                       id={props.name}
                       ref={ref => { inputRef = ref; (props.ref ?? (() => {}))(ref); }}
                       onInput={onInput}
                       placeholder={props.placeholder}
                       onKeyDown={onKeyDown}/>
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