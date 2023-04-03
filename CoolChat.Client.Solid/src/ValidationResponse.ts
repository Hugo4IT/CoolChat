export class ValidationResponse {
    private success: boolean;
    private value?: any;
    private errors?: Map<string, string>;

    public constructor(raw: any) {
        this.success = raw.success;

        if (!this.success) {
            this.errors = new Map();

            for (const [key, value] of Object.entries(raw.errors))
                this.errors.set(key, value as string);
        } else {
            this.value = raw.value;
        }
    }

    public isOk = () => this.success;
    public getDefaultError = () => this.getError("default") ?? <string|undefined>this.errors?.values().next().value;
    public getError = (key: string) => {
        if (!this.errors)
            return undefined;

        return this.errors.get(key);
    };
    public getValue = <T>() => this.value != undefined ? this.value as T : undefined;

    public static ok = () => new ValidationResponse({ success: true });
    public static error = (message: string) =>
        new ValidationResponse({ success: false, errors: { default: message } });
    public static errors = (errors: any) =>
        new ValidationResponse({ success: false, errors });
}