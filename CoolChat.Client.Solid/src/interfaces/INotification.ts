import { Accessor, Setter } from "solid-js";
import { JSX } from "solid-js/jsx-runtime";

export declare type NotificationKind = "error"|"warning"|"success"|"info"|"message"|"ping";

export interface INotification {
    title: string;
    body: string;
    icon?: JSX.Element;
    kind: NotificationKind;

    out: Accessor<boolean>;
    setOut: Setter<boolean>;
}