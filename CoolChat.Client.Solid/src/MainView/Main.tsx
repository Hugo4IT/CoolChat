import { Component, createEffect, createResource, createSignal, For, Match, onCleanup, onMount, Show, Switch } from "solid-js";

import styles from "./Main.module.css";
import formStyles from "../Form/Form.module.css";
import globalStyles from "../GlobalStyles.module.css";
import { FaSolidCircleNotch, FaSolidPlus, FaSolidRightFromBracket } from "solid-icons/fa";
import { getToken, logout } from "../JwtHelper";
import { CreateGroupForm } from "../CreateGroupForm/CreateGroupForm";
import { API_ROOT } from "../Globals";
import { GroupView } from "../GroupView/GroupView";
import { MessageModel } from "../interfaces/MessageModel";
import { ChatConnectionsManager } from "../ChatConnectionsManager";
import { GroupDto } from "../interfaces/GroupDto";
import { InviteDto } from "../interfaces/InviteDto";
import { Overlay } from "../Overlay/Overlay";
import { Form } from "../Form/Form";
import { FormTitle } from "../Form/FormTitle";
import { FormButtons } from "../Form/FormButtons";
import { FormButton } from "../Form/FormButton";

interface MainProps {
    logoutCallback: () => void;
}

const fetchGroups = async () =>
    (await (await fetch(`${API_ROOT}/api/Group/MyGroups`, {
        headers: { "Authorization": "Bearer " + await getToken() }
    })).json()) as GroupDto[];

const fetchInvites = async () =>
    (await (await fetch(`${API_ROOT}/api/Group/MyInvites`, {
        headers: { "Authorization": "Bearer " + await getToken() }
    })).json()) as InviteDto[];

export const Main: Component<MainProps> = (props: MainProps) => {
    const [loading, setLoading] = createSignal("nothing");
    const [selectedGroup, setSelectedGroup] = createSignal<number|undefined>(undefined);
    const [lastSelectedGroup, setLastSelectedGroup] = createSignal<number|undefined>(undefined);
    const [view, setView] = createSignal("main");

    const [invitePopup, setInvitePopup] = createSignal<InviteDto|undefined>(undefined);
    const [showInvitePopup, setShowInvitePopup] = createSignal(false);

    const [groups, { mutate: mutateGroups, refetch: refetchGroups }] = createResource(fetchGroups);
    const [invites, { mutate: mutateInvites, refetch: refetchInvites }] = createResource(fetchInvites);

    createEffect(() => {
        // Animations
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

    const createGroupExit = async (success: boolean, res: GroupDto|null) => {
        if (success)
            mutateGroups([...groups()!, res!]);
        
        setLoading("main");
    };

    const cc = new ChatConnectionsManager();

    cc.onMessageReceived.push(async (id: number, message: MessageModel) => {
        
    });

    cc.onGroupInviteReceived.push(async (invite: InviteDto) => {
        console.log("Recieved invite: ", invite);

        mutateInvites([...invites()!, invite]);

        if (view() == "main") {
            setInvitePopup(invite);
            setShowInvitePopup(true);
        }
    });

    cc.onGroupJoined.push(async (group: GroupDto) => {
        mutateGroups([...groups()!, group]);
    });

    onMount(cc.start);
    onCleanup(cc.stop);

    const groupAnimationShowOld = () => loading() == "group" && lastSelectedGroup() != undefined;
    
    return (
        <Switch>
            <Match when={view() == "main"}>
                <div class={styles.Main}
                    classList={{
                        [styles.Out]: ["createGroup", "logout"].includes(loading()),
                    }}>
                    <div class={styles.Sidebar}>
                        <For each={groups() ?? []}>{(group, i) => (
                            <button class={[styles.GroupButton].join(' ')}
                                    onClick={() => openGroupFunction(i())}
                                    classList={{[styles.GroupButtonActive]: selectedGroup() == i()}}
                                    style={{
                                        "background-image": `url(${API_ROOT}/api/Resource/Icon?id=${group.icon.id})`,
                                    }}>
                                <div class={styles.GroupButtonTooltip}>
                                    {group.title}
                                </div>
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
                            <Match when={groups() == undefined || groups()!.length == 0}>

                            </Match>
                            <Match when={selectedGroup() != undefined}>
                                <GroupView group={groups()![groupAnimationShowOld() ? lastSelectedGroup()! : selectedGroup()!]!}
                                           lastIndex={lastSelectedGroup()}
                                           index={selectedGroup()!}
                                           out={groupAnimationShowOld()}
                                           cc={cc}/>
                            </Match>
                        </Switch>
                    </div>
                </div>

                <Overlay visible={() => showInvitePopup()} onClose={() => setShowInvitePopup(false)}>
                    <Show when={invitePopup() != undefined}>
                        <Form>
                            <FormTitle>You have been invited to join {invitePopup()!.groupName}</FormTitle>
                            <img src={`${API_ROOT}/api/Resource/Icon?id=${invitePopup()!.groupIcon.id}`} class={formStyles.PreviewImage}/>
                            <FormButtons>
                                <FormButton kind="primary" loading={false} onClick={() => { cc.rejectInvite(invitePopup()!); setShowInvitePopup(false); }}>Reject</FormButton>
                                <FormButton kind="secondary" loading={false} onClick={() => setShowInvitePopup(false)}>Later</FormButton>
                                <FormButton kind="primary" loading={false} onClick={() => { cc.acceptInvite(invitePopup()!); setShowInvitePopup(false); }}>Accept</FormButton>
                            </FormButtons>
                        </Form>
                    </Show>
                </Overlay>
            </Match>
            <Match when={view() == "createGroup"}>
                <CreateGroupForm exitCallback={createGroupExit} />
            </Match>
        </Switch>
    );
};