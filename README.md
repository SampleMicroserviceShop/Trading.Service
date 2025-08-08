# Trading.Microservice
Sample Microservice Shop Trading microservice.

## General Variables
```powershell
$version="1.0.1"
$owner="SampleMicroserviceShop"
$gh_pat="[PAT HERE]"
$cosmosDbConnString="[CONN STRING HERE]"
$serviceBusConnString="[CONN STRING HERE]"
$appname="microshop"
```


## Create and publish package
```powershell
dotnet pack --configuration Release -p:PackageVersion=$version -o ..\..\packages\$owner
```

 ## Add the GitHub package source
```powershell
dotnet nuget add source --username USERNAME --password $gh_pat --store-password-in-clear-text --name github https://nuget.pkg.github.com/$owner/index.json
```
 ## Push Package to GitHub
```powershell
dotnet nuget push ..\..\packages\$owner\Trading.Service.$version.nupkg --api-key $gh_pat --source "github"
```

## Build the docker image
```powershell
$env:GH_OWNER="SampleMicroserviceShop"s
$env:GH_PAT="[PAT HERE]"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t trading.service:$version .
```
or with Azure Container Registry tag
```
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$appname.azurecr.io/trading.service:$version"
```

## Run the docker image
```powershell
docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq --network infra_default trading.service:$version
```

## Run the docker image - using ServiceBus ConnectionString
```powershell
docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__ConnectionString=$cosmosDbConnString \
 -e ServiceBusSettings__ConnectionString=$serviceBusConnString -e ServiceSettings__MessageBroker="SERVICEBUS" \
--network infra_default trading.service:$version
```

## Retag docker image to publish to Azure Container Registry
```powershell
docker tag trading.service:$version "$appname.azurecr.io/trading.service:$version"
```

## Publish the docker image to Azure Container Registry
```powershell
az acr login --name $appname
docker push "$appname.azurecr.io/trading.service:$version"
```