import { Accessor, createEffect, createSignal, Setter } from "solid-js";
import { API_ROOT } from "./Globals";
import { ValidationResponse } from "./ValidationResponse";

const USERNAME = "cc-lm-username";
const AUTH_TOKEN = "cc-lm-authToken";
const REFRESH_TOKEN = "cc-lm-refreshToken";
const PERSIST_TOKEN = "cc-lm-persistToken";

const USERNAME_TESTS: ValidationTest[] = [
    [input => input.length >= 5, "Username must contain at least 5 characters or more"],
    [input => input.length <= 40, "Username cannot be longer than 40 characters"],
    [input => !input.includes(' '), "Username cannot contain any spaces"],
];

const PASSWORD_TESTS: ValidationTest[] = [
    [input => input.length >= 12, "Password must be at least 12 characters long"],
    [input => input.length <= 56, "Password cannot be longer than 56 characters"],
];

declare type ValidationTest = [(input: string) => boolean, string];

function validate(tests: ValidationTest[], input: string) {
    for (const [test, message] of tests)
        if (!test(input))
            return message;
    
    return undefined;
}

function isValid(token: string) {
    const { exp } = JSON.parse(atob(token.split(".")[1]));

    return (Date.now() < exp * 1000);
}

interface Tokens {
    accessToken: string;
    refreshToken: string;
}

export class AuthenticationManager {
    private static instance: AuthenticationManager;

    public loggedIn: Accessor<boolean>;
    private setLoggedIn: Setter<boolean>;

    public username: Accessor<string|undefined>;
    private setUsername: Setter<string|undefined>;

    public storeToken: Accessor<boolean>;
    private setStoreToken: Setter<boolean>;

    private authToken: Accessor<string|undefined>;
    private setAuthToken: Setter<string|undefined>;
    
    private refreshToken: Accessor<string|undefined>;
    private setRefreshToken: Setter<string|undefined>;

    public constructor() {
        AuthenticationManager.instance = this;

        // Setup signals
        [this.loggedIn, this.setLoggedIn] = createSignal(false);
        [this.username, this.setUsername] = createSignal(undefined);
        [this.storeToken, this.setStoreToken] = createSignal(false);
        [this.authToken, this.setAuthToken] = createSignal(undefined);
        [this.refreshToken, this.setRefreshToken] = createSignal(undefined);

        // Restore localStorage
        this.restore();

        // Setup signal reactive callbacks

        // Update PERSIST_TOKEN when this.storeToken changes
        createEffect(() => {
            localStorage.setItem(PERSIST_TOKEN, this.storeToken() ? "true" : "false");
        });

        // Update localStorage when this.username changes
        createEffect(() => {
            const value = this.username();

            if (this.storeToken() && value != undefined)
                localStorage.setItem(USERNAME, value);
            else
                localStorage.removeItem(USERNAME);
        });

        // Update localStorage when this.authToken changes
        createEffect(() => {
            const value = this.authToken();

            if (this.storeToken() && value != undefined)
                localStorage.setItem(AUTH_TOKEN, value);
            else
                localStorage.removeItem(AUTH_TOKEN);
        });

        // Update localStorage when this.refreshToken changes
        createEffect(() => {
            const value = this.refreshToken();

            if (this.storeToken() && value != undefined)
                localStorage.setItem(REFRESH_TOKEN, value);
            else
                localStorage.removeItem(REFRESH_TOKEN);
        });
    }

    private clearTokens = () => {
        this.setAuthToken(undefined);
        this.setRefreshToken(undefined);
        this.setUsername(undefined);
        this.setLoggedIn(false);
    };

