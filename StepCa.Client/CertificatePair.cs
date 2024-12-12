using System.Security.Cryptography.X509Certificates;

namespace StepCa.Client;

public class CertificatePair
{
    public CertificatePair(X509Certificate2 ca, X509Certificate2 hostChain)
    {
        Ca = ca;
        HostChain = hostChain;
    }

    public X509Certificate2 Ca { get; init; }
    public X509Certificate2 HostChain { get; internal set; }
}
