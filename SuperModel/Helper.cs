using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuperModel
{
    public class Helper
    {
        public static string PROJECT_ROOT { get; set; }
        public static async Task<byte[]> GetBytesFromChunkedFileAsync(string filePath)
        {
            byte[] fileBytes;
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
            }
            return fileBytes;
        }
        public static string GetJsonAsString(string jsonPath)
        {
            var result = JsonConvert.DeserializeObject<string>(File.ReadAllText(jsonPath));
            return string.IsNullOrEmpty(result) ? result : null;
        }

        public static TResult GetFileFromJsonString<TResult>(string deserializedJson)
        {
            TResult json = JsonConvert.DeserializeObject<TResult>(deserializedJson);
            if (json != null)
            {
                return json;
            }
            else
            {
                return default(TResult);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="model"></param>
        public static void GetAndStoreConfigFiles(string path, ConfigModel model)
        {
            try
            {
                SaveBytesToFile(path, model.JsonConfigFile, true);
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();
            }

        }
        /// <summary>
        /// UnZipPlugin , unzipping files into the same name folder
        /// Need to use in locked context
        /// </summary>
        /// <param name="pluginFullName">Plugin fullpath with extension</param>
        /// <param name="pluginShortName">Plugin name, creating folder for unzipped file (PROJECT_ROOT + "/" + pluginShortName)</param>
        public static bool UnZipPlugin(string pluginFullName, string pluginShortName)
        {
            try
            {
                string extractPath = PROJECT_ROOT + "/" + pluginShortName;
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                    Directory.CreateDirectory(extractPath);
                }
                else
                {
                    Directory.CreateDirectory(extractPath);
                }
                // System.IO.Compression.ZipFile.CreateFromDirectory(PROJECT_ROOT, PROJECT_ROOT+"/"+pluginFullName);
                System.IO.Compression.ZipFile.ExtractToDirectory(pluginFullName, extractPath);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Move Zipped file to selected folder
        /// </summary>
        /// <param name="pluginName">Zipped plugin fullpath</param>
        /// <param name="destination"> Destination folder path</param>
        public static void MoveFilesTo(string pluginName, string destination)
        {
            if (File.Exists(destination))
                File.Delete(destination);
            Directory.Move(pluginName, destination);
        }
        public static bool SaveBytesToFile(string filename, byte[] bytesToWrite)
        {
            if (string.IsNullOrEmpty(filename) || bytesToWrite == null) return false;
            try
            {
                using (FileStream file = File.Create(PROJECT_ROOT + "/" + filename))
                {
                    file.Write(bytesToWrite, 0, bytesToWrite.Length);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(e);
                Console.ResetColor();
                return false;
            }


        }
        public static bool SaveBytesToFile(string filename, byte[] bytesToWrite, bool IsFullPath)
        {
            if (string.IsNullOrEmpty(filename) || bytesToWrite == null) return false;
            try
            {
                using (FileStream file = File.Create(filename))
                {
                    file.Write(bytesToWrite, 0, bytesToWrite.Length);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(e);
                Console.ResetColor();
                return false;
            }


        }
        public static bool SaveBytesToFile(string filename, string path, byte[] bytesToWrite)
        {
            if (string.IsNullOrEmpty(filename) || bytesToWrite == null) return false;
            try
            {
                using (FileStream file = File.Create(path))
                {
                    file.Write(bytesToWrite, 0, bytesToWrite.Length);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(e);
                Console.ResetColor();
                return false;
            }


        }
    }
    public class Utils
    {
        public string FileName { get; set; }
        public string TempFolder { get; set; }
        public int MaxFileSizeKB { get; set; }
        public List<String> FileParts { get; set; }

        public Utils()
        {
            FileParts = new List<string>();
        }

        /// <summary>
        /// Split = get number of files 
        /// .. Name = original name + ".part_N.X" (N = file part number, X = total files)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SplitFile()
        {
            // improvement - make more robust
            bool rslt = false;
            string BaseFileName = Path.GetFileName(FileName);
            int BufferChunkSize = MaxFileSizeKB * 1024;
            const int READBUFFER_SIZE = 1024;
            byte[] FSBuffer = new byte[READBUFFER_SIZE];
            // adapted from: http://stackoverflow.com/questions/3967541/how-to-split-large-files-efficiently
            using (FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int TotalFileParts = 0;
                if (FS.Length < BufferChunkSize)
                {
                    TotalFileParts = 1;
                }
                else
                {
                    float PreciseFileParts = ((float)FS.Length / (float)BufferChunkSize);
                    TotalFileParts = (int)Math.Ceiling(PreciseFileParts);
                }

                int FilePartCount = 0;
                while (FS.Position < FS.Length)
                {
                    string FilePartName = $"{BaseFileName}.part_{(FilePartCount + 1).ToString()}.{TotalFileParts.ToString()}";
                    FilePartName = Path.Combine(TempFolder, FilePartName);
                    FileParts.Add(FilePartName);
                    using (FileStream FilePart = new FileStream(FilePartName, FileMode.Create))
                    {
                        int bytesRemaining = BufferChunkSize;
                        int bytesRead = 0;
                        while (bytesRemaining > 0 && (bytesRead = FS.Read(FSBuffer, 0, Math.Min(bytesRemaining, READBUFFER_SIZE))) > 0)
                        {
                           await FilePart.WriteAsync(FSBuffer, 0, bytesRead);
                            bytesRemaining -= bytesRead;
                        }
                    }
                    FilePartCount++;
                }

            }
            return rslt;
        }
        public async Task<bool> MergeFile(string FileName)
        {
            bool rslt = false;
            // parse out the different tokens from the filename according to the convention
            string partToken = ".part_";
            string baseFileName = FileName.Substring(0, FileName.IndexOf(partToken));
            string trailingTokens = FileName.Substring(FileName.IndexOf(partToken) + partToken.Length);
            int FileIndex = 0;
            int FileCount = 0;
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".") + 1), out FileCount);
            // get a list of all file parts in the temp folder
            string Searchpattern = Path.GetFileName(baseFileName) + partToken + "*";
            string[] FilesList = Directory.GetFiles(Path.GetDirectoryName(FileName), Searchpattern);
            //  merge .. improvement would be to confirm individual parts are there / correctly in sequence, a security check would also be important
            // only proceed if we have received all the file chunks
            if (FilesList.Count() == FileCount)
            {
                // use a singleton to stop overlapping processes
                if (!MergeFileManager.Instance.InUse(baseFileName))
                {
                    MergeFileManager.Instance.AddFile(baseFileName);
                    if (File.Exists(baseFileName))
                        File.Delete(baseFileName);
                    // add each file located to a list so we can get them into 
                    // the correct order for rebuilding the file
                    List<SortedFile> MergeList = new List<SortedFile>();
                    foreach (string File in FilesList)
                    {
                        SortedFile sFile = new SortedFile();
                        sFile.FileName = File;
                        baseFileName = File.Substring(0, File.IndexOf(partToken));
                        trailingTokens = File.Substring(File.IndexOf(partToken) + partToken.Length);
                        int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
                        sFile.FileOrder = FileIndex;
                        MergeList.Add(sFile);
                    }
                    // sort by the file-part number to ensure we merge back in the correct order
                    var MergeOrder = MergeList.OrderBy(s => s.FileOrder).ToList();
                    using (FileStream FS = new FileStream(baseFileName, FileMode.Create))
                    {
                        // merge each file chunk back into one contiguous file stream
                        foreach (var chunk in MergeOrder)
                        {
                            try
                            {
                                using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                                {
                                   await fileChunk.CopyToAsync(FS);
                                }
                            }
                            catch (IOException ex)
                            {
                                // handle                                
                            }
                        }
                    }
                    rslt = true;
                    // unlock the file from singleton
                    MergeFileManager.Instance.RemoveFile(baseFileName);
                }
            }
            return rslt;
        }

    }

    public struct SortedFile
    {
        public int FileOrder { get; set; }
        public String FileName { get; set; }
    }
    public class MergeFileManager
    {
        private static MergeFileManager instance;
        private List<string> MergeFileList;

        private MergeFileManager()
        {
            try
            {
                MergeFileList = new List<string>();
            }
            catch { }
        }

        public static MergeFileManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MergeFileManager();
                return instance;
            }
        }

        public void AddFile(string BaseFileName)
        {
            MergeFileList.Add(BaseFileName);
        }

        public bool InUse(string BaseFileName)
        {
            return MergeFileList.Contains(BaseFileName);
        }

        public bool RemoveFile(string BaseFileName)
        {
            return MergeFileList.Remove(BaseFileName);
        }
    }
}

