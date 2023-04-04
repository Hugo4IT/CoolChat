import { Accessor, createSignal, For, Match, onCleanup, onMount, Setter, Switch } from "solid-js";

import { FaSolidBomb, FaSolidChartLine, FaSolidComments, FaSolidCommentSlash, FaSolidDiagramProject, FaSolidDollarSign, FaSolidFlag, FaSolidFootball, FaSolidGamepad, FaSolidGear, FaSolidGift, FaSolidHashtag, FaSolidHeart, FaSolidHouse, FaSolidImagePortrait, FaSolidLink, FaSolidLocationCrosshairs, FaSolidPaintbrush, FaSolidPaw, FaSolidPlus, FaSolidQuoteRight, FaSolidRightFromBracket, FaSolidStar, FaSolidTags, FaSolidUser, FaSolidUserPlus, FaSolidUsers } from "solid-icons/fa";
import { Form } from "../Form/Form";
import { FormButton } from "../Form/FormButton";
import { FormButtons } from "../Form/FormButtons";
import { FormTextInput } from "../Form/FormTextInput";
import { GroupDto } from "../interfaces/GroupDto";
import { MessageDto } from "../interfaces/MessageDto";
import { RTManager } from "../RTManager";
import { View, ViewStateManager } from "../ViewStateManager";
import { ChannelView } from "./ChannelView";
import styles from "./GroupView.module.pcss";
import { NewChannelForm } from "./NewChannelForm";
import { PopupView } from "./PopupView";

type OverlayId = "nothing" | "inviteUser";
export type TransitionDirection = "up" | "down" | "none";

export const Icons = (size: number) => [
    <FaSolidHashtag size={size} />,
    <FaSolidComments size={size} />,
    <FaSolidCommentSlash size={size} />,
    <FaSolidLink size={size} />,
    <FaSolidPaw size={size} />,
    <FaSolidHeart size={size} />,
    <FaSolidGift size={size} />,
    <FaSolidDollarSign size={size} />,
    <FaSolidLocationCrosshairs size={size} />,
    <FaSolidHouse size={size} />,
    <FaSolidGamepad size={size} />,
    <FaSolidStar size={size} />,
    <FaSolidBomb size={size} />,
    <FaSolidImagePortrait size={size} />,
    <FaSolidDiagramProject size={size} />,
    <FaSolidChartLine size={size} />,
    <FaSolidFootball size={size} />,
    <FaSolidPaintbrush size={size} />,
    <FaSolidTags size={size} />,
    <FaSolidQuoteRight size={size} />,
];

export class GroupView extends View {
    id = "GroupView";
    
    public override inDelay = (id: string) => {
        if (this.localViewStateManager.current().id == "ChannelView") {
            const channelView = this.localViewStateManager.current() as ChannelView;
            channelView.setTransitionDirection(this.transitionDirection());
            channelView.setTransition("in");
        }

        return id == "EmptyView" ? 300 : 150;
    };

    public override outDelay = (_: string) => {
        if (this.localViewStateManager.current().id == "ChannelView") {
            const channelView = this.localViewStateManager.current() as ChannelView;
            channelView.setTransitionDirection(this.transitionDirection());
            channelView.setTransition("out");
        }
        
        return 150;
    };

    private group: GroupDto;
    private transitionDirection: Accessor<TransitionDirection>;
    public setTransitionDirection: Setter<TransitionDirection>;

    private localViewStateManager: ViewStateManager;

    public override preload = async () => {
        await this.localViewStateManager.current().preload();
    }

    public constructor(group: GroupDto, transitionDirection: TransitionDirection) {
        super();

        this.group = group;
        [this.transitionDirection, this.setTransitionDirection] = createSignal(transitionDirection);

        this.localViewStateManager = new ViewStateManager(new ChannelView(this.group.channels.at(0)!, transitionDirection), `group_${group.id}`);
    }

