// EncryptionAlgorithm.cs, defines the EncryptionAgorithm enum supported by ZIP

namespace CipherBox.Zip.Encryption
{
    /// <summary>
    /// An enum that provides the various encryption algorithms supported by this
    /// library.
    /// </summary>
    ///
    /// <remarks>
    ///
    /// <para>
    ///   <c>PkzipWeak</c> implies the use of Zip 2.0 encryption, which is known to be
    ///   weak and subvertible.
    /// </para>
    ///
    /// <para>
    ///   A note on interoperability: Values of <c>PkzipWeak</c> and <c>None</c> are
    ///   specified in <see
    ///   href="http://www.pkware.com/documents/casestudies/APPNOTE.TXT">PKWARE's zip
    ///   specification</see>, and are considered to be "standard".  Zip archives
    ///   produced using these options will be interoperable with many other zip tools
    ///   and libraries, including Windows Explorer.
    /// </para>
    ///
    /// <para>
    ///   Values of <c>WinZipAes128</c> and <c>WinZipAes256</c> are not part of the Zip
    ///   specification, but rather imply the use of a vendor-specific extension from
    ///   WinZip. If you want to produce interoperable Zip archives, do not use these
    ///   values.  For example, if you produce a zip archive using WinZipAes256, you
    ///   will be able to open it in Windows Explorer on Windows XP and Vista, but you
    ///   will not be able to extract entries; trying this will lead to an "unspecified
    ///   error". For this reason, some people have said that a zip archive that uses
    ///   WinZip's AES encryption is not actually a zip archive at all.  A zip archive
    ///   produced this way will be readable with the WinZip tool (Version 11 and
    ///   beyond).
    /// </para>
    ///
    /// <para>
    ///   In case you care: According to <see
    ///   href="http://www.winzip.com/aes_info.htm">the WinZip specification</see>, the
    ///   actual AES key used is derived from the <see cref="ZipEntry.Password"/> via an
    ///   algorithm that complies with <see
    ///   href="http://www.ietf.org/rfc/rfc2898.txt">RFC 2898</see>, using an iteration
    ///   count of 1000.  The algorithm is sometimes referred to as PBKDF2, which stands
    ///   for "Password Based Key Derivation Function #2".
    /// </para>
    /// </remarks>
    public enum EncryptionAlgorithm
    {
        /// <summary>
        /// No encryption at all.
        /// </summary>
        None = 0,

        /// <summary>
        /// Traditional or Classic pkzip encryption.
        /// </summary>
        PkzipWeak,

        /// <summary>
        /// WinZip AES encryption (128 key bits).
        /// </summary>
        WinZipAes128,

        /// <summary>
        /// WinZip AES encryption (256 key bits).
        /// </summary>
        WinZipAes256,

        /// <summary>
        /// An encryption algorithm that is not supported by DotNetZip.
        /// </summary>
        Unsupported = 4,
    }

    internal enum CryptoMode
    {
        Encrypt,
        Decrypt
    }

}
