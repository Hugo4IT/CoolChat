import { Component, createEffect, createSignal, onCleanup, onMount } from "solid-js";
import { JSX } from "solid-js/jsx-runtime";

import styles from "./Overlay.module.pcss"

interface OverlayProps {
    children: JSX.Element;
    visible: () => boolean;
    onClose?: () => void;
}

export const Overlay: Component<OverlayProps> = (props: OverlayProps) => {
    const [hasBeenVisible, setHasBeenVisible] = createSignal(props.visible());
    const [isAnimating, setIsAnimating] = createSignal(false);

    let container: HTMLDivElement|undefined;

    createEffect(() => {
        if (props.visible()) {
            setHasBeenVisible(true);
            setIsAnimating(true);

            window.requestAnimationFrame(() => {
                container!.querySelector("input")?.focus();
            })
        } else {
            setIsAnimating(true);
            setTimeout(() => {
                setIsAnimating(false);
            }, 300);
        }
    });

    const tryClose = () => {
        if (props.onClose != undefined)
            props.onClose();
    };

    const onKeyUp = (event: KeyboardEvent) => {        
        if (!props.visible())
            return;

        event.preventDefault();

        if (event.key == "Escape")
            tryClose();
    };

    onMount(() => {
        window.addEventListener("keyup", onKeyUp);
    });

    onCleanup(() => {
        window.removeEventListener("keyup", onKeyUp);
    });
    
    return (
        <div class={styles.OverlayArea} classList={{
            [styles.Animating]: hasBeenVisible() && isAnimating(),
            [styles.Appear]: hasBeenVisible() &&  props.visible(),
            [styles.Out]: hasBeenVisible() && !props.visible(),
        }} onClick={tryClose}>
            <div class={styles.OverlayContainer} onClick={e => e.stopPropagation()}>
                <div class={styles.Overlay} ref={container}>
                    {props.children}
                </div>
            </div>
        </div>
    );
}