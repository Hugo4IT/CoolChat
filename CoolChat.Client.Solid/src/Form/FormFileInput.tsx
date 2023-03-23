import {Component, createSignal, JSX, Show} from "solid-js";

import styles from "./Form.module.pcss";

interface FormFileInputProps {
    error?: string;
    valueCallback: (value: FileList|null) => void,
    icon: JSX.Element;
    placeholder: string;
    name: string;
    title: string;
}

export const FormFileInput: Component<FormFileInputProps> = (props: FormFileInputProps) => {
    const iconElement = props.icon as SVGSVGElement;
    iconElement.classList.add(styles.FormTextInputIcon);

    let inputRef: HTMLInputElement|undefined;

    const [value, setValue] = createSignal<FileList|null>(null);
    const [textValue, setTextValue] = createSignal("");

    const onChange = () => {
        setValue(inputRef!.files);
        props.valueCallback(value());

        if (value() && value()!.length > 0) {
            setTextValue(value()!.item(0)!.name);
        } else {
            setTextValue("");
        }
    };

    return (
        <div class={styles.FormSection} classList={{[styles.FormSectionError]: typeof props.error !== "undefined"}}>
            <label for={props.name}>{props.title}</label>
            <div class={styles.FormTextInput}>
                <div class={styles.FormTextInputIconContainer}>
                    {iconElement}
                </div>
                <input type="file"
                       name={props.name}
                       id={props.name}
                       ref={inputRef}
                       onChange={onChange}
                       style={{ display: "none" }}
                       accept="image/*"/>
                <label class={styles.FileInputLabel}
                       for={props.name}
                       classList={{
                           [styles.Unset]: textValue() == "",
                       }}>
                    {textValue() != "" ? textValue() : props.placeholder}

                </label>
            </div>
            <span class={styles.FormTextInputError}
                    classList={{
                        [styles.FormTextInputErrorHidden]: typeof props.error === "undefined",
                    }}>
                {props.error}
            </span>
            <Show when={value() && value()!.length > 0}>
                <img src={URL.createObjectURL(value()!.item(0)!)} class={styles.PreviewImage} />
            </Show>
        </div>
    );
};