import { FaSolidCircleNotch } from "solid-icons/fa";
import { Accessor, createSignal, Setter } from "solid-js";
import { View } from "../ViewStateManager";

export class LoadingScreen extends View {
    id = "LoadingScreen";
    isOverlay = false;

    private loadText: Accessor<string>;
    public setLoadText: Setter<string>;

    public constructor() {
        super();

        [this.loadText, this.setLoadText] = createSignal("");
    }

    inDelay = (id: string) => 0;
    outDelay = (id: string) => 0;

    view = () => {
        return (
            <>
                <FaSolidCircleNotch size={24} class="spinner" style={{"color": "inherit"}} />
                <span style={{"color": "inherit"}}>{this.loadText()}</span>
            </>
        );
    };
}