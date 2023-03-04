# CoolChat

## Setup

A secret signing key is required for authentication, type some random characters
and store it in a user-secret:

```bash
dotnet user-secrets set "SecretSigningKey" "{your signing key}" --project CoolChat.Server.ASPNET
```