## Trading.Microservice
Sample Microservice Shop Trading microservice.

## Create and publish package
```powershell
$version="1.0.0"
$owner="SampleMicroserviceShop"
dotnet pack --configuration Release -p:PackageVersion=$version -o ..\..\packages\$owner
```

 ## Add the GitHub package source
```powershell
$owner="SampleMicroserviceShop"
$gh_pat="[PAT HERE]"
dotnet nuget add source --username USERNAME --password $gh_pat --store-password-in-clear-text --name github https://nuget.pkg.github.com/$owner/index.json
```
 ## Push Package to GitHub
```powershell
$version="1.0.0"
$gh_pat="[PAT HERE]"
$owner="SampleMicroserviceShop"
dotnet nuget push ..\..\packages\$owner\Trading.Service.$version.nupkg --api-key $gh_pat --source "github"
```

## Build the docker image
```powershell
$env:GH_OWNER="SampleMicroserviceShop"s
$env:GH_PAT="[PAT HERE]"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t trading.service:$version .
```

## Run the docker image
```powershell
docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq --network infra_default trading.service:$version
```

