import { Component, createResource, createSignal, For, Match, Show, Switch } from "solid-js";
import { Chat } from "../Chat/Chat";
import { API_ROOT } from "../Globals";

import styles from "./GroupView.module.css";
import { GetChannelsResponse } from "../interfaces/GetChannelsResponse";
import { getToken } from "../JwtHelper";
import { FaSolidHashtag, FaSolidPlus } from "solid-icons/fa";

interface GroupViewProps {
    id: number;
    index: number;
    lastIndex: number|undefined;
    out: boolean;
}

const fetchChannels = async (id: number) => (await (await fetch(`${API_ROOT}/api/Group/GetChannels?id=${id}`, {
    headers: { "Authorization": "Bearer " + await getToken() }})).json()) as GetChannelsResponse;

export const GroupView: Component<GroupViewProps> = (props: GroupViewProps) => {
    const [channels] = createResource(props.id, fetchChannels);

    const [selectedChannel, setSelectedChannel] = createSignal(0);

    return (
        <div class={styles.Group}>
            <div class={styles.Sidebar}>
                <div class={[styles.SidebarContents, styles.Animated].join(' ')}
                     classList={{
                        [styles.Out]: props.out,
                        [styles.First]: props.lastIndex == undefined,
                        [styles.Flip]: props.lastIndex != undefined && props.lastIndex > props.index,
                     }}>
                    <For each={channels()?.items ?? []}>{(channel, i) => (
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
                <Show when={channels() != undefined}>
                    <Chat id={channels()!.items[selectedChannel()]!.chatId}/>
                </Show>
            </div>
        </div>
    );
};