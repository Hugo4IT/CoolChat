import { Accessor, Component, createSignal, For, Setter, Show } from "solid-js";

export type TransitionState = "in" | "out" | "none";

export abstract class View {
    public abstract id: string;
    public isOverlay: boolean = false;

    public transition: Accessor<TransitionState>;
    public setTransition: Setter<TransitionState>;

    public constructor() {
        [this.transition, this.setTransition] = createSignal("none");
    }
    
    public inDelay(id: string): number { return 0; }
    public outDelay(id: string): number { return 0; }
    public async preload(): Promise<void> { return Promise.resolve(); }
    
    public abstract view: Component;
}

type State = "hidden" | "animating" | "visible";

class ViewState {
    public view: View;
    public state: Accessor<State>;
    public setState: Setter<State>;

    public constructor(view: View) {
        this.view = view;

        [this.state, this.setState] = createSignal("visible");
    }
}

export class ViewStateManager {
    private static instances: Map<string, ViewStateManager> = new Map();

    private viewStates: Accessor<ViewState[]>;
    private setViewStates: Setter<ViewState[]>;
    
    private isInTransition: boolean;

    public constructor(base: View, id: string = "@@Global") {
        ViewStateManager.instances.set(id, this);
        
        [this.viewStates, this.setViewStates] = createSignal([new ViewState(base)]);

        this.isInTransition = false;
    }

    public pop = async () => {
        if (this.viewStates().length == 1)
            return;
        
        this.isInTransition = true;
        
        const lastViewState = this.viewStates().at(-1)!;
        const outDelay = lastViewState.view.outDelay(this.viewStates().at(-2)!.view.id);

        lastViewState.setState("animating");
        lastViewState.view.setTransition("out");
        
        await this.delay(outDelay);
        
        lastViewState.setState("hidden");
        lastViewState.view.setTransition("none");
        this.popViewState();

        if (!lastViewState.view.isOverlay) {
            const viewState = this.viewStates().at(-1)!;
            const inDelay = viewState.view.inDelay(lastViewState.view.id);

            viewState.setState("animating");
            viewState.view.setTransition("in");
            
            await this.delay(inDelay);

            viewState.setState("visible");
            viewState.view.setTransition("none");
        }

        this.isInTransition = false;
    };

    public push = async (view: View) => {
        this.isInTransition = true;

        if (!view.isOverlay) {
            const lastViewState = this.viewStates().at(-1)!;
            const outDelay = lastViewState.view.outDelay(view.id);
            lastViewState.setState("animating");
            lastViewState.view.setTransition("out");
            
            await Promise.all([
                this.delay(outDelay),
                view.preload(),
            ]);
            
            lastViewState.setState("hidden");
            lastViewState.view.setTransition("none");
        }

        const viewState = new ViewState(view);
        this.pushViewState(viewState);

        viewState.setState("animating");
        viewState.view.setTransition("in");

        const inDelay = view.inDelay(this.viewStates().at(-2)!.view.id);
    
        await this.delay(inDelay);

        viewState.setState("visible");
        viewState.view.setTransition("none");

        this.isInTransition = false;
    };

    public switch = async (view: View) => {
        if (this.viewStates().length == 0)
            return;

        this.isInTransition = true;
        
        const oldViewState = this.viewStates().at(-1)!;

        oldViewState.setState("animating");
        oldViewState.view.setTransition("out");
        
        const outDelay = oldViewState.view.outDelay(view.id);
        
        await Promise.all([
            this.delay(outDelay),
            view.preload(),
        ]);
        
        oldViewState.setState("hidden");
        oldViewState.view.setTransition("none");

        this.popViewState();

        const viewState = new ViewState(view);
        viewState.setState("animating");
        viewState.view.setTransition("in");
        
        this.pushViewState(viewState);

        const inDelay = view.inDelay(oldViewState.view.id);
        await this.delay(inDelay);

        viewState.setState("visible");
        viewState.view.setTransition("none");

        this.isInTransition = false;
    };

    public clear = async (view: View) => {
        if (this.viewStates().length == 0 || view.isOverlay)
            return;
        
        this.isInTransition = true;
        
        const oldViewState = this.viewStates().at(0)!;

        oldViewState.setState("animating");
        oldViewState.view.setTransition("out");
        
        const outDelay = oldViewState.view.outDelay(view.id);
        await this.delay(outDelay);

        oldViewState.setState("hidden");
        oldViewState.view.setTransition("none");

        this.clearViewStates();

        const viewState = new ViewState(view);
        viewState.setState("animating");
        viewState.view.setTransition("in");
        
        this.pushViewState(viewState);

        const inDelay = view.inDelay(oldViewState.view.id);
        await this.delay(inDelay);

        viewState.setState("visible");
        viewState.view.setTransition("none");

        this.isInTransition = false;
    };

    public current = () => this.viewStates().at(-1)!.view;
    public busy = () => this.isInTransition;

    public view: Component = () => {
        return (
            <For each={this.viewStates()}>{s => (
                <Show when={s.state() != "hidden"}>
                    {s.view.view({})}
                </Show>
            )}</For>
        );
    };

    private clearViewStates = () => {
        this.setViewStates([]);
    };

    private popViewState = () => {
        const viewState = this.viewStates().at(-1)!;
        this.setViewStates(this.viewStates().slice(0, this.viewStates().length - 1));
        return viewState;
    };

    private pushViewState = (viewState: ViewState) => {
        this.setViewStates([...this.viewStates(), viewState]);
    };

    private delay = async (ms: number) => {
        if (ms == 0)
            return;
        
        await new Promise(res => setTimeout(res, ms));
    }

    public static get = (id: string = "@@Global") => ViewStateManager.instances.get(id)!;
}