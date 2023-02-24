import { Component } from "solid-js";
import { FaSolidAt, FaSolidKey } from 'solid-icons/fa';

import styles from './LoginForm.module.css';

const LoginForm: Component = () => {
    let usernameRef: HTMLInputElement|undefined;
    let passwordRef: HTMLInputElement|undefined;

    const loginFunction = () => {
        let username = usernameRef!.value;
        let password = passwordRef!.value;

        
    };

    return (
        <div class={styles.LoginForm}>
            <label for={usernameRef}>Username:</label>
            <div class={styles.FormInput}>
                <div class={styles.FormInputIconContainer}>
                    <FaSolidAt size={16} class={styles.FormInputIcon} />
                </div>
                <input type="text" name="username" id="login-form-username" ref={usernameRef} />
            </div>

            <label for={passwordRef}>Password:</label>
            <div class={styles.FormInput}>
                <div class={styles.FormInputIconContainer}>
                    <FaSolidKey size={16} class={styles.FormInputIcon} />
                </div>
                <input type="password" name="password" id="login-form-password" ref={passwordRef} />
            </div>

            <button onClick={loginFunction}>Log In</button>
        </div>
    );
};

export default LoginForm;