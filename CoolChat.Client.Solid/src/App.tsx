import { Component, createResource } from 'solid-js';

import styles from './App.module.css';
import LoginForm from './LoginForm/LoginForm';

const App: Component = () => {
    return (
        <div class={[styles.App, styles.ThemeDark].join(' ')}>
            <LoginForm />
        </div>
    );
};
    
export default App;