using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// based on DotNetZip
using CipherBox.Zip; 
using CipherBox.Zip.Encryption;

namespace CipherBox.Zip
{
	/// <summary>
	/// password add and remove
	/// </summary>
	public class ZipHelper
	{
        public ZipHelper()
		{
		}

        #region query operations

        // API function: it is in zip file format
        public static bool IsMyFile(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            if (!fi.Exists) return false; // if (!File.Exists(filename)) return false;

            string ext = fi.Extension.ToUpper();
            if (ext != ".ZIP")  // ".7Z", ".RAR", ".TAR", ".ARJ", ".GZIP", ".LZW", ".BZIP2"
                return false;

            if (!ZipFile.IsZipFile(filename))
                return false;

            return true;
        }


        // API function
        public static bool IsEncrypted(string filename)
        {
            if (!IsMyFile(filename))
                return false;  // not zip file

            try
            {
                using (var zip = ZipFile.Read(filename))
                {
                    foreach (ZipEntry e in zip)
                    {
                        // if ( e.UsesEncryption )
                        if (e.Encryption != EncryptionAlgorithm.None) return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unsupported file format:" + ex.Message);
                return false;
            }
        }

        public static string GetEncryptionInfo(string filename)
        {
            if (!IsMyFile(filename)) return "\nNot zip file";

            string result = "List of items:\n";
            try
            {
                using (var zip = ZipFile.Read(filename))
                {
                    // each entry could have different encryption method,while the overall Encryption property shows none.
                    foreach(ZipEntry e in zip)
                    {
                        string estr = "\n" + e.FileName + ":\t";
                        switch (e.Encryption)
                        {
                            case EncryptionAlgorithm.None: estr += "None"; break;
                            case EncryptionAlgorithm.PkzipWeak: estr += "PKZip classic encryption"; break;
                            case EncryptionAlgorithm.WinZipAes128: estr += "Winzip AES128 encryption"; break;
                            case EncryptionAlgorithm.WinZipAes256: estr += "Winzip AES256 encryption"; break;
                            case EncryptionAlgorithm.Unsupported: estr += "Unsupported encryption"; break;
                            default: estr += "Unknown"; break;
                        }

                        result += estr;
                    }
                }
            }
            catch (Exception)
            {
                result += "\nUnsupported file format";
            }

            return result;
        }


        // refer to: ChangePassword()
        public static bool VerifyPassword(string filename, String password)
        {
            if (!IsMyFile(filename)) return false;

            try
            {
                // read all entries from a zip file, and verify each one with password
                using (var zip = ZipFile.Read(filename))
                {
                    foreach (ZipEntry e in zip)
                    {
                        //if (e.IsDirectory) continue; // no check on directory ??

                        // note ZipEntry.OpenReader(pwd) only works for read from ZipFile
                        if (e.OpenReader(password) == null)
                            return false;
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                //Console.WriteLine("VerifyPassword error: " + ex.Message);
                return false;
            }
        }

        #endregion


        #region compress and extract

        public static bool Compress(string src)
        {
            return Compress(src, null); // no password
        }

        public static bool Compress(string src, string password)
        {
            return Compress(src, null, password);  // default destination the same directory as src
        }
        
        /// <summary>
        /// compress a file or folder with a password, and save as zip file
        /// </summary>
        /// <param name="src">source file or folder name</param>
        /// <param name="dest">destination file or folder name</param>
        /// <param name="password">password</param>
        /// <returns>true if succeeds</returns>
        public static bool Compress(string src, string dest, string password)
        {
            // determine whether the src path is file or folder
            FileAttributes attr = File.GetAttributes(src);
            bool srcIsFolder = ((attr & FileAttributes.Directory) == FileAttributes.Directory); // else is file

            if ( srcIsFolder && !Directory.Exists(src) ) return false;
            if ((!srcIsFolder) && !File.Exists(src)) return false;

            if (string.IsNullOrEmpty(dest)) { dest = src + ".zip"; }

            try
            {
                using (ZipFile zip = new ZipFile(dest))
                {
                    /*
                    By default it uses PKZIP encryption, which is pretty weak, but widely supported. 
                    If you double-click the resulting zipfile in Windows Explorer, and then try to 
                    extract the files, Windows will prompt you for the password.

                    If you want stronger encryption, DotNetZip supports AES as well. 
                    This is supported by tools like XCeed and WinZip, but not by Windows Explorer, as far as I know. 
                    You need to add one line of code to right after setting the password
                    */
                    if (!string.IsNullOrEmpty(password))
                    {
                        zip.Password = password;
                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                    }

                    // add one file or folder with relative path
                    //zip.AddItem(src, "");  // ZipFile will determine if it is file or folder
                    if (srcIsFolder)
                    {
                        // add one folder and keep the relative folder name in zip
                        zip.AddDirectory(src, Path.GetFileName(src));
                    }
                    else
                    {
                        // add one single file with relative path
                        zip.AddFile(src, "");  // can add more files
                    }

                    zip.Save();
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Compress error: " + ex.Message);
                return false;
            }

            return true;
        }
        
        
        public static bool Extract(string src)
        {
            return Extract(src, null); // no password required
        }
        
        public static bool Extract(string src, string password)
        {
            return Extract(src, null, password);  // default to the same direcotyr as src
        }
        
        /// <summary>
        /// extract contents from a zip file, and save to given destination folder
        /// </summary>
        /// <param name="src">source file</param>
        /// <param name="dest">destination folder</param>
        /// <param name="password">password</param>
        /// <returns></returns>
        public static bool Extract(string src, string dest, string password)
        {
            try
            {
                if (!IsMyFile(src)) return false;

                // assume destination is a folder name
                string targetFolder = dest;
                if (string.IsNullOrEmpty(dest))
                {
                    targetFolder = Path.GetDirectoryName(src); // Path.GetFileNameWithoutExtension(src);
                }
                else
                {
                    FileAttributes attr = File.GetAttributes(dest);
                    bool destIsFolder = ((attr & FileAttributes.Directory) == FileAttributes.Directory);
                    if (!destIsFolder)
                    {
                        Console.WriteLine("warning: destination is not a folder! extracted to same folder then.");
                        targetFolder = Path.GetDirectoryName(src);
                        //string filename = Path.GetFileName(src);
                    }
                }

                // extract all files in an existing zip that uses encryption
                using (var zip = ZipFile.Read(src))
                {
                    zip.Password = password;
                    zip.ExtractAll(targetFolder);

                    // extract one file from a zip that uses encryption
                    //zip[oneFileNameOfEntry].Extract("TargetFolder");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Extract error: " + ex.Message);
                return false;
            }
        }
        
        #endregion


        #region add, remove, and change password

        // API function: protect a pdf document with a password.
        public static bool AddPassword(string filename, string password)
        {
            return ChangePassword(filename, null, password);
        }

        // API function: unprotect a document (if you know the password)
        public static bool RemovePassword(string filename, string password)
        {
            return ChangePassword(filename, password, null);
        }


        public static bool ChangePassword(string filename, string oldPassword, string newPassword)
        {
            if (!IsMyFile(filename)) return false;

            // init
            Dictionary<string, byte[]> tmp = new Dictionary<string, byte[]>();  // key = entry name

            try
            {
                // read all entries from a zip file, and save in memory temporarily
                using (var zip = new ZipInputStream(filename)) // ZipFile.Read(filename))
                {
                    // must set password before calling read(). so the read data for ZipEntry will
                    // be decrypted and uncompressed
                    zip.Password = oldPassword;

                    ZipEntry e;
                    while ((e = zip.GetNextEntry()) != null)
                    {
                        if (e.IsDirectory)
                        {
                            tmp[e.FileName] = null;
                            continue;  // ??
                        }

                        using (MemoryStream stream = new MemoryStream())
                        {
                            byte[] buffer = new byte[2048];
                            int n;
                            while ((n = zip.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                stream.Write(buffer, 0, n);
                            }

                            // store the decrypted and uncompressed bytes for each entry
                            tmp[e.FileName] = stream.ToArray(); 
                        }
                    }
                }

                // create a new zip file with password, and add all entries into it
                // the original zip file will be overwritten
                //using (FileStream fs raw = File.Open(filename, FileMode.Create, FileAccess.ReadWrite ))
                {
                    using (var zip = new ZipOutputStream(filename))
                    {
                        if (!string.IsNullOrEmpty(newPassword))
                        {
                            zip.Password = newPassword;  // set a same password for all entries
                            zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                        }

                        foreach (var entryName in tmp.Keys)
                        {
                            zip.PutNextEntry(entryName);
                            byte[] buffer = tmp[entryName];
                            if (buffer == null)
                            {
                                // this will be zero length for directory
                            }
                            else
                            {
                                zip.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ChangePassword error: " + ex.Message);
                return false;
            }
        }


        #endregion

        
	}
}
