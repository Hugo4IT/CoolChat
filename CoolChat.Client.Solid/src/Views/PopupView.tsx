import { Accessor, createSignal, onCleanup, onMount, Setter } from "solid-js";
import { JSX } from "solid-js/jsx-runtime";
import { View, ViewStateManager } from "../ViewStateManager";

import styles from "./PopupView.module.pcss";

export class PopupView extends View {
    id = "PopupView";
    public override isOverlay: boolean = true;

    private content: JSX.Element;
    
    private onClose?: () => void;

    public override inDelay = (_: string) => 300;
    public override outDelay = (_: string) => 300;

    public constructor(content: JSX.Element, onClose?: () => void) {
        super();
        
        this.content = content;
        this.onClose = onClose;
    }

    view = () => {
        const tryClose = () => {
            if (ViewStateManager.get().busy())
                return;

            window.removeEventListener("keyup", onKeyUp);

            if (this.onClose != undefined)
                this.onClose();
            
            ViewStateManager.get().pop();
        };
    
        const onKeyUp = (event: KeyboardEvent) => {
            event.preventDefault();
    
            if (event.key == "Escape")
                tryClose();
        };

        window.addEventListener("keyup", onKeyUp);

        return (
            <div class={styles.OverlayArea} classList={{
                [styles.In]: this.transition() == "in",
                [styles.Out]: this.transition() == "out",
            }} onClick={tryClose}>
                <div class={styles.OverlayContainer} onClick={e => e.stopPropagation()}>
                    <div class={styles.Overlay}>
                        {this.content}
                    </div>
                </div>
            </div>
        );
    };
}