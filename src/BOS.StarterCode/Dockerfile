##
# Create a runtime image for the service
# ---
# Assumptions:
#   - The context of the docker build is the output of a dotnet publish
##

FROM microsoft/dotnet:2.2-aspnetcore-runtime
ENV ASPNETCORE_URLS http://+:80
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "BOS.StarterCode.dll"]
