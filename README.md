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

build bot docker image
```
docker build -f .\apps\backend\discord.bot\Dockerfile -t discord-bot .
```

run bot docker image
```
docker compose -f .\docker-compose.dev.yml up -d --remove-orphans --build bot
# run migrations
docker compose -f .\docker-compose.dev.yml run dbmigrations
```

stop all containers
```
docker compose -f .\docker-compose.dev.yml down --remove-orphans
```