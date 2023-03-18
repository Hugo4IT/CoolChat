import { FaSolidBell, FaSolidCheck, FaSolidCircleXmark, FaSolidInfo, FaSolidMessage, FaSolidTriangleExclamation } from "solid-icons/fa";
import { Accessor, Component, createSignal, For, JSX, Setter, Show } from "solid-js";
import { Portal } from "solid-js/web";
import { AuthenticationManager } from "./AuthenticationManager";
import { API_ROOT } from "./Globals";
import { INotification, NotificationKind } from "./interfaces/INotification";

import styles from "./NotificationsManager.module.css";

const iconSize = 20;

const defaultIcons: { [kind: string]: () => JSX.Element } = {
    "ping": () => (<FaSolidBell size={iconSize} />),
    "message": () => (<FaSolidMessage size={iconSize} />),
    "info": () => (<FaSolidInfo size={iconSize} />),
    "success": () => (<FaSolidCheck size={iconSize} />),
    "warning": () => (<FaSolidTriangleExclamation size={iconSize} />),
    "error": () => (<FaSolidCircleXmark size={iconSize} />),
};

export class NotificationsManager {
    private static instance: NotificationsManager;

    public notifications: Accessor<INotification[]>;
    private setNotifications: Setter<INotification[]>;

    public constructor() {
        NotificationsManager.instance = this;

        [this.notifications, this.setNotifications] = createSignal<INotification[]>([]);
    }

    public launchServiceWorker = async () => {
        if ("serviceWorker" in navigator) {
            try {
                const registration = await navigator.serviceWorker.register("src/assets/sw.js", { scope: "src/assets/" });

                let subscription = await registration.pushManager.getSubscription();

                if (!subscription) {
                    subscription = await registration.pushManager.subscribe({ userVisibleOnly: true });
                }

                await fetch(`${API_ROOT}/api/Account/SubscribeWebPush`, {
                    method: "post",
                    headers: {
                        "Content-Type": "application/json",
                        ...(await AuthenticationManager.authorize()).headers,
                    },
                    body: JSON.stringify({
                        endpoint: subscription.endpoint,
                        key_p256dh: btoa(String.fromCharCode(...new Uint8Array(subscription.getKey("p256dh")!))),
                        key_auth: btoa(String.fromCharCode(...new Uint8Array(subscription.getKey("auth")!))),
                    }),
                });
            } catch (error) {
                console.error(error);
            }
        }
    };

    private notify = (notification: INotification) => {
        const notifications = this.notifications();
        this.setNotifications([]);
        this.setNotifications([...notifications, notification]);

        setTimeout(() => {
            notification.setOut(true);

            setTimeout(() => {
                if (this.notifications().includes(notification)) {
                    this.setNotifications(this.notifications().filter(v => v != notification));
                }
            }, 300);
        }, 2700);
    };

    public view: Component = () => {
        return (
            <Portal>
                <div class={styles.NotificationsContainer}>
                    <For each={this.notifications()}>{(notification, i) => (
                        <div class={[styles.NotificationContainer, `NOTI_${notification.kind}`].join(' ')}
                             classList={{
                                 [styles.Move]: i() != this.notifications().length - 1,
                                 [styles.In]: i() == this.notifications().length - 1,
                                 [styles.Out]: notification.out(),
                             }}>
                            {notification.icon ?? defaultIcons[notification.kind]}
                            <div class={styles.NotificationContent}>
                                <span class={styles.NotificationTitle}>{notification.title}</span>
                                <span class={styles.NotificationBody}>{notification.body}</span>
                            </div>
                        </div>
                    )}</For>
                </div>
            </Portal>
        );
    };

    public static notify = async (kind: NotificationKind, title: string, body: string, icon: string|undefined = undefined) => {
        const [out, setOut] = createSignal(false);

        NotificationsManager.instance.notify({ kind, title, body, icon, out, setOut });

        await new Promise(resolve => setTimeout(resolve, 300));
    }

    public static launchServiceWorker = async () => await NotificationsManager.instance.launchServiceWorker();
}