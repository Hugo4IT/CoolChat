# CoolChat

## Setup

A secret signing key is required for authentication, type some random characters
and store it in a user-secret:

```bash
dotnet user-secrets set "SecretSigningKey" "{your signing key}" --project CoolChat.Server.ASPNET
```

## Running

To start the ASP.NET backend, simply run

```
dotnet run --project ./CoolChat.Server.ASPNET --launch-profile http
```

To start the frontend, run `npm run dev` in the `CoolChat.Client.Solid` directory