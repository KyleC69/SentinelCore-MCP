// Solution: SentinelCore
// Project:   SentinelCore.Orchestrations
// File:         CertificateStoreReadTool.cs
// Author: Kyle L. Crowder
// Build Num:  080801



using ModelContextProtocol.Server;

using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;




namespace SentinelCoreMCP.Tools;





/// <summary>
///     Read-only tool for querying Windows certificate stores.
/// </summary>
[McpServerToolType]
public sealed class CertificateStoreReadTool
{








    [McpServerTool(Name = "Certificate_List", ReadOnly = true, Destructive = false)]
    [Description("Lists certificates in the specified store and location.")]
    public static ToolResult CertificateList([Description("The store name, e.g. My, Root, TrustedPublisher.")] string storeName, [Description("The store location: CurrentUser or LocalMachine. Defaults to LocalMachine.")] StoreLocation location = StoreLocation.LocalMachine, [Description("Maximum number of certificates to return. Defaults to 50.")] int maxRecords = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                return ToolResult.Fail("storeName is required.");
            }

            StringBuilder sb = new();
            int count = 0;
            using X509Store store = new(storeName, location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            foreach (X509Certificate2 cert in store.Certificates)
            {
                if (count >= maxRecords)
                {
                    break;
                }

                sb.AppendLine($"Subject={cert.Subject}, Issuer={cert.Issuer}, Thumbprint={cert.Thumbprint}, NotAfter={cert.NotAfter}, FriendlyName={cert.FriendlyName}");
                count++;
            }

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Certificate store listing failed.");
        }
    }








    [McpServerTool(Name = "Certificate_Read", ReadOnly = true, Destructive = false)]
    [Description("Reads details of a specific certificate by thumbprint.")]
    public static ToolResult CertificateRead([Description("The certificate thumbprint.")] string thumbprint, [Description("The store name, e.g. My, Root.")] string storeName, [Description("The store location: CurrentUser or LocalMachine. Defaults to LocalMachine.")] StoreLocation location = StoreLocation.LocalMachine)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(thumbprint) || string.IsNullOrWhiteSpace(storeName))
            {
                return ToolResult.Fail("thumbprint and storeName are required.");
            }

            using X509Store store = new(storeName, location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2? cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false).FirstOrDefault();
            if (cert is null)
            {
                return ToolResult.Fail($"Certificate not found: {thumbprint} in {location}\\{storeName}");
            }

            StringBuilder sb = new();
            sb.AppendLine($"Subject={cert.Subject}");
            sb.AppendLine($"Issuer={cert.Issuer}");
            sb.AppendLine($"Thumbprint={cert.Thumbprint}");
            sb.AppendLine($"NotBefore={cert.NotBefore}");
            sb.AppendLine($"NotAfter={cert.NotAfter}");
            sb.AppendLine($"HasPrivateKey={cert.HasPrivateKey}");
            sb.AppendLine($"FriendlyName={cert.FriendlyName}");
            sb.AppendLine($"SerialNumber={cert.SerialNumber}");
            sb.AppendLine($"SignatureAlgorithm={cert.SignatureAlgorithm.FriendlyName}");
            sb.AppendLine($"Version={cert.Version}");

            return ToolResult.Ok(sb.ToString());
        }
        catch
        {
            return ToolResult.Fail("Certificate read failed.");
        }
    }
}
