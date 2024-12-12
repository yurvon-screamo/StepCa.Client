#! /bin/bash -e

# if [ ! -f /home/step/secrets/intermediate_ca_key ]; then

    step ca init \
        --acme \
        --remote-management \
        --root=/home/certs/ca.crt \
        --key=/home/certs/ca.key \
        --password-file=/home/step/secrets/password \
        --deployment-type=standalone \
        --name=sample \
        --dns=${DNS} \
        --address=:443 \
        --provisioner=admin

    config=/home/step/config/ca.json
    source=$(cat $config)

    sourceSubstr='"enableAdmin": true'
    targetSubstr='"enableAdmin":true,"claims":{"minTLSCertDuration":"5m","maxTLSCertDuration":"17520h","defaultTLSCertDuration":"24h"}'
    echo "${source/"$sourceSubstr"/"$targetSubstr"}" > $config

# fi

entrypoint.sh "$@"
