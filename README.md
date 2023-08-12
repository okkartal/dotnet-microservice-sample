# dotnet-microservice-sample

When creating a nuget package for Catalog.Contract
```
cd Catalog/src/Catalog.Contracts

dotnet pack  -p:PackageVersion=1.0.0 -o ../../../packages

```

When creating a nuget package for Common project

```
cd Common

dotnet pack  -p:PackageVersion=1.0.0 -o ../packages

```

when starting docker for RabbitMQ and mongoDB

```

docker-compose up -d

```
