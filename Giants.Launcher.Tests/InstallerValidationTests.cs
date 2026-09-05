using System;
using System.Security.Cryptography;
using System.Text;
using Giants.WebApi.Clients;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Giants.Launcher.Tests
{
    [TestClass]
    public class InstallerValidationTests
    {
        [TestMethod]
        public void InstallerPathBuildsTrustedUri()
        {
            VersionInfo versionInfo = CreateVersionInfo();
            versionInfo.InstallerPath = "GPatch_1_520_78_0.exe";

            bool isTrusted = InstallerValidation.TryGetTrustedInstallerUri(versionInfo, out Uri installerUri);

            Assert.IsTrue(isTrusted);
            Assert.AreEqual(
                "https://giants.blob.core.windows.net/public/GPatch_1_520_78_0.exe",
                installerUri.AbsoluteUri);
        }

        [TestMethod]
        public void LegacyTrustedUriRemainsSupported()
        {
            VersionInfo versionInfo = CreateVersionInfo();
            versionInfo.InstallerUri = new Uri(
                "https://giants.blob.core.windows.net/public/GPatch_1_520_78_0.exe");

            bool isTrusted = InstallerValidation.TryGetTrustedInstallerUri(versionInfo, out Uri installerUri);

            Assert.IsTrue(isTrusted);
            Assert.AreEqual(versionInfo.InstallerUri, installerUri);
        }

        [TestMethod]
        public void ValidInstallerPathTakesPrecedenceOverLegacyUri()
        {
            VersionInfo versionInfo = CreateVersionInfo();
            versionInfo.InstallerPath = "GPatch_1_520_78_0.exe";
            versionInfo.InstallerUri = new Uri("https://attacker.example/payload.exe");

            bool isTrusted = InstallerValidation.TryGetTrustedInstallerUri(versionInfo, out Uri installerUri);

            Assert.IsTrue(isTrusted);
            Assert.AreEqual(
                "https://giants.blob.core.windows.net/public/GPatch_1_520_78_0.exe",
                installerUri.AbsoluteUri);
        }

        [DataTestMethod]
        [DataRow("https://attacker.example/GPatch_1_520_78_0.exe")]
        [DataRow("https://giants.blob.core.windows.net/private/GPatch_1_520_78_0.exe")]
        [DataRow("https://giants.blob.core.windows.net/public/GPatch_1_520_78_0.exe?redirect=1")]
        [DataRow("https://giants.blob.core.windows.net/public/GPatch_1_520_79_0.exe")]
        public void UntrustedUriIsRejected(string installerUriText)
        {
            VersionInfo versionInfo = CreateVersionInfo();
            versionInfo.InstallerUri = new Uri(installerUriText);

            bool isTrusted = InstallerValidation.TryGetTrustedInstallerUri(versionInfo, out _);

            Assert.IsFalse(isTrusted);
        }

        [TestMethod]
        public void MatchingSha256IsAcceptedCaseInsensitively()
        {
            byte[] actualSha256 = new byte[32];
            actualSha256[0] = 0x29;
            actualSha256[31] = 0xEF;

            Assert.IsTrue(
                InstallerValidation.IsMatchingSha256(
                    "29000000000000000000000000000000000000000000000000000000000000EF",
                    actualSha256));
        }

        [TestMethod]
        public void MismatchedSha256IsRejected()
        {
            byte[] actualSha256 = new byte[32];
            actualSha256[0] = 0x29;
            actualSha256[31] = 0xEF;

            Assert.IsFalse(
                InstallerValidation.IsMatchingSha256(
                    "29000000000000000000000000000000000000000000000000000000000000EA",
                    actualSha256));
        }

        [TestMethod]
        public void CanonicalPayloadIsStable()
        {
            VersionInfo versionInfo = CreateSignedVersionInfo();

            Assert.AreEqual(
                "Giants.Update.v1\nR2lhbnRz\nVGVzdA\n1\n520\n78\n0\nR1BhdGNoXzFfNTIwXzc4XzAuZXhl\n29dcba5dd75753d36f4c1eca419649c09d81975aeb91809112a395acd2e67efa",
                InstallerSignatureValidation.CreateCanonicalPayload(versionInfo));
        }

        [TestMethod]
        public void ValidInstallerSignatureIsAccepted()
        {
            Assert.IsTrue(InstallerSignatureValidation.IsTrusted(CreateSignedVersionInfo()));
        }

        [TestMethod]
        public void TamperedSignedMetadataIsRejected()
        {
            VersionInfo versionInfo = CreateSignedVersionInfo();
            versionInfo.InstallerPath = "GPatch_1_520_79_0.exe";

            Assert.IsFalse(InstallerSignatureValidation.IsTrusted(versionInfo));
        }

        [TestMethod]
        public void WrongKeySignatureIsRejected()
        {
            VersionInfo versionInfo = CreateSignedVersionInfo();
            using (var wrongKey = new RSACryptoServiceProvider(2048))
            {
                byte[] signature = wrongKey.SignData(
                    Encoding.UTF8.GetBytes(InstallerSignatureValidation.CreateCanonicalPayload(versionInfo)),
                    CryptoConfig.MapNameToOID("SHA256"));
                versionInfo.InstallerSignature = Convert.ToBase64String(signature)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }

            Assert.IsFalse(InstallerSignatureValidation.IsTrusted(versionInfo));
        }

        [TestMethod]
        public void MalformedInstallerSignatureIsRejected()
        {
            VersionInfo versionInfo = CreateSignedVersionInfo();
            versionInfo.InstallerSignature = "not-a-signature";

            Assert.IsFalse(InstallerSignatureValidation.IsTrusted(versionInfo));
        }

        [TestMethod]
        public void UnsignedLegacyMetadataRemainsSupported()
        {
            VersionInfo versionInfo = CreateVersionInfo();
            versionInfo.InstallerUri = new Uri(
                "https://giants.blob.core.windows.net/public/GPatch_1_520_78_0.exe");

            Assert.IsTrue(InstallerSignatureValidation.IsTrusted(versionInfo));
        }

        private static VersionInfo CreateVersionInfo()
        {
            return new VersionInfo()
            {
                AppName = "Giants",
                BranchName = "Test",
                Version = new AppVersion()
                {
                    Major = 1,
                    Minor = 520,
                    Build = 78,
                    Revision = 0,
                },
            };
        }

        private static VersionInfo CreateSignedVersionInfo()
        {
            VersionInfo versionInfo = CreateVersionInfo();
            versionInfo.InstallerPath = "GPatch_1_520_78_0.exe";
            versionInfo.InstallerSha256 = "29dcba5dd75753d36f4c1eca419649c09d81975aeb91809112a395acd2e67efa";
            versionInfo.InstallerSignature = "k0QRwkGIxRJluCtW107iszZEd6cMjv2065CkqkEOQ0dy05uEJ95HgqhePDwti8dD3381ZRQ6TqJryxwGcQK8NrUtCx6f5dZe7-wNEuixL31G3NaAN0HEiGmvZoiU-gfo-XAek92Wf1yDEtUS0zkZWBnZs_XMOvTmBYdZI3d3GE3z-EO-Yaqliw-Ds8QhXOLjW7bftgYx7v5cLe0PlncYesEIkVNP0tXe2e2iaalxic3bX-_CwYCDZIn3wVuYsWS-6RGZ4A1Amesqvfxj3ra2ogd4BC6wklU1GdXHtnudl1TjnWuwwBg6F1iMBDel_LNbxaAqV30Knz58s_YbIvtv8w";
            return versionInfo;
        }
    }
}
