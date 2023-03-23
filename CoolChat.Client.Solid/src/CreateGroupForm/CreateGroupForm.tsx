import { Component, createSignal } from "solid-js";

import styles from './CreateGroupForm.module.pcss';
import { Form } from "../Form/Form";
import { FormTextInput } from "../Form/FormTextInput";
import { FormButtons } from "../Form/FormButtons";
import { FormButton } from "../Form/FormButton";
import { FaSolidImage, FaSolidTag } from "solid-icons/fa";
import { FormTitle } from "../Form/FormTitle";
import { FormFileInput } from "../Form/FormFileInput";
import { API_ROOT } from "../Globals";
import { GroupDto } from "../interfaces/GroupDto";
import { AuthenticationManager } from "../AuthenticationManager";
import { RTManager } from "../RTManager";

interface CreateGroupFormProps {
    exitCallback: (success: boolean, response: GroupDto|null) => void;
}

export const CreateGroupForm: Component<CreateGroupFormProps> = (props: CreateGroupFormProps) => {
    const [title, setTitle] = createSignal("");
    const [icon, setIcon] = createSignal<FileList|null>(null);
    const [titleError, setTitleError] = createSignal<string|undefined>();
    const [iconError, setIconError] = createSignal<string|undefined>();
    const [loading, setLoading] = createSignal("nothing");

    const cancelFunction = () => {
        setLoading("cancel");
        props.exitCallback(false, null);
    };
    
    const createFunction = async () => {
        setLoading("create");

        setTitleError(undefined);
        setIconError(undefined);

        if (title().length < 2)
            setTitleError("Group title must be longer than 1 character");

        if (icon() == null)
            setIconError("Please provide an icon");
        else if (icon()!.length != 1)
            setIconError("Please provide 1 file");

        if (typeof titleError() !== 'undefined' || typeof iconError() !== 'undefined') {
            setLoading("nothing");
            return;
        }

        const requestBody = new FormData();
        requestBody.append("title", title());
        requestBody.append("icon", icon()![0]);

        await fetch(`${API_ROOT}/api/Group/Create`, {
            method: "post",
            body: requestBody,
            ...await AuthenticationManager.authorize(),
        })
            .then(res => res.json())
            .then((res: GroupDto) => {
                RTManager.get().pushGroup(res);
                props.exitCallback(true, res);
            })
            .catch(res => {
                console.error(res)
            });
    };

    return (
        <Form class={styles.CreateGroupForm + (loading() != "nothing" ? " " + styles.Out : "")}>
            <FormTitle>Create Group</FormTitle>
            <FormTextInput icon={(<FaSolidTag size={16} />)} valueCallback={setTitle} placeholder="Cool People Club" name="group-title" title="Group Title:" error={titleError()} />
            <FormFileInput icon={(<FaSolidImage size={16} />)} valueCallback={setIcon} placeholder="cat.png" name="group-image" title="Icon:" error={iconError()} />
            <FormButtons>
                <FormButton kind="secondary" loading={loading() == "cancel"} onClick={cancelFunction}>Cancel</FormButton>
                <FormButton kind="primary" loading={loading() == "create"} onClick={createFunction}>Create</FormButton>
            </FormButtons>
        </Form>
    );
};