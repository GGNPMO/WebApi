# Employee Payroll API

ASP.NET Core Web API for employee payroll management. The API is packaged as a small multi-stage .NET 10 container and has a Kubernetes base configuration that works with Minikube, Kind, and AKS.

## Local configuration

Runtime settings are supplied through environment variables. Do not put database credentials or JWT keys in `appsettings*.json` or source control.

PowerShell example:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=EmployeePayrollDb;Trusted_Connection=True;TrustServerCertificate=True"
$env:Jwt__Key = "generate-a-long-random-development-key"
$env:Jwt__Issuer = "PayrollApi"
$env:Jwt__Audience = "PayrollReactApp"
$env:Database__EnsureCreated = "true"
dotnet run --project .\src\Payroll.Api\Payroll.Api.csproj
```

`/health` is an anonymous endpoint intended for Kubernetes probes. It does not expose configuration or database details.

## Docker

From the repository root:

```powershell
$env:IMAGE_NAME = "payroll-api"
$env:IMAGE_TAG = "1.0.0"
docker build --tag "$env:IMAGE_NAME`:$env:IMAGE_TAG" .
docker run --rm --publish 8080:8080 `
  --env "ConnectionStrings__DefaultConnection=$env:ConnectionStrings__DefaultConnection" `
  --env "Jwt__Key=$env:Jwt__Key" `
  --env "Jwt__Issuer=PayrollApi" `
  --env "Jwt__Audience=PayrollReactApp" `
  --env "Database__EnsureCreated=false" `
  "$env:IMAGE_NAME`:$env:IMAGE_TAG"

Invoke-WebRequest http://localhost:8080/health
```

The Dockerfile uses the official free `mcr.microsoft.com/dotnet` SDK and ASP.NET runtime images, publishes without an app host, and runs as the runtime image's non-root `APP_UID`.

## Kubernetes deployment

The default Deployment has one replica, small requests/limits, readiness and liveness probes, no public IP, and a `ClusterIP` Service. The Secret template is not included in Kustomize resources.

Create the secret from values held outside the repository:

```powershell
$env:DB_CONNECTION = "Server=sql-host;Database=EmployeePayrollDb;User Id=payroll;Password=use-a-secret-store;TrustServerCertificate=True"
$env:JWT_KEY = "generate-a-long-random-key"
kubectl create secret generic payroll-api-secrets `
  --from-literal="ConnectionStrings__DefaultConnection=$env:DB_CONNECTION" `
  --from-literal="Jwt__Key=$env:JWT_KEY" `
  --dry-run=client --output yaml | kubectl apply -f -
```

### Minikube

```powershell
minikube start --driver=docker
$env:IMAGE_NAME = "payroll-api"
$env:IMAGE_TAG = "1.0.0"
docker build --tag "$env:IMAGE_NAME`:$env:IMAGE_TAG" .
minikube image load "$env:IMAGE_NAME`:$env:IMAGE_TAG"
kubectl apply -k .\k8s
kubectl set image deployment/payroll-api "api=$env:IMAGE_NAME`:$env:IMAGE_TAG"
kubectl rollout status deployment/payroll-api
kubectl port-forward service/payroll-api 8080:80
```

### Kind

```powershell
kind create cluster --name payroll
$env:IMAGE_NAME = "payroll-api"
$env:IMAGE_TAG = "1.0.0"
docker build --tag "$env:IMAGE_NAME`:$env:IMAGE_TAG" .
kind load docker-image "$env:IMAGE_NAME`:$env:IMAGE_TAG" --name payroll
kubectl apply -k .\k8s
kubectl set image deployment/payroll-api "api=$env:IMAGE_NAME`:$env:IMAGE_TAG"
kubectl rollout status deployment/payroll-api
kubectl port-forward service/payroll-api 8080:80
```

### AKS

Use a small single-node cluster for a low-cost test environment. The AKS Free tier avoids a control-plane charge, but the node and other resources still incur Azure charges.

```powershell
$env:RESOURCE_GROUP = "rg-payroll-dev"
$env:AKS_NAME = "aks-payroll-dev"
az login
az group create --name $env:RESOURCE_GROUP --location eastus
az aks create --resource-group $env:RESOURCE_GROUP --name $env:AKS_NAME `
  --tier free --node-count 1 --node-vm-size Standard_B2s `
  --generate-ssh-keys
az aks get-credentials --resource-group $env:RESOURCE_GROUP --name $env:AKS_NAME --overwrite-existing
```

Push the image to a registry that the cluster can pull from, then deploy. For a private registry, configure `imagePullSecrets`; do not put registry credentials in this repository.

```powershell
$env:IMAGE_NAME = "docker.io/<your-registry-user>/payroll-api"
$env:IMAGE_TAG = "1.0.0"
docker build --tag "$env:IMAGE_NAME`:$env:IMAGE_TAG" .
docker login docker.io
docker push "$env:IMAGE_NAME`:$env:IMAGE_TAG"
kubectl apply -k .\k8s
kubectl set image deployment/payroll-api "api=$env:IMAGE_NAME`:$env:IMAGE_TAG"
kubectl rollout status deployment/payroll-api
kubectl port-forward service/payroll-api 8080:80
```

For a private Azure Container Registry, the equivalent setup is `az acr create`, `az acr login`, `az aks update --attach-acr`, then push the image and set the same Deployment image. This is optional and adds an Azure resource.

## Operations

```powershell
kubectl get pods,svc,deploy
kubectl describe pod -l app.kubernetes.io/name=payroll-api
kubectl logs deployment/payroll-api --tail=100
kubectl get events --sort-by=.lastTimestamp
kubectl rollout history deployment/payroll-api
```

Rollback a failed image or configuration rollout:

```powershell
kubectl rollout undo deployment/payroll-api
kubectl rollout status deployment/payroll-api
```

Common fixes:

- `CreateContainerConfigError`: create `payroll-api-secrets` in the current namespace.
- `ImagePullBackOff`: verify the image tag, registry visibility, AKS pull permissions, or local Minikube/Kind image loading.
- Probe failures: inspect `kubectl logs` and confirm the container listens on port `8080`; test with `kubectl port-forward service/payroll-api 8080:80`.
- Database errors: verify `ConnectionStrings__DefaultConnection`, network access from the cluster, and SQL Server credentials. Production does not run `EnsureCreated`.

## Cost saving and cleanup

Keep one replica and the smallest node that meets your workload. Use `kubectl port-forward` instead of adding a LoadBalancer or ingress for private/test access. Stop the AKS node pool when it is not needed:

```powershell
az aks nodepool stop --resource-group $env:RESOURCE_GROUP --cluster-name $env:AKS_NAME --name nodepool1
az aks nodepool start --resource-group $env:RESOURCE_GROUP --cluster-name $env:AKS_NAME --name nodepool1
```

Stopping the node pool reduces compute charges; attached disks and other resources may still cost money. Delete the whole test environment when finished:

```powershell
az group delete --name $env:RESOURCE_GROUP --yes --no-wait
```