    private restore = () => {
        const persistToken = localStorage.getItem(PERSIST_TOKEN);

        if (persistToken == null)
            return;

        this.setStoreToken(persistToken == "true");

        if (!this.storeToken()) {
            localStorage.removeItem(USERNAME);
            localStorage.removeItem(AUTH_TOKEN);
            localStorage.removeItem(REFRESH_TOKEN);

            return;
        }

        const username = localStorage.getItem(USERNAME);
        const authToken = localStorage.getItem(AUTH_TOKEN);
        const refreshToken = localStorage.getItem(REFRESH_TOKEN);

        if (username == null || authToken == null || refreshToken == null)
            return this.clearTokens();
        
        this.setUsername(username);
        this.setAuthToken(authToken);
        this.setRefreshToken(refreshToken);
    };

    public refresh = async () => {
        // If either token is undefined, clear token cache
        if (this.username() == undefined
         || this.authToken() == undefined
         || this.refreshToken() == undefined)
            return this.clearTokens();
        
        // If token is not expired yet, don't refresh
        if (isValid(this.authToken()!)) {
            this.setLoggedIn(true);
            return;
        }

        const response = await fetch(`${API_ROOT}/api/Token/Refresh`, {
            method: "post",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                accessToken: this.authToken(),
                refreshToken: this.refreshToken(),
            }),
        }).catch(error => {
            console.error(error);
            this.clearTokens();
        });

        if (!response)
            return;

        const { accessToken, refreshToken } = await response.json();

        this.setAuthToken(accessToken);
        this.setRefreshToken(refreshToken);
        this.setUsername(this.username()); // Force update
        this.setLoggedIn(true);
    };

    public login = async (username: string, password: string): Promise<ValidationResponse> => {
        // If username is invalid, the user doesn't exist anyway
        if (validate(USERNAME_TESTS, username))
            return ValidationResponse.errors({ username: "Unable to find user with this name" });
        
        let response = await fetch(`${API_ROOT}/api/Auth/Login`, {
            method: "post",
            headers: {
                "Access-Control-Allow-Origin": "*",
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                "username": username,
                "password": password
            })
        }).catch(error => {
            console.error(error);
            this.clearTokens();
        });
        
        if (!response)
            return ValidationResponse.error("An unknown error occurred");
        
        const vResponse = new ValidationResponse(await response.json());

        if (!vResponse.isOk()) {
            this.clearTokens();

            return vResponse;
        }
    
        const tokens = vResponse.getValue<Tokens>()!;
        this.setAuthToken(tokens.accessToken);
        this.setRefreshToken(tokens.refreshToken);
        this.setUsername(username);
        this.setLoggedIn(true);

        return ValidationResponse.ok();
    };

    public register = async (username: string, password: string): Promise<ValidationResponse> => {
        const usernameError = validate(USERNAME_TESTS, username);
        const passwordError = validate(PASSWORD_TESTS, password);

        if (usernameError || passwordError)
            return new ValidationResponse({ success: false, errors: { username: usernameError, password: passwordError } });

        const response = await fetch(`${API_ROOT}/api/Auth/Register`, {
            method: "post",
            headers: {
                "Content-Type": "application/json",
                "Access-Control-Allow-Origin": "*"
            },
            body: JSON.stringify({
                "username": username,
                "password": password
            })
        }).catch(error => {
            console.error(error);
            this.clearTokens();
        });

        if (!response)
            return ValidationResponse.error("An unknown error occurred");

        const vResponse = new ValidationResponse(await response.json());

        if (!vResponse.isOk()) {
            this.clearTokens();

            return vResponse;
        }

        const tokens = vResponse.getValue<Tokens>()!;
        this.setAuthToken(tokens.accessToken);
        this.setRefreshToken(tokens.refreshToken);
        this.setUsername(username);
        this.setLoggedIn(true);

        return ValidationResponse.ok();
    }

    public logout = async () => {
        await fetch(`${API_ROOT}/api/Auth/Logout`, await AuthenticationManager.authorize());
        this.clearTokens();
    };

    public getToken = async () => {
        await this.refresh();
        return this.authToken();
    };

    public setPersistToken = (value: boolean) => this.setStoreToken(value);

    public static get = () => AuthenticationManager.instance;
    public static authorize = async () => ({ headers: { "Authorization": "Bearer " + await AuthenticationManager.instance.getToken()} });
}