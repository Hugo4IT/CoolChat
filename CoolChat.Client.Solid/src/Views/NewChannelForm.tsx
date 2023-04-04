import { createSignal } from "solid-js";

import { FaSolidIcons, FaSolidTag } from "solid-icons/fa";
import { AuthenticationManager } from "../AuthenticationManager";
import { Form } from "../Form/Form";
import { FormButton } from "../Form/FormButton";
import { FormButtons } from "../Form/FormButtons";
import { FormCheckBox } from "../Form/FormCheckbox";
import { FormIconPicker } from "../Form/FormIconPicker";
import { FormTextInput } from "../Form/FormTextInput";
import { FormTitle } from "../Form/FormTitle";
import { API_ROOT } from "../Globals";
import { ChannelDto } from "../interfaces/ChannelDto";
import { View, ViewStateManager } from "../ViewStateManager";
import { Icons } from "./GroupView";
import styles from './NewChannelForm.module.pcss';

export class NewChannelForm extends View {
    id = "NewChannelForm";

    public override inDelay = (_: string) => 300;
    public override outDelay = (_: string) => 300

    private groupId: number;

    public constructor(groupId: number) {
        super();

        this.groupId = groupId;
    }

    view = () => {
        const [name, setName] = createSignal("");
        const [icon, setIcon] = createSignal<number>(0);
        const [nameError, setNameError] = createSignal<string|undefined>();
        const [iconError, setIconError] = createSignal<string|undefined>();
        const [isPublic, setIsPublic] = createSignal(true);
        const [loading, setLoading] = createSignal("nothing");

        const cancelFunction = () => {
            setLoading("cancel");
            ViewStateManager.get().pop();
        };
        
        const createFunction = async () => {
            setLoading("create");

            // setNameError(undefined);
            // setIconError(undefined);

            // if (name().length < 2)
            //     setNameError("Channel title must be longer than 1 character");

            // if (icon() == null)
            //     setIconError("Please provide an icon");
            // else if (icon()!.length != 1)
            //     setIconError("Please provide 1 file");

            // if (typeof nameError() !== 'undefined' || typeof iconError() !== 'undefined') {
            //     setLoading("nothing");
            //     return;
            // }

            const requestBody = new FormData();
            requestBody.append("name", name());
            requestBody.append("icon", icon().toString());
            requestBody.append("private", (!isPublic()).toString());

            await fetch(`${API_ROOT}/api/AddChannel/${this.groupId}`, {
                method: "post",
                body: requestBody,
                ...await AuthenticationManager.authorize(),
            })
                .then(res => res.json())
                .then((res: ChannelDto) => {
                    // RTManager.get().pushGroup(res);
                    // RTManager.get().trySubscribe(res);
                    ViewStateManager.get().pop();
                })
                .catch(res => {
                    console.error(res);
                });
        };

        return (
            <Form class={styles.NewChannelForm}
                classList={{
                    [styles.In]: this.transition() == "in",
                    [styles.Out]: this.transition() == "out",
                }}>
                <FormTitle>New Channel</FormTitle>
                <FormTextInput icon={(<FaSolidTag size={16} />)} valueCallback={setName} placeholder="Chaos Corner" name="channel-name" title="Channel Name:" error={nameError()} />
                <FormIconPicker icon={(<FaSolidIcons size={16} />)} valueCallback={setIcon} default={0} name="channel-icon" title="Icon" error={iconError()} icons={Icons(19)} />
                <FormCheckBox valueCallback={setIsPublic}
                    default={true}
                    name="channel-public"
                    text="Public" />
                <FormButtons>
                    <FormButton kind="secondary" loading={loading() == "cancel"} onClick={cancelFunction}>Cancel</FormButton>
                    <FormButton kind="primary" loading={loading() == "create"} onClick={createFunction}>Create</FormButton>
                </FormButtons>
            </Form>
        );
    };
}