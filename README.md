# Getting started

```
dotnet user-secrets set "Discord:BotToken" "<Your bot token here>" --project apps/backend/discord.bot/
```

Create migrations
```
dotnet ef migrations add <migration-name> --project apps/backend/data.access/
```

Execute migrations
```
dotnet ef database update --project apps/backend/data.access/
```