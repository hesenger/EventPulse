help:
	@cat Makefile

build:
	@dotnet build

test:
	@rm -rf ./TestResults
	@dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults"
	@reportgenerator -reports:./TestResults/**/coverage.cobertura.xml -targetdir:./TestResults/
	@open ./TestResults/index.html

startdb:
	@docker-compose up -d

stopdb:
	@docker-compose stop

pack:
	@rm -rf ./nupkg
	@dotnet pack -c Release -o nupkg
	@dotnet nuget push ./nupkg/*.nupkg -s https://api.nuget.org/v3/index.json -k $(NUGET_KEY)
