[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N7LH6KJ)

# CoolChat

CoolChat is a chat application inspired by Discord, built with the ASP.NET framework and SolidJS for the frontend.

<!-- ## Quick Setup & Run

Run this in Git Bash:

```bash
chmod +x run && ./run
``` -->

## Setup

A secret signing key is required for authentication, type some random characters
and store it in a user-secret:

```bash
dotnet user-secrets init --project CoolChat.Server.ASPNET
dotnet user-secrets set "SecretSigningKey" "{your signing key}" --project CoolChat.Server.ASPNET
```

You must also run the migrations, to do this first install efcore tools:

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

Then update the database:

```bash
dotnet ef database update --project CoolChat.Server.ASPNET
```

## Running

To start the ASP.NET backend, simply run

```bash
dotnet run --project ./CoolChat.Server.ASPNET --launch-profile http
```

To start the frontend, run `npm install` and `npm run dev` in the `CoolChat.Client.Solid` directory

## Screenshots

Everything has smooth and pretty animations, clone the repo for yourself to try it out

![image](https://github.com/user-attachments/assets/c5d173c6-ea10-4a77-8ad0-0812fa9f4a47)
![image](https://github.com/user-attachments/assets/cb881c14-d5f4-4172-878d-00bdf2756dc0)
![image](https://github.com/user-attachments/assets/b6751215-f44d-4d1b-8aad-f8f2937968be)
![image](https://github.com/user-attachments/assets/15b62805-53ab-4ba9-a8bb-f42787685574)
![image](https://github.com/user-attachments/assets/a17e1af2-4657-489e-a125-4d2e98f30cb6)
![image](https://github.com/user-attachments/assets/14d31a23-4561-40bc-a30b-5e3cedb7acfd)
![image](https://github.com/user-attachments/assets/e2ae24c7-78e0-4471-b0dd-8a74eda77b32)

