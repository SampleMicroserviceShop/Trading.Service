# Trading.Microservice
Sample Microservice Shop Trading microservice.

## General Variables
```powershell
$version="1.0.6"
$owner="SampleMicroserviceShop"
$gh_pat="[PAT HERE]"
$cosmosDbConnString="[CONN STRING HERE]"
$serviceBusConnString="[CONN STRING HERE]"
$appname="microshop"
$namespace="trading"
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
$env:GH_OWNER="SampleMicroserviceShop"
$env:GH_PAT="[PAT HERE]"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t trading.service:$version .
```
or with Azure Container Registry tag
```
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$appname.azurecr.io/trading.service:$version" .
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

## Create the Kubernetes namespace
```powershell
kubectl create namespace $namespace
```

## Create the Kubernetes pod
```powershell
kubectl apply -f .\Kubernetes\$namespace.yaml -n $namespace
```
other useful commands
```powershell
kubectl rollout restart deployment/trading-deployment -n trading
kubectl get pods -n $namespace -w
kubectl get services -n $namespace
kubectl logs [POD_NAME] -n $namespace -c $namespace
kubectl delete pod [POD_NAME] -n $namespace
```

## Get AKS 
```powershell
kubectl get services -n $namespace
```

## Creating the Azure Managed Identity and granting it access to Key Vault secrets
```powershell
az identity create --resource-group $appname --name $namespace

$IDENTITY_CLIENT_ID = az identity show -g $appname -n $namespace --query clientId -otsv

$Object_Id = az ad sp show --id $IDENTITY_CLIENT_ID --query id -o tsv
az role assignment create --assignee $Object_Id --role "Key Vault Secrets User" --scope $(az keyvault show -n $appname --query id -o tsv)
```

## Establish the federated identity credential
```powershell
$AKS_OIDC_ISSUER = az aks show -g $appname -n $appname --query oidcIssuerProfile.issuerUrl -otsv

az identity federated-credential create --name $namespace --identity-name $namespace --resource-group $appname --issuer $AKS_OIDC_ISSUER --subject "system:serviceaccount:${namespace}:${namespace}-serviceaccount"
```


