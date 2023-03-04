import { FaSolidCircleNotch } from "solid-icons/fa";
import { Component, Show } from "solid-js";
import { JSX } from "solid-js/jsx-runtime";

import styles from "./Form.module.css";
import globalStyles from "../GlobalStyles.module.css";

interface FormButtonProps {
    kind: "primary"|"secondary";
    children: JSX.Element;
    loading: boolean,
    onClick: () => void,
}

export const FormButton: Component<FormButtonProps> = (props: FormButtonProps) => {
    return (
        <button class={[globalStyles.Button].join(' ')}
                onClick={() => props.onClick()}
                classList={{
                    [globalStyles.ButtonActive]: props.loading,
                    [styles.SecondaryButton]: props.kind === "secondary",
                }}>
            <Show when={props.loading} fallback={props.children}>
                <FaSolidCircleNotch size={16} class={globalStyles.ButtonSpinner} />
            </Show>
        </button>
    );
};