export interface AuthenticatedResponse {
    success: boolean;
    usernameError?: string;
    passwordError?: string;

    token: string;
    refreshToken: string;
}