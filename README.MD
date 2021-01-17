# DAPR C# DEMO

This is a sample project using Dapr with dotnet core 5.0 and redis cache as a store.
We have a sample api controller with several actions:
- create client
- get client
- delete client

Check `src/DaprSamples/SampleStateStore_Redis` for a sample project.

## TOOLS

* [Minikube](https://minikube.sigs.k8s.io/docs/)
* Docker
* Kubernetes (kubectl)
* dotnet core 5.0
* [Dapr](https://dapr.io)

## INSTALL DAPR ON KUBERNETES

I am going to use `minukube` to run Kubernetes locally.

1. Start minikube
```cli
minikube start
```

2. Install DAPR locally on your kubernetes cluster

```cli
dapr init -k
```


3. Install Redis into your cluster

```cli
dapr init -k
```

and check the status

```cli
kubectl get pods
```
, and you should see 3 nodes in RUNNING status.

To get the redis password, use

```cli
export REDIS_PASSWORD=$(kubectl get secret --namespace default redis -o jsonpath="{.data.redis-password}" | base64 --decode)
```


4. Run DAPR API project locally

```cli
cd src/DaprSamples/SampleStateStore_Redis
dotnet build
dapr run --app-id controller --app-port 5000 -- dotnet run
```

, where app-id defines the name of the app for Dapr.

5. Add DAPR components to the Kubernetes cluster

5.1. Create  redis-statestore.yaml

 and add

```cli
apiVersion: v1
kind: Secret
metadata:
  name: redis
data:
  redis-password: <REPLACE WITH BASE64 ENCODED PASSWORD>
---
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
  namespace: default
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    secretKeyRef:
      name: redis
      key: redis-password
```

Or check [redis-statestore.yaml](src/DaprSamples/SampleStateStore_Redis/Deployment/redis-statestore.yaml)


Here we create a state store component for persistence and restoration, using redis cache.


5.2. Deploy to the Kubernetes cluster

```cli
deploy -f redis-pubsub.yaml apply
```

6. Mappings

* `redis-pubsub.yaml --> kind: Component (1st) --> name: statestore`  equals `SampleController.cs --> public const string StoreName = "statestore";`
* `redis-pubsub.yaml --> kind: Component (2nd) --> name: pubsub`  equals `SampleController.cs --> [Topic("pubsub", ...)]`


7. Add client to the store

```http
POST http://localhost:5000/addClient
Content-Type: application/json

{
    "Id": "1",
    "FirstName" : "Evgeny",
    "LastName" : "Grishchenko"
}
```

To verify the redis (the store ):

```cli
redis-cli -h localhost -a <YOUR REDIS PASSWORD>
KEYS *
HGET <OUR KEY NAME>
```

Yes, the value is stored as HASH type.
In this case the name of the key would be `controller||<client id>`


8.  Get client from the store

```http
GET http://localhost:5000/getClient/1
Content-Type: application/json

{
    "id": "1",
    "firstName": "Evgeny",
    "lastName": "Grishchenko"
}
```

9. Delete client from the store

```http
DELETE http://localhost:5000/deleteClient/1
```


## Who do I talk to?

* Evgeny Grishchenko <e.grisss@gmail.com>



