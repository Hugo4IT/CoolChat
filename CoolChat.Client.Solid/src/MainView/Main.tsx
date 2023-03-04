import { Component, createEffect, createResource, createSignal, For, Match, Show, Switch } from "solid-js";
import { createStore } from "solid-js/store";
import { Chat } from "../Chat/Chat";

import styles from "./Main.module.css";
import globalStyles from "../GlobalStyles.module.css";
import { FaSolidCircleNotch, FaSolidPlus, FaSolidRightFromBracket } from "solid-icons/fa";
import { getToken, logout } from "../JwtHelper";
import { CreateGroupForm } from "../CreateGroupForm/CreateGroupForm";
import { GroupCreateResponse } from "../interfaces/GroupCreateResponse";
import { MyGroupsResponse } from "../interfaces/MyGroupsResponse";
import { Resource } from "../interfaces/Resource";
import { API_ROOT } from "../Globals";
import { GroupView } from "../GroupView/GroupView";

interface MainProps {
    logoutCallback: () => void;
}

const fetchGroups = async () =>
    (await (await fetch(`${API_ROOT}/api/Group/MyGroups`, {
        headers: { "Authorization": "Bearer " + await getToken() }
    })).json()) as MyGroupsResponse;

export const Main: Component<MainProps> = (props: MainProps) => {
    const [loading, setLoading] = createSignal("nothing");
    const [selectedGroup, setSelectedGroup] = createSignal<number|undefined>(undefined);
    const [lastSelectedGroup, setLastSelectedGroup] = createSignal<number|undefined>(undefined);
    const [view, setView] = createSignal("main");
    const [groups, { mutate, refetch }] = createResource(fetchGroups);

    createEffect(() => {
        if (loading() == "main") {
            setTimeout(() => {
                setView("main");
                setLoading("nothing");
            }, 300);
        } else if (loading() == "createGroup") {
            setTimeout(() => {
                setView("createGroup")
                setLoading("nothing")
            }, 300);
        } else if (loading() == "group") {
            setTimeout(() => {
                setLoading("nothing")
            }, 150);
        }
    })

    const logoutFunction = () => {
        if (loading() != "nothing")
            return;
        
        setLoading("logout");
        logout();
        props.logoutCallback();
    };

    const openGroupFunction = (i: number) => {
        if (loading() != "nothing")
            return;

        if (selectedGroup() == i)
            return;
        
        setLastSelectedGroup(selectedGroup());
        setSelectedGroup(i);

        setLoading("group");
    };

    const createGroupFunction = () => {
        setLoading("createGroup");
    };

    const createGroupExit = (success: boolean, res: GroupCreateResponse|null) => {
        refetch();
        setLoading("main");
    };
    
    return (
        <Switch>
            <Match when={view() == "main"}>
                <div class={styles.Main}
                    classList={{
                        [styles.Out]: ["createGroup", "logout"].includes(loading()),
                    }}>
                    <div class={styles.Sidebar}>
                        <For each={groups()?.items ?? []}>{(group, i) => (
                            <button class={[styles.GroupButton].join(' ')}
                                    onClick={() => openGroupFunction(i())}
                                    classList={{[styles.GroupButtonActive]: selectedGroup() == i()}}
                                    style={{
                                        "background-image": `url(${API_ROOT}/api/Resource/Icon?id=${group.icon.id})`,
                                    }}>
                                {/* <div class={styles.GroupButtonTooltip}>
                                    {group.title}
                                </div> */}
                            </button>
                        )}</For>

                        <button class={[styles.GroupButton, styles.AddButton].join(' ')}
                                onClick={createGroupFunction}
                                style={{"background-color": "var(--button-background-color)"}}>
                            <FaSolidPlus size={16} />

                            <div class={styles.GroupButtonTooltip}>
                                Create Group
                            </div>
                        </button>

                        <button class={[globalStyles.Button, styles.LogoutButton].join(' ')}
                                onClick={logoutFunction}
                                classList={{[globalStyles.ButtonActive]: loading() == "logout"}}>
                            <Show when={loading() == "logout"} fallback={() => (
                                <>
                                    <FaSolidRightFromBracket size={16} />
                                    Logout
                                </>
                            )}>
                                <FaSolidCircleNotch size={16} class={globalStyles.ButtonSpinner} />
                            </Show>
                        </button>
                    </div>
                    <div class={styles.Content}>
                        <Switch>
                            <Match when={groups() == undefined || groups()!.items.length == 0}>

                            </Match>
                            <Match when={selectedGroup() != undefined}>
                                <Switch>
                                    <Match when={loading() == "group" && lastSelectedGroup() != undefined}>
                                        <GroupView id={groups()!.items[lastSelectedGroup()!]!.id} index={selectedGroup()!} lastIndex={lastSelectedGroup()} out={true} />
                                    </Match>
                                    <Match when={loading() != "group"}>
                                        <GroupView id={groups()!.items[selectedGroup()!]!.id} index={selectedGroup()!} lastIndex={lastSelectedGroup()} out={false} />
                                    </Match>
                                </Switch>
                            </Match>
                        </Switch>
                    </div>
                </div>
            </Match>
            <Match when={view() == "createGroup"}>
                <CreateGroupForm exitCallback={createGroupExit} />
            </Match>
        </Switch>
    );
};