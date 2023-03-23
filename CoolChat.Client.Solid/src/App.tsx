import { Component, createEffect, createSignal, Match, on, onMount, Switch, untrack } from 'solid-js';

import styles from './App.module.pcss';
import { LoginForm } from './LoginForm/LoginForm';
import { AuthenticationManager } from './AuthenticationManager';
import { Main } from './MainView/Main';
import { NotificationsManager } from './NotificationsManager';
import { FaSolidCircleNotch } from 'solid-icons/fa';
import { RTManager } from './RTManager';

const App: Component = () => {
    const [view, setView] = createSignal("login-attempt");
    const [accent, setAccent] = createSignal("blue");

    const [loadText, setLoadText] = createSignal("");

    const authenticationManager = new AuthenticationManager();
    const notificationsManager = new NotificationsManager();
    const rtManager = new RTManager();
    
    // Change view with delay for animation
    createEffect(async () => {
        const loggedIn = authenticationManager.loggedIn();
        
        if (untrack(view) == "login-attempt")
            return;
        
        if (loggedIn) {
            await rtManager.load(setLoadText);
            setTimeout(() => setView("main"), 300);
        } else {
            await rtManager.unload();
            setTimeout(() => setView("login"), 300);
        }
    });

    onMount(async () => {
        await authenticationManager.refresh();

        if (!authenticationManager.loggedIn()) {
            setView("login");
        } else {
            await rtManager.load(setLoadText);
            setView("main");
        }
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
                    <FaSolidCircleNotch size={24} class={styles.LoadingSpinner} />
                    <span class={styles.LoadText}>{loadText()}</span>
                </Match>
                <Match when={view() == "login"}>
                    <LoginForm />
                </Match>
                <Match when={view() == "main"}>
                    <Main />
                </Match>
            </Switch>
            {notificationsManager.view({})}
        </div>
    );
};
    
export default App;