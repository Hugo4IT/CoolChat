import { Accessor, createSignal, Setter } from "solid-js";
import { Chat } from "../Chat/Chat";
import { ChannelDto } from "../interfaces/ChannelDto";
import { RTManager } from "../RTManager";
import { View } from "../ViewStateManager";

import styles from "./GroupView.module.pcss";
import { TransitionDirection } from "./GroupView";

export class ChannelView extends View {
    id = "ChannelView";

    public override inDelay = (_: string) => 150;
    public override outDelay = (_: string) => 150;

    private channel: ChannelDto;

    private transitionDirection: Accessor<TransitionDirection>;
    public setTransitionDirection: Setter<TransitionDirection>;

    public override preload = async () => {
        await RTManager.get().getMessages(this.channel.chatId, 0, 50);
    };

    public constructor(channel: ChannelDto, transitionDirection: TransitionDirection) {
        super();

        this.channel = channel;
        
        [this.transitionDirection, this.setTransitionDirection] = createSignal(transitionDirection);
    }

    view = () => {
        return (
            <div class={[styles.ChatContainer, styles.Animated].join(' ')}
                classList={{
                    [styles.In]: this.transition() == "in",
                    [styles.Out]: this.transition() == "out",
                    [styles.None]: this.transitionDirection() == "none",
                    [styles.Down]: this.transitionDirection() == "down",
                    [styles.Up]: this.transitionDirection() == "up",
                }}>
                <Chat id={this.channel.chatId} />
            </div>
        )
    };
}