FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PaymentsAPI/PaymentsAPI.csproj", "src/PaymentsAPI/"]
RUN dotnet restore "src/PaymentsAPI/PaymentsAPI.csproj"
COPY . .
WORKDIR "/src/src/PaymentsAPI"
RUN dotnet build "PaymentsAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM runtime AS final
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentsAPI.dll"]
