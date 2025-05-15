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

**Everything should now be running, but endpoints need to be called in order for all functionality to work**

# Endpoint order

1. Go to http://localhost:5218/swagger/index.html in a browser.
2. Run the _"/api/Aktor/fetch"_ endpoint
3. Run the _"/api/Calendar/run-calendar-scraper"_ endpoint
4. Run the _"/api/polidle/admin/seed-all-aktor-quotes"_ endpoint
5. Run the _"/api/polidle/admin/generate-today"_ endpoint
6. Run the _"/api/search/ensure-and-reindex"_ endpoint

**All functionality should now be working, the twitter feed will take a while to warm up as we can only pull from their API one politician at a time every 16 minutes**
