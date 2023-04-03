import { createSignal } from "solid-js";

import { FaSolidImage, FaSolidTag } from "solid-icons/fa";
import { AuthenticationManager } from "../AuthenticationManager";
import { Form } from "../Form/Form";
import { FormButton } from "../Form/FormButton";
import { FormButtons } from "../Form/FormButtons";
import { FormFileInput } from "../Form/FormFileInput";
import { FormTextInput } from "../Form/FormTextInput";
import { FormTitle } from "../Form/FormTitle";
import { API_ROOT } from "../Globals";
import { GroupDto } from "../interfaces/GroupDto";
import { RTManager } from "../RTManager";
import { View, ViewStateManager } from "../ViewStateManager";
import styles from './CreateGroupForm.module.pcss';


export class NewChannelForm extends View {
    id = "NewChannelForm";

    public override inDelay = (_: string) => 300;
    public override outDelay = (_: string) => 300

    view = () => {
        const [name, setName] = createSignal("");
        const [icon, setIcon] = createSignal<number>(0);
        const [nameError, setNameError] = createSignal<string|undefined>();
        const [iconError, setIconError] = createSignal<string|undefined>();
        const [loading, setLoading] = createSignal("nothing");

        const cancelFunction = () => {
            setLoading("cancel");
            ViewStateManager.get().pop();
        };
        
        const createFunction = async () => {
            setLoading("create");

            setNameError(undefined);
            setIconError(undefined);

            if (name().length < 2)
                setNameError("Group title must be longer than 1 character");

            if (icon() == null)
                setIconError("Please provide an icon");
            else if (icon()!.length != 1)
                setIconError("Please provide 1 file");

            if (typeof nameError() !== 'undefined' || typeof iconError() !== 'undefined') {
                setLoading("nothing");
                return;
            }

            const requestBody = new FormData();
            requestBody.append("title", name());
            requestBody.append("icon", icon()![0]);

            await fetch(`${API_ROOT}/api/Group/Create`, {
                method: "post",
                body: requestBody,
                ...await AuthenticationManager.authorize(),
            })
                .then(res => res.json())
                .then((res: GroupDto) => {
                    RTManager.get().pushGroup(res);
                    RTManager.get().trySubscribe(res);
                    ViewStateManager.get().pop();
                })
                .catch(res => {
                    console.error(res);
                });
        };

        return (
            <Form class={styles.CreateGroupForm}
                classList={{
                    [styles.In]: this.transition() == "in",
                    [styles.Out]: this.transition() == "out",
                }}>
                <FormTitle>Create Group</FormTitle>
                <FormTextInput icon={(<FaSolidTag size={16} />)} valueCallback={setName} placeholder="Cool People Club" name="group-title" title="Group Title:" error={nameError()} />
                <FormFileInput icon={(<FaSolidImage size={16} />)} valueCallback={setIcon} placeholder="cat.png" name="group-image" title="Icon:" error={iconError()} />
                <FormButtons>
                    <FormButton kind="secondary" loading={loading() == "cancel"} onClick={cancelFunction}>Cancel</FormButton>
                    <FormButton kind="primary" loading={loading() == "create"} onClick={createFunction}>Create</FormButton>
                </FormButtons>
            </Form>
        );
    };
}