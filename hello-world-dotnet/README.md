```sh
dapr run --app-id dotnetapp --app-port 5001 --dapr-http-port 13500 -- dotnet run
```

```powershell
dapr invoke --app-id dotnetapp --method neworder --payload '{\"data\": { \"id\": \"42\" } }'
```