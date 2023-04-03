import { Accessor, batch, createSignal, For, onMount, Setter, Show } from "solid-js";

import { FaSolidCircleNotch, FaSolidPlus, FaSolidRightFromBracket } from "solid-icons/fa";
import { AuthenticationManager } from "../AuthenticationManager";
import { Form } from "../Form/Form";
import formStyles from "../Form/Form.module.pcss";
import { FormButton } from "../Form/FormButton";
import { FormButtons } from "../Form/FormButtons";
import { FormTitle } from "../Form/FormTitle";
import { API_ROOT } from "../Globals";
import globalStyles from "../GlobalStyles.module.pcss";
import { InviteDto } from "../interfaces/InviteDto";
import { MessageDto } from "../interfaces/MessageDto";
import { NotificationsManager } from "../NotificationsManager";
import { RTManager } from "../RTManager";
import { View, ViewStateManager } from "../ViewStateManager";
import { CreateGroupForm } from "./CreateGroupForm";
import { EmptyView } from "./EmptyView";
import { GroupView, TransitionDirection } from "./GroupView";
import styles from "./Main.module.pcss";
import { PopupView } from "./PopupView";
import { LoginForm } from "./LoginForm";

export class Main extends View {
    id = "Main";

    override inDelay = (_: string) => 300;
    override outDelay = (_: string) => 300;

    override preload = async () => {
        await RTManager.get().load();
    };

    private localViewStateManager: ViewStateManager;

    private selectedGroup: Accessor<number | undefined>;
    private setSelectedGroup: Setter<number | undefined>;

    public constructor() {
        super();

        this.localViewStateManager = new ViewStateManager(new EmptyView(), "main");

        [this.selectedGroup, this.setSelectedGroup] = createSignal<number|undefined>(undefined);
    }

    view = () => {
        const rt = RTManager.get();

        const [loading, setLoading] = createSignal("nothing");

        const logoutFunction = () => {
            if (loading() != "nothing")
                return;
            
            setLoading("logout");

            AuthenticationManager.get().logout();
            ViewStateManager.get().switch(new LoginForm());
        };

        const openGroupFunction = (i: number) => {
            if (this.localViewStateManager.busy())
                return;

            if (this.selectedGroup() == i)
                return;
            
            batch(() => {
                let dir: TransitionDirection = "none";

                if (this.selectedGroup() != undefined)
                    dir = this.selectedGroup()! < i ? "up" : "down";

                if (this.localViewStateManager.current().id == "GroupView")
                    (this.localViewStateManager.current() as GroupView).setTransitionDirection(dir);

                this.localViewStateManager.switch(new GroupView(rt.groups[i], dir))
                this.setSelectedGroup(i);
            });
        };

        const createGroupFunction = () => {
            ViewStateManager.get().push(new CreateGroupForm());
        };

        rt.onMessageReceived.push(async (id: number, message: MessageDto) => {
            // Check if the user has this chat open
            if (this.selectedGroup() == undefined || !rt.groups[this.selectedGroup()!].channels.map(c => c.chatId).includes(id)) {
                const el = document.createElement("div");
                el.innerHTML = message.content;

                const body = el.innerText;
                const kind = body.includes(`@${localStorage.getItem("username")!}`) ? "ping" : "message";

                NotificationsManager.notify(kind, message.author, body.substring(0, Math.min(300, body.length)));
            }
        });

        rt.onGroupInviteReceived.push(async (invite: InviteDto) => {
            if (ViewStateManager.get().current() == this) {
                const reject = async () => {
                    await rt.rejectInvite(invite);
                    await ViewStateManager.get().pop();
                };

                const cancel = async () => {
                    await ViewStateManager.get().pop();
                };
                
                const accept = async () => {
                    await rt.acceptInvite(invite);
                    await ViewStateManager.get().pop();
                };

                ViewStateManager.get().push(new PopupView((
                    <Form>
                        <FormTitle>You have been invited to join {invite.groupName}</FormTitle>
                        <img src={`${API_ROOT}/api/Resource/Icon?id=${invite.groupIcon.id}`} class={formStyles.PreviewImage}/>
                        <FormButtons>
                            <FormButton kind="primary" loading={loading() == "invite-reject"} onClick={reject}>Reject</FormButton>
                            <FormButton kind="secondary" loading={false} onClick={cancel}>Later</FormButton>
                            <FormButton kind="primary" loading={loading() == "invite-accept"} onClick={accept}>Accept</FormButton>
                        </FormButtons>
                    </Form>
                )));
            }
        });

        onMount(async () => {
            await NotificationsManager.launchServiceWorker();
        });

        return (
            <div class={styles.Main}
                classList={{
                    [styles.In]: this.transition() == "in",
                    [styles.Out]: this.transition() == "out",
                }}>
                <div class={styles.Sidebar}>
                    <For each={rt.groups}>{(group, i) => (
                        <button class={[styles.GroupButton].join(' ')}
                                onClick={() => openGroupFunction(i())}
                                classList={{[styles.GroupButtonActive]: this.selectedGroup() == i()}}
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

                    <button class={[styles.GroupButton, styles.LogoutButton].join(' ')}
                            onClick={logoutFunction}
                            classList={{[globalStyles.ButtonActive]: loading() == "logout"}}>
                        <Show when={loading() == "logout"} fallback={() => (
                            <FaSolidRightFromBracket size={16} />
                        )}>
                            <FaSolidCircleNotch size={16} class={globalStyles.ButtonSpinner} />
                        </Show>
                    </button>
                </div>
                <div class={styles.Content}>
                    {this.localViewStateManager.view({})}
                </div>
            </div>
        );
    };
}