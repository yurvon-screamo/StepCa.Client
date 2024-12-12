using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using IdentityModel;

using Jose;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using StepCaClient;

namespace StepCa.Client;

public static class StepCaConfigure
{
    private static readonly object s_updating = new();

    public static async Task<CertificatePair> ConfigureStepCaHotreload(this WebApplicationBuilder builder, StepCaOption stepCaOption)
    {
        await LoadCaAsync(stepCaOption);

        CertificatePair certificatePair = await stepCaOption.LoadCertsAsync();

        builder.WebHost.ConfigureKestrel(c => c.ConfigureHttpsDefaults(c => c.ServerCertificateSelector = (c, _) =>
        {
            bool update = certificatePair.HostChain.NotAfter < DateTime.Now;

            if (update)
            {
                lock (s_updating)
                {
                    if (certificatePair.HostChain.NotAfter < DateTime.Now)
                    {
                        X509Certificate2 oldData = certificatePair.HostChain;
                        certificatePair.HostChain = stepCaOption.LoadCertsAsync().GetAwaiter().GetResult().HostChain;
                        oldData.Dispose();
                    }
                }
            }

            return certificatePair.HostChain;
        }));

        return certificatePair;
    }

    public static async Task LoadCaAsync(this StepCaOption stepCaOption)
    {
        HttpClient client = new(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (_, cert, cetChain, err) => true
        });

        string caUri = $"https://{stepCaOption.StepCaHost}:{stepCaOption.StepCaPort}/root/{stepCaOption.Fingerprint}";

        HttpResponseMessage httpResponseMessage = await client.GetAsync(caUri);

        if (httpResponseMessage.StatusCode is not System.Net.HttpStatusCode.OK)
        {
            string err = await httpResponseMessage.Content.ReadAsStringAsync();

            throw new StepCaConfigureException(err);
        }

        CaJson data = await httpResponseMessage.Content.ReadFromJsonAsync(SourceGenerationContext.Default.CaJson)
            ?? throw new StepCaConfigureException("root request return null");

        using X509Store store = new(StoreName.Root, StoreLocation.CurrentUser, OpenFlags.ReadWrite);

        X509Certificate2 certificate = new(Encoding.UTF8.GetBytes(data.Ca), (string?)null, X509KeyStorageFlags.MachineKeySet);

        store.Add(certificate);
    }

    public static async Task<CertificatePair> LoadCertsAsync(this StepCaOption option)
    {
        string baseAddress = $"https://{option.StepCaHost}:{option.StepCaPort}";

        using HttpClient client = new();

        HttpResponseMessage provisioners = await client.GetAsync(baseAddress + "/provisioners");

        ProvisionerList? provisionerList = await provisioners.Content.ReadFromJsonAsync(SourceGenerationContext.Default.ProvisionerList)
            ?? throw new StepCaConfigureException("ProvisionerList is null");

        Provisioner provisioner = provisionerList.Provisioners.Single(c => c.Name == option.StepCaProvisioner);

        JWT.DefaultSettings.JsonMapper = new AotJsonMapper();

        JWT.DefaultSettings.RegisterJwa(
            JweAlgorithm.PBES2_HS256_A128KW,
            new Pbse2HmacShaKeyManagementWithAesKeyWrap(128, new AesKeyWrapManagement(128), 700000, 10000));

        string jwtDataJson = JWT.Decode(provisioner.EncryptedKey, option.StepCaProvisionerPassword);

        JsonWebKey jsonWebKey = new(jwtDataJson);

        JwtPayload payload = new()
        {
            { "sans", new string[] { option.Domain } },
            { "sha", option.Fingerprint },
            { JwtClaimTypes.JwtId, Guid.NewGuid().ToString() },
            { JwtClaimTypes.Audience, baseAddress + "/1.0/sign" },
            { JwtClaimTypes.Subject, option.Domain },
            { JwtClaimTypes.Issuer, option.StepCaProvisioner },
            { JwtClaimTypes.IssuedAt, DateTimeOffset.Now.ToUnixTimeSeconds() },
            { JwtClaimTypes.NotBefore, DateTimeOffset.Now.ToUnixTimeSeconds() },
            { JwtClaimTypes.Expiration, DateTimeOffset.Now.AddMinutes(30).ToUnixTimeSeconds() },
        };

        SigningCredentials credentials = new(jsonWebKey, SecurityAlgorithms.EcdsaSha256);
        JwtHeader header = new(credentials);
        JwtSecurityToken token = new(header, payload);
        JwtSecurityTokenHandler handler = new();

        string tokenString = handler.WriteToken(token);

        using ECDsa eCDsa = ECDsa.Create();
        eCDsa.GenerateKey(ECCurve.NamedCurves.nistP256);

        X500DistinguishedNameBuilder x500DistinguishedName = new();
        x500DistinguishedName.AddCommonName(option.Domain);
        X500DistinguishedName distinguishedName = x500DistinguishedName.Build();

        CertificateRequest certificateRequest = new(distinguishedName, eCDsa, HashAlgorithmName.SHA256);

        SignRequest request = new(certificateRequest.CreateSigningRequestPem(), tokenString);

        Root root;

        using (HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync(
            baseAddress + "/sign",
            request,
            SourceGenerationContext.Default.SignRequest))
        {
            root = await httpResponseMessage.Content.ReadFromJsonAsync(SourceGenerationContext.Default.Root)
                ?? throw new StepCaConfigureException("Root is null");
        }

        string key = eCDsa.ExportECPrivateKeyPem();

        X509Certificate2 ca = new(Encoding.UTF8.GetBytes(root.Ca), (string?)null, X509KeyStorageFlags.MachineKeySet);

        using (X509Store store = new(StoreName.Root, StoreLocation.CurrentUser, OpenFlags.ReadWrite))
        {
            store.Add(ca);
        }

        X509Certificate2 x509Certificate2 = X509Certificate2.CreateFromPem(root.Crt + root.Ca, key);

        return new(ca, x509Certificate2);
    }
}