    view = () => {
        const rt = RTManager.get();

        const [selectedChannel, setSelectedChannel] = createSignal(0);

        const animationClasses = () => ({
            [styles.In]: this.transition() == "in",
            [styles.Out]: this.transition() == "out",
            [styles.None]: this.transitionDirection() == "none",
            [styles.Down]: this.transitionDirection() == "down",
            [styles.Up]: this.transitionDirection() == "up",
        });

        const newChannelButton = () => {
            if (this.localViewStateManager.busy() || ViewStateManager.get().busy())
                return;

            ViewStateManager.get().push(new NewChannelForm(this.group.id));
        };

        const inviteButton = () => {
            const [value, setValue] = createSignal("");
            const [error, setError] = createSignal("");
            const [hasError, setHasError] = createSignal(false);
            const [busy, setBusy] = createSignal(false);

            let ref: HTMLInputElement | undefined;

            const close = async () => {
                await ViewStateManager.get().pop();
            };

            const invite = async () => {
                setBusy(true);

                const response = await rt.sendInvite(this.group.id, value());

                setHasError(!response.isOk());

                if (error())
                    setError(response.getDefaultError()!);
                else
                    ref!.value = "";

                setBusy(false);
            };

            ViewStateManager.get().push(new PopupView((
                <Form>
                    <FormTextInput icon={(<FaSolidUser size={16}/>)}
                                name="add-user"
                                placeholder="CoolGuy123"
                                title="Invite User:"
                                valueCallback={setValue}
                                error={hasError() ? error() : undefined}
                                onSubmit={invite}
                                ref={r => ref = r}/>
                    <FormButtons>
                        <FormButton kind="secondary" loading={false} onClick={close}>Close</FormButton>
                        <FormButton kind="primary" loading={busy()} onClick={invite}>Invite</FormButton>
                    </FormButtons>
                </Form>
            )));
        };

        const onMessageReceived = (id: number, message: MessageDto) => {
            if (!this.group.channels.map(c => c.chatId).includes(id))
                return;
            
            const ping = message.content.includes(`@${localStorage.getItem("username")}`)

            
        };

        onMount(() => {
            rt.onMessageReceived.push(onMessageReceived);
        });

        onCleanup(() => {
            rt.onMessageReceived.splice(rt.onMessageReceived.indexOf(onMessageReceived));
        });

        return (
            <div class={styles.Group}>
                <div class={styles.Sidebar}>
                    <div class={[styles.SidebarContents, styles.Animated].join(' ')}
                        classList={animationClasses()}>
                        <For each={this.group.channels}>{(channel, i) => (
                            <button class={styles.ChannelButton}
                                    classList={{
                                        [styles.Active]: selectedChannel() == i(),
                                    }}>
                                <Switch>
                                    <Match when={channel.icon == 0}>
                                        <FaSolidHashtag size={16} />
                                    </Match>
                                </Switch>
                                {channel.name}
                            </button>
                        )}</For>
                        <button class={[styles.ChannelButton, styles.Dim].join(' ')}
                            onClick={newChannelButton}>
                            <FaSolidPlus size={16} />
                            New Channel
                        </button>
                    </div>
                </div>
                {this.localViewStateManager.view({})}
                <div class={styles.MembersBar}>
                    <div class={[styles.MembersBarContents, styles.Animated].join(' ')}
                        classList={animationClasses()}>
                        <div class={styles.MembersToolbar}>
                            <button class={styles.MembersToolbarButton} onClick={inviteButton}>
                                <FaSolidUserPlus size={20}/>
                            </button>
                            <button class={styles.MembersToolbarButton}>
                                <FaSolidUsers size={20}/>
                            </button>
                            <button class={styles.MembersToolbarButton}>
                                <FaSolidFlag size={20}/>
                            </button>
                            <button class={styles.MembersToolbarButton}>
                                <FaSolidGear size={20}/>
                            </button>
                            <button class={styles.MembersToolbarButton}>
                                <FaSolidRightFromBracket size={20}/>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        );
    };
}