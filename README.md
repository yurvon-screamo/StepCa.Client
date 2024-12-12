# StepCa.Client

Minial client for native [step-ca](https://github.com/smallstep/certificates) api, with features:

* Load CA from step-ca server
* Load cert for application from step-ca server
* Autoupdate cert for application
* AOT ready

## Start example

Run compose file:

```bash
docker compose -f ./docker/compose.yaml up --build
```

Open <https://localhost/8080>

Also you can get current fingerprint with command:

```bash
docker compose  -f ./docker/compose.yaml run step-ca step certificate fingerprint /home/certs/ca.crt
```

## Sample project

You can find sample project [in](./Sample/Sample.csproj).
