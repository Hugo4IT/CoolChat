import { batch, Component, createEffect, createResource, createSignal, For, Match, onCleanup, onMount, Show, Switch } from "solid-js";

import styles from "./Main.module.css";
import formStyles from "../Form/Form.module.css";
import globalStyles from "../GlobalStyles.module.css";
import { FaSolidCircleNotch, FaSolidPlus, FaSolidRightFromBracket } from "solid-icons/fa";
import { CreateGroupForm } from "../CreateGroupForm/CreateGroupForm";
import { API_ROOT } from "../Globals";
import { GroupView } from "../GroupView/GroupView";
import { MessageDto } from "../interfaces/MessageDto";
import { GroupDto } from "../interfaces/GroupDto";
import { InviteDto } from "../interfaces/InviteDto";
import { Overlay } from "../Overlay/Overlay";
import { Form } from "../Form/Form";
import { FormTitle } from "../Form/FormTitle";
import { FormButtons } from "../Form/FormButtons";
import { FormButton } from "../Form/FormButton";
import { NotificationsManager } from "../NotificationsManager";
import { AuthenticationManager } from "../AuthenticationManager";
import { RTManager } from "../RTManager";

interface MainProps {
}

export const Main: Component<MainProps> = (props: MainProps) => {
    const [loading, setLoading] = createSignal("nothing");
    const [selectedGroup, setSelectedGroup] = createSignal<number|undefined>(undefined);
    const [lastSelectedGroup, setLastSelectedGroup] = createSignal<number|undefined>(undefined);
    const [view, setView] = createSignal("main");

    const [invitePopup, setInvitePopup] = createSignal<InviteDto|undefined>(undefined);
    const [showInvitePopup, setShowInvitePopup] = createSignal(false);

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
        AuthenticationManager.get().logout();
    };

    const openGroupFunction = (i: number) => {
        if (loading() != "nothing")
            return;

        if (selectedGroup() == i)
            return;
        
        batch(() => {
            setLastSelectedGroup(selectedGroup());
            setSelectedGroup(i);
    
            setLoading("group");
        });
    };

    const createGroupFunction = () => {
        setLoading("createGroup");
    };

    const createGroupExit = async (success: boolean, res: GroupDto|null) => {
        setLoading("main");
    };

    const acceptInviteCallback = async () => {
        setLoading("invite-accept");
        await rt.acceptInvite(invitePopup()!);
        setLoading("nothing");
        setShowInvitePopup(false);
    };
    
    const rejectInviteCallback = async () => {
        setLoading("invite-reject");
        await rt.rejectInvite(invitePopup()!);
        setLoading("nothing");
        setShowInvitePopup(false);
    };

    // const cc = new ChatConnectionsManager();
    const rt = RTManager.get();

    rt.onMessageReceived.push(async (id: number, message: MessageDto) => {
        // Check if the user has this chat open
        if (selectedGroup() == undefined || !rt.groups[selectedGroup()!].channels.map(c => c.chatId).includes(id)) {
            const el = document.createElement("div");
            el.innerHTML = message.content;

            const body = el.innerText;
            const kind = body.includes(`@${localStorage.getItem("username")!}`) ? "ping" : "message";

            NotificationsManager.notify(kind, message.author, body.substring(0, Math.min(300, body.length)));
        }
    });

    rt.onGroupInviteReceived.push(async (invite: InviteDto) => {
        if (view() == "main") {
            setInvitePopup(invite);
            setShowInvitePopup(true);
        }
    });

    onMount(async () => {
        await NotificationsManager.launchServiceWorker();
        NotificationsManager.notify("info", "Testing", "you'll get a few test notifications in a bit");

        setTimeout(async () => {
            await NotificationsManager.notify("ping", "Ping", "Account1 pinged you!");
            await NotificationsManager.notify("message", "Account1", "This is a cool message that you definitely received");
            await NotificationsManager.notify("info", "Info", "I ate peanutbutter today");
            await NotificationsManager.notify("success", "Success", `Logged in as ${localStorage.getItem("username")!}`);
            await NotificationsManager.notify("warning", "Warning", "Area fifty-juan seems to be leaking aliens");
            await NotificationsManager.notify("error", "Error", "Ya dumb");
        }, 2000);
    });

    const groupAnimationShowOld = () => loading() == "group" && lastSelectedGroup() != undefined;
    
    return (
        <Switch>
            <Match when={view() == "main"}>
                <div class={styles.Main}
                    classList={{
                        [styles.Out]: ["createGroup", "logout"].includes(loading()),
                    }}>
                    <div class={styles.Sidebar}>
                        <For each={rt.groups}>{(group, i) => (
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
                            <Match when={rt.groups.length == 0}>

                            </Match>
                            <Match when={selectedGroup() != undefined}>
                                <GroupView group={rt.groups[groupAnimationShowOld() ? lastSelectedGroup()! : selectedGroup()!]!}
                                           lastIndex={lastSelectedGroup()}
                                           index={selectedGroup()!}
                                           out={groupAnimationShowOld()}/>
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
                                <FormButton kind="primary" loading={loading() == "invite-reject"} onClick={rejectInviteCallback}>Reject</FormButton>
                                <FormButton kind="secondary" loading={false} onClick={() => setShowInvitePopup(false)}>Later</FormButton>
                                <FormButton kind="primary" loading={loading() == "invite-accept"} onClick={acceptInviteCallback}>Accept</FormButton>
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