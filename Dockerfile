FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
COPY . /opt/app
RUN cd /opt/app && dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY --from=builder /opt/app/out /opt/app/out
ENTRYPOINT ["dotnet", "/opt/app/out/TooGoodToGoNotifier.dll"]