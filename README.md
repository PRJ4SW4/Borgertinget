# Borgertinget

Repository for project group 4 2025's Web-App: Borgertinget

# Dependencies

* Dotnet 9: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
* Docker Desktop: https://www.docker.com/products/docker-desktop/
* Node.js: https://nodejs.org/en

# Startup from scratch

## Backend Setup
1. Open terminal within "Borgertinget/backend" folder
2. Run ```docker compose up --build -d```
3. Run ```dotnet ef database update```
4. Run ```dotnet run```
5. Open http://localhost:5218/swagger/index.html to access Swagger in browser.

### Reset backend & Database
1. Run ```docker compose down -v``` within "backend" folder
2. Continue from step 2 of Backend Setup

## Frontend Setup
1. Open terminal within "Borgertinget/frontend" folder
2. Run ```npm install```
3. Run ```npm run dev```
4. Open http://localhost:5173/homepage in your browser.

**Everything should now be running, but endpoints need to be called in order for all functionality to work, you can either do this through the Admin Panel credit to #liv808 using the Admin User below or triggering the endpoints via. Swagger. Both are described below.**

# Admin User Credentials

Email: superuser@borgertinget.dk (Not an actual email don't try it)
Pass: Borgertinget123

# Admin Endpoint order

1. Log in with the Admin Credentials above.
2. You should now see an Admin button on the front page navbar. Click it.
3. Click "Hent alt" near the bottom.

# Manual Endpoint order

1. Go to http://localhost:5218/swagger/index.html in a browser.
2. Run the _"/api/Aktor/fetch"_ endpoint
3. Run the _"/api/Calendar/run-calendar-scraper"_ endpoint
4. Run the _"/api/polidle/admin/seed-all-aktor-quotes"_ endpoint
5. Run the _"/api/polidle/admin/generate-today"_ endpoint
6. Run the _"/api/search/ensure-and-reindex"_ endpoint

**All functionality should now be working, the twitter feed will take a while to warm up as we can only pull from their API one politician at a time every 16 minutes**

# READ IF NOT A BORGERTINGET DEV!

You obviously need a specific .env file for most of the features to work. Below is the template for the kind of authentication you need, you could fill it out yourself.
```
# --- Secrets For Docker Compose Services ---
# Credentials for the POSTGRES db
DB_USER=
DB_PASSWORD=
DB_NAME=
# Admin credentials for the PgAdmin Panel
PGADMIN_EMAIL=
PGADMIN_PASSWORD=

# --- Secrets for .NET Backend Applikationen ---
# Is read by DotNetEnv and mapped to IConfiguration via Section__Key format 
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD} 
Jwt__Key=
# Insert your gmail key below, you have to generate it, DONT SHARE IT WITH ANYONE. We use a fresh email for our testing purposes.
Email__Password=
# Your email itself
Email__Username=
# GoogleOAuth Authentication Client key, look it up yourself if you want GoogleOAuth to work.
GoogleOAuth__ClientSecret=

# Twitter Authentication
# These are for our feed implementation. If you want to test that go look up how to get these on twitters documentation page.
TwitterApiApiKey=
TwitterApiApiSecret=
TwitterApiAccessToken=
TwitterApiAccessSecret=
TwitterApi__BearerToken=
```
