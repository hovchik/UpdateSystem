using Microsoft.AspNetCore.SignalR.Client;
using SuperModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace AgainClient
{
    internal class Program
    {
        #region Fields Constants Properties
        private static HubConnection connection;
        private static List<Model> superList = new List<Model>();
        static List<string> pluginsList = new List<string>();
        static Dictionary<string, List<ChunkedData>> chunkedData = new Dictionary<string, List<ChunkedData>>();
        static HttpClient _client = new HttpClient();
        static string PROJECT_ROOT = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
        private static int _count = -1;


        private static string FileExtension = string.Empty;
        private static string PATH_VERSIONING = ConfigurationManager.AppSettings["PATH_VERSIONING"]; //@"C:\Users\hovch\Desktop\DavLong__UpdateProject\AgainClient\Versioning.xml";
        private static PluginVersions pluginVersion;
        private static Dictionary<string, int> PlugnameVersion = new Dictionary<string, int>();
        private static Dictionary<string, string> PlugChunk = new Dictionary<string, string>();
        private static object lockObj = new object();
        private static string argumentName = String.Empty;
        private static TaskScheduler instance;
        private static Dictionary<string, int> countDict = new Dictionary<string, int>();
        public static int IntervalInSeconds { get; private set; } = 15;
        public static bool IsDataGetted { get; private set; }
        private static int PluginsCount = -1;
        private static Dictionary<string, ConfigModel> configDict = new Dictionary<string, ConfigModel>();

        private static string _hubUrl = ConfigurationManager.AppSettings["HubUrl"];
        #endregion
        private static void Main(string[] args)
        {
            #region Construct
            Helper.PROJECT_ROOT = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            Console.WriteLine("Starting Jobs");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl+"/messageHub")
                .Build();
            connection.On<string>("ReceiveMessage", message =>
            {
                Console.WriteLine(message);
            });
            connection.On<List<string>>("ReceiveList", list => { pluginsList.AddRange(list); });
            //connection.On<ConfigModel,int>("ReceiveConfigs", (cModel,count) =>
            //{ http://hovsignal-001-site1.htempurl.com/


            //});
            connection.On<ChunkedData, string, int, int, ConfigModel>("ReceiveChunks", (cd, name, count, itemCount, cModel) =>
             {

                 //_count = count;
                 //pluginNAME = name;
                 //chunkNAME = cd.ChunkName;
                 FileExtension = cd.Extension;
                 PluginsCount = itemCount;
                 Console.WriteLine($"Chunked Data Received: {cd.ChunkName}");
                 if (PlugChunk.Keys.Contains(name))
                 {
                     PlugChunk[name] = cd.ChunkName;
                 }
                 else
                 {
                     PlugChunk.Add(name, cd.ChunkName);
                 }
                 if (PlugnameVersion.Keys.Contains(name))
                 {
                     PlugnameVersion[name] = cd.Version;
                 }
                 else
                 {
                     PlugnameVersion.Add(name, cd.Version);
                 }
                 if (countDict.Keys.Contains(name))
                 {
                     countDict[name] = count;
                 }
                 else
                 {
                     countDict.Add(name, count);
                 }
                 if (chunkedData.Keys.Contains(name))
                 {
                     chunkedData[name].Add(cd);
                 }
                 else
                 {
                     chunkedData.Add(cd.PluginName, new List<ChunkedData>() { cd });
                 }
                 if (configDict.Keys.Contains(cModel.PluginName))
                 {
                     configDict[cModel.PluginName] = cModel;
                 }
                 else
                 {
                     configDict.Add(cModel.PluginName, cModel);
                 }

                 IsDataGetted = true;

             });
            connection.StartAsync().GetAwaiter().GetResult();
            Console.WriteLine(connection.State);
            Console.ForegroundColor = ConsoleColor.Red;

            #endregion Construct

            _client.BaseAddress = new Uri(_hubUrl);

            instance = TaskScheduler.Instance;
            // instance.ScheduleTask(0, 06, 30, Job);

            if (args.Length == 1)
            {
                argumentName = args[0];
            }
            else if (args.Length == 2)
            {
                argumentName = args[0];
                IntervalInSeconds = int.Parse(args[1]);

            }
            instance.ScheduleChecker(IntervalInSeconds, StartCheck);
            if (args.Length > 0)
            {
                _client.GetAsync($"api/update/{argumentName}").GetAwaiter().GetResult();
            }
            Console.Read();

        }
        private static void UpdateJurnal()
        {
            lock (lockObj)
            {
                Console.WriteLine("Locked");
                var read = System.IO.File.ReadAllText(PATH_VERSIONING);
                var serializer = new XmlSerializer(typeof(PluginVersions));
                using (var stream = new StringReader(read))
                using (var reader = XmlReader.Create(stream))
                {
                    pluginVersion = (PluginVersions)serializer.Deserialize(reader);
                }
                Console.WriteLine("UnLocked");
            }
        }
        private static VersioningType AddMissedVersion(string PluginName, int Version)
        {
            lock (lockObj)
            {
                if (Version == 0)
                {
                    Console.WriteLine("11112222333334444");
                    return VersioningType.None;
                }

                if (pluginVersion.Plugin.All(x => x.name.ToLower() != PluginName.ToLower()))
                {
                    pluginVersion.Plugin.Add(new PluginVersionsPlugin { name = PluginName, version = Version });
                    using (StreamWriter myWriter = new StreamWriter(PATH_VERSIONING, false))
                    {
                        XmlSerializer mySerializer = new XmlSerializer(typeof(PluginVersions));
                        mySerializer.Serialize(myWriter, pluginVersion);
                    }
                    Console.WriteLine($"Added new Row In Client Versioning for P: {PluginName} | V: {Version}");
                    return VersioningType.Added;
                }
                else
                {
                    var row = pluginVersion.Plugin.FirstOrDefault(x => x.name.ToLower() == PluginName.ToLower());
                    if (row.version >= Version)
                    {
                        Console.WriteLine($"Dismiss Client Version is Up to date | P: {PluginName} | V: {Version}");
                        return VersioningType.Dismiss;
                    }
                    else if (IsDataGetted && chunkedData[PluginName]?.Count == countDict[PluginName])
                    {
                        Console.WriteLine($"Need an Update, Client Versioning for P: {PluginName} | From: {row.version} To: {Version}");
                        return VersioningType.NeedUpdate;
                    }
                    else
                    {
                        return VersioningType.None;
                    }
                }
            }
        }

        public static void StartCheck()
        {
            lock (lockObj)
            {
                if (connection.State == HubConnectionState.Disconnected)
                {
                    connection.StartAsync().GetAwaiter().GetResult();
                }

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("StartCheck activated");
                Console.ResetColor();
                UpdateJurnal();

                if (countDict.Count != PluginsCount && configDict.Count != PluginsCount)
                    return;

                foreach (var pluginNAME in countDict.Keys)
                {
                    instance.StopTimers();

                    if (IsDataGetted && chunkedData[pluginNAME].Count == countDict[pluginNAME])
                    {
                        switch (AddMissedVersion(pluginNAME, PlugnameVersion[pluginNAME]))
                        {
                            case VersioningType.Added:
                                Console.WriteLine("VersioningType.Added");
                                NeedUpdatePlugin(pluginNAME);
                                break;
                            case VersioningType.Dismiss:
                                Console.WriteLine("VersioningType.Dismiss");
                                break;
                            case VersioningType.NeedUpdate:
                                Console.WriteLine("VersioningType.NeedUpdate");
                                NeedUpdatePlugin(pluginNAME);
                                break;
                            case VersioningType.None:
                                Console.WriteLine("None");
                                break;
                        }
                    }
                }
                chunkedData.Clear();
                PlugnameVersion.Clear();
                IsDataGetted = false;
                countDict.Clear();
                configDict.Clear();
                instance.ScheduleChecker(IntervalInSeconds, StartCheck);
            }

            #region old Code
            //var mess = superList.FirstOrDefault();
            //if (mess != null)
            //{
            //    if (mess.Version > 0)
            //    {
            //        var pluginFullName = mess.PluginName + "." + mess.Extension;
            //        var res = SaveBytesToFile(pluginFullName, mess.Files);
            //        if (res)
            //        {
            //            Console.ForegroundColor = ConsoleColor.Green;
            //            Console.WriteLine($"Plugin: {mess.PluginName} is Succeed! at {DateTime.Now}");
            //            UnZipPlugin(pluginFullName, mess.PluginName);
            //            MoveFilesTo(mess.PluginName);
            //        }
            //        else
            //        {
            //            Console.ForegroundColor = ConsoleColor.Red;
            //            Console.WriteLine($"Plugin: {mess.PluginName} is Failed! at {DateTime.Now}");
            //        }
            //    }
            //    else
            //    {
            //        Console.ForegroundColor = ConsoleColor.DarkMagenta;
            //        Console.WriteLine($"There is no New version for '{mess.PluginName}' plugin");
            //    }
            //    Console.ResetColor();
            //    //Console.WriteLine($"Message from server : {mess.Type} || {mess.ID}");
            //    superList.RemoveAt(0);
            //}
            //if (pluginsList.Count != 0)
            //{
            //    Console.ForegroundColor = ConsoleColor.Blue;
            //    Console.WriteLine("There are Updates for Plugins:");
            //    Console.ForegroundColor = ConsoleColor.DarkYellow;
            //    foreach (var t in pluginsList)
            //    {
            //        Console.WriteLine($"Plugin: '{t}'");
            //        _client.GetAsync($"api/values/{t}").GetAwaiter().GetResult();
            //    }

            //    _client.PostAsJsonAsync("api/values", pluginsList);
            //    pluginsList.Clear();

            //}
            #endregion

        }
        public static void NeedUpdatePlugin(string plName)
        {
            lock (lockObj)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;

                if (!Directory.Exists(PROJECT_ROOT + @"\Temp\" + plName))
                {
                    Console.WriteLine($"Directory : '{PROJECT_ROOT + @"\Temp\" + plName}' Not exists");
                    Directory.CreateDirectory(PROJECT_ROOT + @"\Temp\" + plName);
                }
                else
                {
                    Console.WriteLine($"Directory : '{PROJECT_ROOT + @"\Temp\" + plName}' exists,Delete with all files and Create");
                    Directory.Delete(PROJECT_ROOT + @"\Temp\" + plName, true);

                    Directory.CreateDirectory(PROJECT_ROOT + @"\Temp\" + plName);
                }

                foreach (var y in chunkedData[plName])
                {
                    Helper.SaveBytesToFile(y.PluginName, PROJECT_ROOT + @"\Temp\" + y.PluginName + @"\" + y.ChunkName,
                        y.Bytes);

                }


                Utils UT = new Utils();
                var isFileMergedSuccess = UT.MergeFile(PROJECT_ROOT + @"\Temp\" + plName + @"\" + PlugChunk[plName]).GetAwaiter().GetResult();  // async context


                if (isFileMergedSuccess)
                {

                    Console.WriteLine("Update Plugin Config file");
                    _client.PutAsJsonAsync("api/update", plName).GetAwaiter().GetResult();
                    var row = pluginVersion.Plugin.FirstOrDefault(x => x.name.ToLower() == plName.ToLower());
                    row.version = PlugnameVersion[plName];
                    using (StreamWriter myWriter = new StreamWriter(PATH_VERSIONING, false))
                    {
                        XmlSerializer mySerializer = new XmlSerializer(typeof(PluginVersions));
                        mySerializer.Serialize(myWriter, pluginVersion);
                    }
                    Console.WriteLine($"Moving Merged Data to: {PROJECT_ROOT + @"\" + plName + "." + FileExtension}");

                    Helper.MoveFilesTo(PROJECT_ROOT + @"\Temp\" + plName + @"\" + plName + "." + FileExtension,
                        PROJECT_ROOT + @"\" + plName + "." + FileExtension);

                    Console.WriteLine($"Deleting Chunks from: {PROJECT_ROOT + @"\Temp\" + plName}");

                    Directory.Delete(PROJECT_ROOT + @"\Temp\" + plName, true);


                    var isUnzipped = Helper.UnZipPlugin(PROJECT_ROOT + @"\" + plName + "." + FileExtension, plName);
                    if (isUnzipped)
                    {
                        Console.WriteLine("Plugin Unzipped Path ");
                        File.Delete(PROJECT_ROOT + @"\" + plName + "." + FileExtension);
                    }

                    var cModel = configDict[plName];
                    Helper.GetAndStoreConfigFiles(PROJECT_ROOT + @"\" + plName + ".json", cModel);
                }
                else
                {
                    Console.WriteLine("Not Send");
                }

            }

            Console.ResetColor();
        }


        private static void Job()
        {
            if (connection.State == HubConnectionState.Disconnected)
            {
                connection.StartAsync().GetAwaiter().GetResult();
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine($"Called check Update at {DateTime.Now} ");
            _client.GetAsync($"api/values/all").GetAwaiter().GetResult();
            Console.ResetColor();
        }

    }
    public class TaskScheduler
    {
        private static TaskScheduler _instance;
        private List<Timer> timers = new List<Timer>();
        public int _interval;
        private TaskScheduler() { }

        public static TaskScheduler Instance => _instance ?? (_instance = new TaskScheduler());

        public void StopTimers()
        {
            timers.ForEach(x => x.Dispose());
        }
        public void ScheduleTask(int hour, int min, double intervalInHour, Action task)
        {
            DateTime now = DateTime.Now;
            DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, 0);
            if (now > firstRun)
            {
                firstRun = firstRun.AddDays(1);
            }

            TimeSpan timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero;
            }

            var timer = new Timer(x =>
            {
                task.Invoke();
            }, null, timeToGo, TimeSpan.FromSeconds(intervalInHour));

            timers.Add(timer);
        }

        public void ScheduleChecker(int interval, Action task)
        {
            _interval = interval;
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(_interval);

            var timer = new System.Threading.Timer((e) =>
            {
                task.Invoke();
            }, null, startTimeSpan, periodTimeSpan);
            timers.Add(timer);
        }
    }
}