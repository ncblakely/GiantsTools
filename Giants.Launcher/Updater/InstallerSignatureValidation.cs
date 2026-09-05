using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Giants.WebApi.Clients;

namespace Giants.Launcher
{
    internal static class InstallerSignatureValidation
    {
        private const string SignatureVersion = "Giants.Update.v1";

        // This is the public modulus for giants-updater-signing-v1. The private key remains in Azure Key Vault.
        private const string SigningKeyModulus =
            "2FBm9lwAJTQpg6CTEMgbV22uNdzubdT8FUiqfiDevYmkAkDH8j27fxePWxwRE2sX9qePtwnWhlzomJ+vbCL/U5sRDQRyBw/nmKKPaRnFihKQ"
            + "pFx3qpUxowe77mB/2lzFmCWK9f+wv6iE7C80o2ht3BOvzrh1Fh9dK8TXVT6LbY9G4ROhzOJ7OB9rj2nJgikTmBDmzoKKfXUcscuANzolVOeK"
            + "MkI9ddF0kpDMPUXX5FTb7sXVOKwUGhA8Gt3b7YD+jwcn/wYgHeMCVIFbCLtrwRZSmwR5IHOCauRGSaTaj+eupsI9WiGKgTdaSCAaGRxvIALy"
            + "N3th+8Oj1jv64WWFHQ==";
        private static readonly byte[] SigningKeyExponent = { 1, 0, 1 };

        internal static bool IsTrusted(VersionInfo versionInfo)
        {
            if (versionInfo == null || string.IsNullOrEmpty(versionInfo.InstallerSignature))
            {
                return true;
            }

            if (versionInfo.Version == null
                || !InstallerValidation.IsValidInstallerFileName(versionInfo.InstallerPath, versionInfo.Version)
                || !InstallerValidation.IsValidSha256(versionInfo.InstallerSha256)
                || !TryDecodeBase64Url(versionInfo.InstallerSignature, out byte[] signature)
                || signature.Length != 256)
            {
                return false;
            }

            byte[] payload = Encoding.UTF8.GetBytes(CreateCanonicalPayload(versionInfo));
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(new RSAParameters()
                {
                    Modulus = Convert.FromBase64String(SigningKeyModulus),
                    Exponent = SigningKeyExponent,
                });

                return rsa.VerifyData(
                    payload,
                    CryptoConfig.MapNameToOID("SHA256"),
                    signature);
            }
        }

        internal static string CreateCanonicalPayload(VersionInfo versionInfo)
        {
            return string.Join(
                "\n",
                SignatureVersion,
                EncodeField(versionInfo.AppName),
                EncodeField(versionInfo.BranchName),
                versionInfo.Version.Major.ToString(CultureInfo.InvariantCulture),
                versionInfo.Version.Minor.ToString(CultureInfo.InvariantCulture),
                versionInfo.Version.Build.ToString(CultureInfo.InvariantCulture),
                versionInfo.Version.Revision.ToString(CultureInfo.InvariantCulture),
                EncodeField(versionInfo.InstallerPath),
                versionInfo.InstallerSha256.ToLowerInvariant());
        }

        private static string EncodeField(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static bool TryDecodeBase64Url(string value, out byte[] bytes)
        {
            bytes = null;
            if (string.IsNullOrEmpty(value) || value.Length % 4 == 1)
            {
                return false;
            }

            foreach (char character in value)
            {
                if (!((character >= 'A' && character <= 'Z')
                    || (character >= 'a' && character <= 'z')
                    || (character >= '0' && character <= '9')
                    || character == '-'
                    || character == '_'))
                {
                    return false;
                }
            }

            try
            {
                string paddedValue = value
                    .Replace('-', '+')
                    .Replace('_', '/');
                paddedValue += new string('=', (4 - paddedValue.Length % 4) % 4);
                bytes = Convert.FromBase64String(paddedValue);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
