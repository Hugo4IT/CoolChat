import { Accessor, createSignal, onCleanup, onMount, Setter } from "solid-js";
import { JSX } from "solid-js/jsx-runtime";
import { View, ViewStateManager } from "../ViewStateManager";

import styles from "./PopupView.module.pcss";

export class PopupView extends View {
    id = "PopupView";
    public override isOverlay: boolean = true;

    private content: JSX.Element;

    private visible: Accessor<boolean>;
    private setVisible: Setter<boolean>;
    
    private onClose?: () => void;

    public override inDelay(_: string): number {
        this.setVisible(true);

        return 300;
    }

    public override outDelay(_: string): number {
        this.setVisible(false);
        
        return 300;
    }

    public constructor(content: JSX.Element, onClose?: () => void) {
        super();
        
        this.content = content;
        this.onClose = onClose;

        [this.visible, this.setVisible] = createSignal(false);
    }

    view = () => {
        const tryClose = () => {
            if (this.onClose != undefined)
                this.onClose();
            
            ViewStateManager.get().pop();
        };
    
        const onKeyUp = (event: KeyboardEvent) => {
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