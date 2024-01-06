help:
	@cat Makefile

build:
	@dotnet build

test:
	@rm -rf ./TestResults
	@dotnet test --collect:"XPlat Code Coverage" --results-directory:"./TestResults"
	@reportgenerator -reports:./TestResults/**/coverage.cobertura.xml -targetdir:./TestResults/
	@open ./TestResults/index.html
