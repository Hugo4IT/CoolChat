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
