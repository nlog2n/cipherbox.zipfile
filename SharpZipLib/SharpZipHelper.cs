// example from ICSharpCode.SharpZipLib: create zip file using secure password
// refer to: https://github.com/icsharpcode/SharpZipLib/wiki/Zip-Samples

using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

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

        // API function: it is in zip file format
        public static bool IsMyFile(string TestFile)
        {
            return false;
        }


        // API function
        public static bool IsEncrypted(string filenameDest)
        {
            if (!File.Exists(filenameDest))
                return false;

            if (!IsMyFile(filenameDest))
                return false;  // not zip file

            return false;
        }




        // API function: protect a pdf document with a password.
        public static bool AddPassword(string filenameDest, string password)
        {
            if (!File.Exists(filenameDest))
                return false;

            return true;
        }

        // API function: unprotect a document (if you know the password)
        public static bool RemovePassword(string filenameDest, string password)
        {
            if (!File.Exists(filenameDest))
                return false;



            return true;
        }


        public static bool ChangePassword(string filenameDest, String oldPassword, String newPassword)
        {
            if (!File.Exists(filenameDest))
                return false;

            return true;
        }

        public static bool VerifyPassword(string filenameDest, String password)
        {
            // try open
            if (!File.Exists(filenameDest))
                return false;

            return true;
        }


        
        // using zip library (ICSharpCode.SharpZipLib.Zip)
        public void ProtectFile2(string filename, string pwd)
        {
            string zipFileName;
            string fileExt;

            try
            {
                fileExt = Path.GetExtension(filename);
                zipFileName = filename.Replace(fileExt, ".zip");
                using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFileName)))
                {
                    s.Password = pwd;
                    s.SetLevel(4); // 0 - store only to 9 - means best compression

                    byte[] buffer = new byte[4096];

                    ZipEntry entry = new ZipEntry(Path.GetFileName(filename));
                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(filename))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                    s.Finish();

                    s.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during processing {0}", ex);
            }
        }


        // zip a file with password, and save as oldFileName.zip        
        public static bool ProtectFile(string fileName, string password)
        {
            if (!File.Exists(fileName))
                return false;

            // TODO: check if it is already zip file

            string zipFileName = fileName + ".zip";

            FastZip zip = new ICSharpCode.SharpZipLib.Zip.FastZip();
            zip.Password = password;
            zip.CreateZip(zipFileName, "temp\\", true, null, null); // TODO: set directory

            return true;
        }


        public static bool UnprotectFile(string fileName, string password)
        {
            return false;
        }


        #region compress folder sample from sharpziplib
        // Compresses the files in the nominated folder, and creates a zip file on disk named as outPathname.
        //
        public void CreateSample( string folderName, string outPathname, string password)
        {

            FileStream fsOut = File.Create(outPathname);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);

            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

            zipStream.Password = password;  // optional. Null is the same as not setting. Required if using AES.

            // This setting will strip the leading part of the folder path in the entries, to
            // make the entries relative to the starting folder.
            // To include the full path for each entry up to the drive root, assign folderOffset = 0.
            int folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);

            CompressFolder(folderName, zipStream, folderOffset);

            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        // Recurses down the folder structure
        //
        private void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }


        // Unpack a Zip with full control over the operation
        public void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }



        #endregion



        #region handles zipping or unzipping a folder with all its subfolders and contained files, using SharpZipLib

        public static void ProtectFolder(string inputFolderPath, string outputPathAndFile, string password)
        {
            ArrayList ar = GenerateFileList(inputFolderPath); // generate file list
            int TrimLength = (Directory.GetParent(inputFolderPath)).ToString().Length;
            // find number of chars to remove     // from orginal file path
            TrimLength += 1; //remove '\'
            FileStream ostream;
            byte[] obuffer;
            string outPath = inputFolderPath + @"\" + outputPathAndFile;
            ZipOutputStream oZipStream = new ZipOutputStream(File.Create(outPath)); // create zip stream
            if (password != null && password != String.Empty)
                oZipStream.Password = password;
            oZipStream.SetLevel(9); // maximum compression
            ZipEntry oZipEntry;
            foreach (string Fil in ar) // for each file, generate a zipentry
            {
                oZipEntry = new ZipEntry(Fil.Remove(0, TrimLength));
                oZipStream.PutNextEntry(oZipEntry);

                if (!Fil.EndsWith(@"/")) // if a file ends with '/' its a directory
                {
                    ostream = File.OpenRead(Fil);
                    obuffer = new byte[ostream.Length];
                    ostream.Read(obuffer, 0, obuffer.Length);
                    oZipStream.Write(obuffer, 0, obuffer.Length);
                }
            }
            oZipStream.Finish();
            oZipStream.Close();
        }


        private static ArrayList GenerateFileList(string Dir)
        {
            ArrayList fils = new ArrayList();
            bool Empty = true;
            foreach (string file in Directory.GetFiles(Dir)) // add each file in directory
            {
                fils.Add(file);
                Empty = false;
            }

            if (Empty)
            {
                if (Directory.GetDirectories(Dir).Length == 0)
                // if directory is completely empty, add it
                {
                    fils.Add(Dir + @"/");
                }
            }

            foreach (string dirs in Directory.GetDirectories(Dir)) // recursive
            {
                foreach (object obj in GenerateFileList(dirs))
                {
                    fils.Add(obj);
                }
            }
            return fils; // return file list
        }


        public static void UnZipFiles(string zipPathAndFile, string outputFolder, string password, bool deleteZipFile)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
            if (password != null && password != String.Empty)
                s.Password = password;
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (directoryName != "")
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    if (theEntry.Name.IndexOf(".ini") < 0)
                    {
                        string fullPath = directoryName + "\\" + theEntry.Name;
                        fullPath = fullPath.Replace("\\ ", "\\");
                        string fullDirPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                        FileStream streamWriter = File.Create(fullPath);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();
                    }
                }
            }
            s.Close();
            if (deleteZipFile)
                File.Delete(zipPathAndFile);
        }

        #endregion


    }
}
