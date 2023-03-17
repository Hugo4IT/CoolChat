import { Component, createEffect, createSignal, Match, onMount, Switch } from 'solid-js';

import styles from './App.module.css';
import { getToken } from './JwtHelper';
import { LoginForm } from './LoginForm/LoginForm';
import { Main } from './MainView/Main';
import { NotificationsManager } from './NotificationsManager';

const App: Component = () => {
    const [loggedIn, setLoggedIn] = createSignal(false);
    const [view, setView] = createSignal("login-attempt");

    const [accent, setAccent] = createSignal("blue");

    createEffect(() => {
        if (loggedIn()) {
            setTimeout(() => setView("main"), 300);
        } else {
            setTimeout(() => setView("login"), 300);
        }
    });

    const notificationsManager = new NotificationsManager();

    onMount(async () => {
        if (await getToken() != undefined)
            setLoggedIn(true);
        else
            setView("login")
    });

    createEffect(() => {
        for (let i = 0; i < 10; i++) {
            document.body.style.setProperty(`--accent-${i}`, `var(--oc-${accent()}-${i})`);
            document.body.style.setProperty(`--accent-background-${i}`, `var(--oc-${accent()}-${i})`);
        }
    });

    return (
        <div class={styles.App}>
            <Switch>
                <Match when={view() == "login-attempt"}>

                </Match>
                <Match when={view() == "login"}>
                    <LoginForm loginCallback={setLoggedIn} />
                </Match>
                <Match when={view() == "main"}>
                    <Main logoutCallback={() => setLoggedIn(false)} />
                </Match>
            </Switch>
            {notificationsManager.view({})}
        </div>
    );
};
    
export default App;