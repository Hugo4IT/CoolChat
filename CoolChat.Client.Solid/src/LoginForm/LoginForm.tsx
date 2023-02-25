import { Component, createSignal, Show } from "solid-js";
import { FaSolidAt, FaSolidKey } from 'solid-icons/fa';

import styles from './LoginForm.module.css';
import { AuthenticatedResponse } from "../interfaces/AuthenticatedResponse";

const LoginForm: Component = () => {
    const [usernameError, setUsernameError] = createSignal("");
    const [passwordError, setPasswordError] = createSignal("");
    const [invalidLogin, setInvalidLogin] = createSignal(false);

    let usernameRef: HTMLInputElement|undefined;
    let passwordRef: HTMLInputElement|undefined;

    const loginFunction = () => {
        let username = usernameRef!.value;
        let password = passwordRef!.value;

        fetch("http://localhost:5010/api/Login", {
            headers: {
                "Content-Type": "application/json",
            }
        }).then(res => res.json())
          .then((res: AuthenticatedResponse) => {
              const token = res.token;
              localStorage.setItem("jwt", token);

              setInvalidLogin(false);
          })
          .catch(res => {
              setInvalidLogin(true);
              console.log(res);
          });
    };

    return (
        <div class={styles.LoginForm}>
            <label for="username">Username:</label>
            <div class={styles.FormInput}>
                <div class={styles.FormInputIconContainer}>
                    <FaSolidAt size={16} class={styles.FormInputIcon} />
                </div>
                <input type="text" name="username" id="login-form-username" ref={usernameRef} />
            </div>

            <label for="password">Password:</label>
            <div class={styles.FormInput}>
                <div class={styles.FormInputIconContainer}>
                    <FaSolidKey size={16} class={styles.FormInputIcon} />
                </div>
                <input type="password" name="password" id="login-form-password" ref={passwordRef} />
            </div>

            <Show when={invalidLogin()}>
                <div class={styles.LoginError}>
                    {usernameError()}
                    <br/>
                    {passwordError()}
                </div>
            </Show>

            <button class={styles.LoginButton} onClick={loginFunction}>Log In</button>
        </div>
    );
};

export default LoginForm;