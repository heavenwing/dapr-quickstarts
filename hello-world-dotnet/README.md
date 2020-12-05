```sh
dapr run --app-id dotnetapp --app-port 5000 --dapr-http-port 13501 -- dotnet run
```

```powershell
dapr invoke --app-id dotnetapp --method neworder --payload '{\"data\": { \"id\": \"42\" } }'
```