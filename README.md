# Pizzas App API (Backend Project )

## Tech-Stack used:

- asp.net core
- entity framework core
- ms sql server
- docker

## Docker MS SQL SERVER Image command:

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Password1." -p 1433:1433 -v sqlvolume:/var/opt/mssql -d --rm --name mssql mcr.microsoft.com/mssql/server:2022-latest
