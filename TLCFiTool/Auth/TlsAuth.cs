using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace TLCFiTool.Auth;

public static class TlsAuth
{
    public static bool ValidateServerCertificate(bool allowSelfSigned, object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
    {
        if (errors == SslPolicyErrors.None)
        {
            return true;
        }

        if (allowSelfSigned && certificate is not null)
        {
            return errors == SslPolicyErrors.RemoteCertificateChainErrors;
        }

        return false;
    }

    public static bool IsClientCertificateAllowed(X509Certificate2? certificate, IReadOnlyCollection<string> allowedThumbprints)
    {
        if (certificate is null)
        {
            return false;
        }

        if (allowedThumbprints.Count == 0)
        {
            return true;
        }

        return allowedThumbprints.Contains(certificate.Thumbprint ?? string.Empty, StringComparer.OrdinalIgnoreCase);
    }
}
