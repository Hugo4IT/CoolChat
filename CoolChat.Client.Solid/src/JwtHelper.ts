import { API_ROOT } from "./Globals";
import { AuthenticatedResponse } from "./interfaces/AuthenticatedResponse";

export interface LoginResponse {
    success: boolean;
    usernameError?: string;
    passwordError?: string;
}

export async function tryRefreshToken(token: string) {
    const refreshToken = localStorage.getItem("refreshToken");

    if (!token || !refreshToken)
        return false;
    
    const credentials = JSON.stringify({ accessToken: token, refreshToken: refreshToken });
    let refreshFailed = false;

    await fetch(`${API_ROOT}/api/Token/Refresh`, {
        body: credentials,
        headers: {
            "Content-Type": "application/json",
        },
        method: "post"
    })
        .then(res => res.json())
        .then((res: AuthenticatedResponse) => {
            localStorage.setItem("jwt", res.token);
            localStorage.setItem("refreshToken", res.refreshToken);
        })
        .catch(res => {
            refreshFailed = true;
            console.error(res);
        });
    
    return !refreshFailed;
}

export async function isValid(token: string) {
    const exp = JSON.parse(atob(token.split(".")[1]))["exp"];

    if (Date.now() >= exp * 1000)
        return tryRefreshToken(token);
    
    return true;
}

export async function register(username: string, password: string): Promise<LoginResponse> {
    let success = false;
    let usernameError: string|undefined;
    let passwordError: string|undefined;

    if (username.trim().length == 0)
        return { success: false, usernameError: "Please enter a username" };

    if (password.length < 12)
        return { success: false, passwordError: "Password must be 12 characters or longer"};

    if (password.length > 56)
        return { success: false, passwordError: "Password may not be longer than 56 characters"};
    
    await fetch(`${API_ROOT}/api/Auth/Register`, {
        method: "post",
        headers: {
            "Content-Type": "application/json",
            "Access-Control-Allow-Origin": "*"
        },
        body: JSON.stringify({
            "username": username,
            "password": password
        })
    })
        .then(res => res.json())
        .then((res: AuthenticatedResponse) => {
            localStorage.setItem("jwt", res.token);
            localStorage.setItem("refreshToken", res.refreshToken);
            localStorage.setItem("username", username);

            success = res.success;
            usernameError = res.usernameError;
            passwordError = res.passwordError;
        })
        .catch(res => {
            console.error(res);
        });

    return { success, usernameError, passwordError };
}

export async function login(username: string, password: string): Promise<LoginResponse> {
    let success = false;
    let usernameError: string|undefined;
    let passwordError: string|undefined;

    if (username.trim().length == 0)
        return { success: false, usernameError: "Please enter a username" };

    await fetch(`${API_ROOT}/api/Auth/Login`, {
        method: "post",
        headers: {
            "Content-Type": "application/json",
            "Access-Control-Allow-Origin": "*"
        },
        body: JSON.stringify({
            "username": username,
            "password": password
        })
    })
        .then(res => res.json())
        .then((res: AuthenticatedResponse) => {
            localStorage.setItem("jwt", res.token);
            localStorage.setItem("refreshToken", res.refreshToken);
            localStorage.setItem("username", username);

            success = res.success;
            usernameError = res.usernameError;
            passwordError = res.passwordError;
        })
        .catch(res => {
            console.error(res);
        });
    
    return { success, usernameError, passwordError };
}

export function logout() {
    localStorage.removeItem("jwt");
    localStorage.removeItem("refreshToken");
}

export async function getToken() {
    const token = localStorage.getItem("jwt");
    if (token == null)
        return undefined;
    
    if (await isValid(token))
        return localStorage.getItem("jwt") ?? undefined;
    
    return undefined;
}