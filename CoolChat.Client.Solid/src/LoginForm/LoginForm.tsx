import { Component, createSignal } from "solid-js";
import { FaSolidKey, FaSolidUser } from 'solid-icons/fa';

import styles from './LoginForm.module.css';
import { Form } from "../Form/Form";
import { FormTextInput } from "../Form/FormTextInput";
import { FormButtons } from "../Form/FormButtons";
import { FormButton } from "../Form/FormButton";
import { FormTitle } from "../Form/FormTitle";
import { FormCheckBox } from "../Form/FormCheckbox";
import { IDBManager } from "../IDBManager";
import { AuthenticationFailed, AuthenticationManager } from "../AuthenticationManager";

interface LoginFormProps {

}

export const LoginForm: Component<LoginFormProps> = (props: LoginFormProps) => {
    const [username, setUsername] = createSignal("");
    const [password, setPassword] = createSignal("");
    const [usernameError, setUsernameError] = createSignal<string|undefined>();
    const [passwordError, setPasswordError] = createSignal<string|undefined>();
    const [loading, setLoading] = createSignal("nothing");

    const authenticationManager = AuthenticationManager.get();

    let passwordRef: HTMLInputElement|undefined;

    const loginFunction = async () => {
        if (loading() != "nothing")
            return;

        setLoading("login");

        let result = await authenticationManager.login(username(), password());

        setLoading("nothing");

        if (result.success()) {
            setLoading("main");
        } else {
            const { errors } = result as AuthenticationFailed;

            setUsernameError(errors.username);
            setPasswordError(errors.password);
        }
    };
    
    const registerFunction = async () => {
        if (loading() != "nothing")
            return;

        setLoading("register");

        let result = await authenticationManager.register(username(), password());

        setLoading("nothing");

        if (result.success()) {
            setLoading("main");
        } else {
            const { errors } = result as AuthenticationFailed;

            setUsernameError(errors.username);
            setPasswordError(errors.password);
        }
    };

    return (
        <Form class={styles.LoginForm + (loading() == "main" ? " " + styles.Out : "")}>
            <FormTitle>Account</FormTitle>
            <FormTextInput icon={(<FaSolidUser size={16} />)}
                           valueCallback={setUsername}
                           placeholder="CoolGuy123"
                           name="username"
                           title="Username:"
                           error={usernameError()}
                           onSubmit={() => passwordRef!.focus()}/>
            <FormTextInput icon={(<FaSolidKey size={16} />)}
                           valueCallback={setPassword}
                           placeholder="CoolPassword123"
                           kind="password"
                           name="password"
                           title="Password:"
                           error={passwordError()}
                           ref={ref => passwordRef = ref}
                           onSubmit={loginFunction}/>
            <FormCheckBox valueCallback={authenticationManager.setPersistToken}
                          default={true}
                          name="remember-me"
                          text="Remember Me" />
            <FormButtons>
                <FormButton kind="secondary" loading={loading() == "register"} onClick={registerFunction}>Register</FormButton>
                <FormButton kind="primary" loading={loading() == "login"} onClick={loginFunction}>Log In</FormButton>
            </FormButtons>
        </Form>
    );
};