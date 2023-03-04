import { Component, createEffect, createSignal, Match, Switch } from 'solid-js';

import styles from './App.module.css';
import { LoginForm } from './LoginForm/LoginForm';
import { Main } from './MainView/Main';

const App: Component = () => {
    const [loggedIn, setLoggedIn] = createSignal(false);
    const [view, setView] = createSignal("login");

    createEffect(() => {
        if (loggedIn()) {
            setTimeout(() => setView("main"), 300);
        } else {
            setTimeout(() => setView("login"), 300);
        }
    });

    return (
        <div class={[styles.App, styles.ThemeDark].join(' ')}>
            <Switch>
                <Match when={view() == "login"}>
                    <LoginForm loginCallback={setLoggedIn} />
                </Match>
                <Match when={view() == "main"}>
                    <Main logoutCallback={() => setLoggedIn(false)} />
                </Match>
            </Switch>
        </div>
    );
};
    
export default App;