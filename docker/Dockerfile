FROM smallstep/step-ca:0.25.2

USER root
COPY run.sh /usr/bin/run.sh

RUN mv /entrypoint.sh /usr/bin/entrypoint.sh && \
    chmod 0005 /usr/bin/run.sh && \
    chmod 0005 /usr/bin/entrypoint.sh && \
    mkdir /home/step/secrets && \
    chmod 0073 -R /home/step/secrets

USER step

ENTRYPOINT ["/bin/bash", "run.sh"]
CMD ["/bin/sh", "-c", "exec /usr/local/bin/step-ca --password-file $PWDPATH $CONFIGPATH"]
