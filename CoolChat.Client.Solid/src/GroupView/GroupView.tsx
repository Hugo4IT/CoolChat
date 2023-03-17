import { Component, createSignal, For, Match, onCleanup, onMount, Switch } from "solid-js";
import { Chat } from "../Chat/Chat";

import styles from "./GroupView.module.css";
import { FaSolidFlag, FaSolidGear, FaSolidHashtag, FaSolidPlus, FaSolidRightFromBracket, FaSolidUser, FaSolidUserPlus, FaSolidUsers } from "solid-icons/fa";
import { ChatConnectionsManager } from "../ChatConnectionsManager";
import { GroupDto } from "../interfaces/GroupDto";
import { Overlay } from "../Overlay/Overlay";
import { FormTextInput } from "../Form/FormTextInput";
import { Form } from "../Form/Form";
import { FormButtons } from "../Form/FormButtons";
import { FormButton } from "../Form/FormButton";
import { MessageModel } from "../interfaces/MessageModel";

declare type OverlayId = "nothing"|"inviteUser";

interface GroupViewProps {
    group: GroupDto;
    index: number;
    lastIndex: number|undefined;
    out: boolean;
    cc: ChatConnectionsManager;
}

export const GroupView: Component<GroupViewProps> = (props: GroupViewProps) => {
    const [selectedChannel, setSelectedChannel] = createSignal(0);

    const getSelectedChannelChat = () => props.group.channels[selectedChannel()]!.chatId;

    const [overlay, setOverlay] = createSignal<OverlayId>("nothing");
    const [loading, setLoading] = createSignal("nothing");

    const [invitePopupValue, setInvitePopupValue] = createSignal("");
    const [invitePopupError, setInvitePopupError] = createSignal("");
    const [invitePopupHasError, setInvitePopupHasError] = createSignal(false);

    let invitePopupInputRef: HTMLInputElement|undefined;

    const trySetOverlay = (value: OverlayId) => {
        if (overlay() == "nothing" && loading() == "nothing") {
            setOverlay(value);
            setInvitePopupHasError(false);
        }
    }

    const inviteFunction = async () => {
        setLoading("invite");

        const { success, error } = await props.cc.sendInvite(props.group.id, invitePopupValue());

        setInvitePopupHasError(!success);
        setInvitePopupError(error);

        if (success)
            invitePopupInputRef!.value = "";

        setLoading("nothing");
    };

    const onMessageReceived = (id: number, message: MessageModel) => {
        if (!props.group.channels.map(c => c.chatId).includes(id))
            return;
        
        const ping = message.content.includes(`@${localStorage.getItem("username")}`)

        
    };

    onMount(() => {
        props.cc.onMessageReceived.push(onMessageReceived);
    });

    onCleanup(() => {
        props.cc.onMessageReceived.splice(props.cc.onMessageReceived.indexOf(onMessageReceived));
    });

    return (
        <div class={styles.Group}>
            <div class={styles.Sidebar}>
                <div class={[styles.SidebarContents, styles.Animated].join(' ')}
                     classList={{
                        [styles.Out]: props.out,
                        [styles.First]: props.lastIndex == undefined,
                        [styles.Flip]: props.lastIndex != undefined && props.lastIndex > props.index,
                     }}>
                    <For each={props.group.channels}>{(channel, i) => (
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
                    <button class={[styles.ChannelButton, styles.Dim].join(' ')}>
                        <FaSolidPlus size={16} />
                        New Channel
                    </button>
                </div>
            </div>
            <div class={[styles.ChatContainer, styles.Animated].join(' ')}
                 classList={{
                    [styles.Out]: props.out,
                    [styles.First]: props.lastIndex == undefined,
                    [styles.Flip]: props.lastIndex != undefined && props.lastIndex > props.index,
                 }}>
                <Chat id={getSelectedChannelChat}
                      cc={props.cc} />
            </div>
            <div class={styles.MembersBar}>
                <div class={[styles.MembersBarContents, styles.Animated].join(' ')}
                     classList={{
                        [styles.Out]: props.out,
                        [styles.First]: props.lastIndex == undefined,
                        [styles.Flip]: props.lastIndex != undefined && props.lastIndex > props.index,
                     }}>
                    <div class={styles.MembersToolbar}>
                        <button class={styles.MembersToolbarButton} onClick={() => trySetOverlay("inviteUser")}>
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

            <Overlay visible={() => overlay() == "inviteUser"} onClose={() => setOverlay("nothing")}>
                <Form>
                    <FormTextInput icon={(<FaSolidUser size={16}/>)}
                                   name="add-user"
                                   placeholder="CoolGuy123"
                                   title="Invite User:"
                                   valueCallback={setInvitePopupValue}
                                   error={invitePopupHasError() ? invitePopupError() : undefined}
                                   onSubmit={inviteFunction}
                                   ref={ref => invitePopupInputRef = ref}/>
                    <FormButtons>
                        <FormButton kind="secondary" loading={false} onClick={() => setOverlay("nothing")}>Cancel</FormButton>
                        <FormButton kind="primary" loading={loading() == "invite"} onClick={inviteFunction}>Invite</FormButton>
                    </FormButtons>
                </Form>
            </Overlay>
        </div>
    );
};