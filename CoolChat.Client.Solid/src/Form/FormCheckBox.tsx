import { Component, createSignal, onMount } from "solid-js";

import styles from "./Form.module.pcss";

interface FormCheckBoxProps {
    name: string;
    valueCallback: (value: boolean) => void;
    text: string;
    default: boolean;
}

export const FormCheckBox: Component<FormCheckBoxProps> = (props: FormCheckBoxProps) => {
    const [value, setValue] = createSignal(props.default);
    
    let checkboxRef: HTMLInputElement|undefined;
    
    const onChange = () => {
        setValue(checkboxRef!.checked);
        props.valueCallback(value());
    };

    onMount(() => {
        if (checkboxRef!.checked != props.default)
            checkboxRef!.click();
    });

    return (
        <div class={styles.FormSection}>
            <label for={props.name} class={styles.FormCheckBoxContainer} classList={{[styles.Checked]: value()}}>
                <input type="checkbox"
                       onChange={onChange}
                       ref={checkboxRef}
                       name={props.name}
                       id={props.name}
                       class={styles.FormCheckBox}/>
                <div class={styles.FormFakeCheckBox}></div>
                <span class={styles.FormCheckBoxText}>{props.text}</span>
            </label>
        </div>
    )
};