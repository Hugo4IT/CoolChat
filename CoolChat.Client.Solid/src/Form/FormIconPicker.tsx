import { Component, createSignal, For, JSX } from "solid-js";

import styles from "./Form.module.pcss";

interface FormIconPickerProps {
    error?: string;
    valueCallback: (value: number) => void,
    icon: JSX.Element;
    default: number;
    name: string;
    title: string;
    icons: JSX.Element[];
}

export const FormIconPicker: Component<FormIconPickerProps> = (props: FormIconPickerProps) => {
    const [value, setValue] = createSignal(props.default);

    const iconElement = props.icon as SVGSVGElement;
    iconElement.classList.add(styles.FormTextInputIcon);

    const iconClicked = (index: number) => {
        setValue(index);
        props.valueCallback(value());
    };

    return (
        <div class={styles.FormSection} classList={{[styles.FormSectionError]: typeof props.error !== "undefined"}}>
            <label for={props.name}>{props.title}</label>
            <div class={styles.FormTextInput}>
                <div class={styles.FormTextInputIconContainer}>
                    {iconElement}
                </div>
                <div class={styles.IconPickerList}>
                    <For each={props.icons}>{(element, i) =>
                        <div class={styles.IconPickerItem}
                            classList={{
                                [styles.Active]: value() == i(),
                            }}
                            onClick={() => iconClicked(i())}>
                            {element}
                        </div>
                    }</For>
                </div>
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