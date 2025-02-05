# Borgertinget

Repository for project group 4 2025's Web-App: Borgertinget

# Missing setup for react

"Frontend:

Add scripts to your package.json:
json
Copy
Edit
{
"scripts": {
"lint": "eslint src/\*_/_.{js,jsx,ts,tsx}",
"test": "react-scripts test --env=jsdom",
"build": "react-scripts build"
}
}
Replace react-scripts with your actual build/test tooling if different."

We need to setup react to work with the cicd workflow specified.
