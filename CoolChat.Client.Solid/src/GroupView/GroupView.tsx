import { Component, createResource, createSignal, For, Match, Show, Switch } from "solid-js";
import { Chat } from "../Chat/Chat";
import { API_ROOT } from "../Globals";

import styles from "./GroupView.module.css";
import { getToken } from "../JwtHelper";
import { FaSolidHashtag, FaSolidPlus } from "solid-icons/fa";
import { ChatConnectionsManager } from "../ChatConnectionsManager";
import { GroupModel } from "../interfaces/GroupModel";

interface GroupViewProps {
    group: GroupModel;
    index: number;
    lastIndex: number|undefined;
    out: boolean;
    cc: ChatConnectionsManager;
}

export const GroupView: Component<GroupViewProps> = (props: GroupViewProps) => {
    const [selectedChannel, setSelectedChannel] = createSignal(0);

    const getSelectedChannelChat = () => props.group.channels[selectedChannel()]!.chatId;

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
        </div>
    );
};