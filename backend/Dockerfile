FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .

EXPOSE 8080

CMD ["dotnet", "watch", "run", "--no-launch-profile"]