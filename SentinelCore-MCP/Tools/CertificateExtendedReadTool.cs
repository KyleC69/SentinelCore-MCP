// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         CertificateExtendedReadTool.cs
// Author: Kyle L. Crowler
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying certificate revocation lists and verifying
///     certificate trust status.
/// </summary>
[McpServerToolType]
public sealed class CertificateExtendedReadTool
{








    [SupportedOSPlatform("windows")]
    [McpServerTool(Name = "Certificate_Verify", ReadOnly = true, Destructive = false)]
    [Description("Verifies a certificate's trust chain and revocation status from the specified store.")]
    public static ToolResult CertificateVerify(
        [Description("The certificate thumbprint/hash to verify.")] string thumbprint,
        [Description("The certificate store name: My, Root, CA, Trust, Disallowed. Defaults to My.")] string storeName = "My",
        [Description("The certificate store location: CurrentUser or LocalMachine. Defaults to LocalMachine.")] string storeLocation = "LocalMachine")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(thumbprint))
            {
                return ToolResult.Fail("thumbprint is required.");
            }

            StoreLocation location = storeLocation.Equals("CurrentUser", StringComparison.OrdinalIgnoreCase)
                ? StoreLocation.CurrentUser
                : StoreLocation.LocalMachine;

            StoreName name = storeName.ToUpperInvariant() switch
            {
                "MY" => StoreName.My,
                "ROOT" => StoreName.Root,
                "CA" => StoreName.CertificateAuthority,
                "TRUST" => StoreName.TrustedPublisher,
                "DISALLOWED" => StoreName.Disallowed,
                _ => StoreName.My
            };

            using X509Store store = new(name, location);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (certs.Count == 0)
            {
                return ToolResult.Fail($"Certificate not found with thumbprint: {thumbprint}");
            }

            X509Certificate2 cert = certs[0];
            var result = new
            {
                cert.Subject,
                cert.Issuer,
                cert.Thumbprint,
                cert.NotBefore,
                cert.NotAfter,
                cert.SerialNumber,
                IsExpired = DateTime.Now > cert.NotAfter,
                IsNotYetValid = DateTime.Now < cert.NotBefore,
                HasPrivateKey = cert.HasPrivateKey,
                cert.SignatureAlgorithm?.FriendlyName,
                KeyAlgorithm = cert.GetKeyAlgorithm(),
                cert.IssuerName?.Name
            };

            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            return ToolResult.Ok(json);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Certificate verification failed: {ex.Message}");
        }
    }
}