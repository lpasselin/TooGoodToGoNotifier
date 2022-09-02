FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
COPY . /opt/app
RUN cd /opt/app && dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runner
COPY --from=builder /opt/app/out ./opt/app
WORKDIR /opt/app
CMD ["docker run"]  
