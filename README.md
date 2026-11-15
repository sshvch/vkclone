# VK clone

Учебный проект, клон ВК на микросервисах. Регистрация и вход, профиль с загрузкой аватарки, друзья и заявки, лента с постами, комментариями и лайками, удаление своих постов и комментариев, чат через SignalR.

## Запуск

Сначала MongoDB:

```bash
cd infra
docker compose up -d
```

Потом четыре сервиса, каждый в своём терминале:

```bash
cd backend/auth && dotnet run --launch-profile http
cd backend/user && dotnet run --launch-profile http
cd backend/content && dotnet run --launch-profile http
cd backend/chat && dotnet run --launch-profile http
```

И фронтенд:

```bash
cd frontend
npm install
npm run dev
```

Фронт откроется на http://localhost:5173.

## Порты

auth 5143, user 5260, content 5119, chat 5281, MongoDB 27017. Swagger у каждого сервиса на `/swagger`.

## Как устроено

auth хранит аккаунты и выдаёт JWT. user хранит профили, друзей и заявки, профиль создаётся при первом запросе, аватарки лежат в `backend/user/wwwroot/avatars`. content хранит посты и комментарии. chat хранит сообщения и отдаёт их через SignalR. У каждого сервиса своя база в MongoDB.
