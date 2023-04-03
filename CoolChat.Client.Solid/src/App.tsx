import { Component, createEffect, createSignal, Match, onMount, Switch, untrack } from 'solid-js';

import { FaSolidCircleNotch } from 'solid-icons/fa';
import styles from './App.module.pcss';
import { AuthenticationManager } from './AuthenticationManager';
import { NotificationsManager } from './NotificationsManager';
import { RTManager } from './RTManager';
import { LoadingScreen } from './Views/LoadingScreen';
import { ViewStateManager } from './ViewStateManager';
import { LoginForm } from './Views/LoginForm';
import { Main } from './Views/Main';

const App: Component = () => {
    const [accent, setAccent] = createSignal("blue");

    const loadingScreen = new LoadingScreen();
    loadingScreen.setLoadText("Connecting...");
    
    const viewStateManager = new ViewStateManager(loadingScreen);
    const authenticationManager = new AuthenticationManager();
    const notificationsManager = new NotificationsManager();
    const rtManager = new RTManager();

    onMount(async () => {
        await authenticationManager.refresh();

        if (authenticationManager.loggedIn()) {
            await viewStateManager.push(new Main());
        } else {
            await viewStateManager.push(new LoginForm())
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
            {viewStateManager.view({})}
            {notificationsManager.view({})}
        </div>
    );
};
    
export default App;