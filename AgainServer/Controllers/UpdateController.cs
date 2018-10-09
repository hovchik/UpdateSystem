using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using SuperModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AgainServer.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly string PATH;
        private readonly string PluginPathInServer;
        private readonly int MaxFileSizeKB;
        private readonly HubConnection _connection;
        private Plugins _plugins;
        IConfiguration _config;


        public UpdateController(HubConnection connection, IConfiguration config)
        {
            _config = config;
            _connection = connection;
            if (_connection.State == HubConnectionState.Disconnected)
            {
                _connection.StartAsync().GetAwaiter().GetResult();
            }
            PATH = config["XmlPath"];
            PluginPathInServer = config["PluginTempPath"];
            MaxFileSizeKB = int.Parse(config["MaxFileSizeKB"]);
            UpdateJurnal();
        }

        private void UpdateJurnal()
        {
            var read = System.IO.File.ReadAllText(PATH);
            var serializer = new XmlSerializer(typeof(Plugins));
            using (var stream = new StringReader(read))
            using (var reader = XmlReader.Create(stream))
            {
                _plugins = (Plugins)serializer.Deserialize(reader);
            }
        }


        [HttpGet]
        public string Get()
        {
            return "Starting Work";
        }

        [HttpGet("{id}")]
        public async Task<string> Get(string id)
        {
            var tempFolderPath = Path.Combine(PluginPathInServer, "Temp");
            if (id.ToLower() == "all")
            {
                var plugs = _plugins.Plugin.Where(pl => pl.IsSync).ToList();

                if (plugs.Count != 0)
                {
                    foreach (var plug in plugs)
                    {
                        await SendToClient(tempFolderPath, plug, plugs.Count);
                    }
                }
                else
                {
                    await _connection.InvokeAsync<string>("SendMessage", $"All Plugins are up to date {DateTime.Now}");
                }
            }
            else
            {
                var plug = _plugins.Plugin.FirstOrDefault(x => x.name == id);
                if (plug == null)
                {
                    await _connection.InvokeAsync<string>("SendMessage", $"'{id}' Plugin is Not exist");
                }
                else if (plug.IsSync == false)
                {
                    await _connection.InvokeAsync<string>("SendMessage", $"'{id}' Plugin is up to date {DateTime.Now}");
                }
                else
                {
                    await SendToClient(tempFolderPath, plug, 1);

                }
            }
            return id;
        }

        private async Task SendToClient(string tempFolderPath, PluginsPlugin plug, int itemCount)
        {
            try
            {
                if (plug != null)
                {
                    if (plug.IsSync)
                    {
                        Utils ut = new Utils();
                        ut.FileName = plug.Path;
                        ut.TempFolder = tempFolderPath;
                        if (!Directory.Exists(ut.TempFolder))
                            Directory.CreateDirectory(ut.TempFolder);
                        ut.MaxFileSizeKB = MaxFileSizeKB;
                        await ut.SplitFile();
                        ConfigModel sendModel = new ConfigModel
                        {
                            PluginName = plug.name,
                            JsonConfigFile = await Helper.GetBytesFromChunkedFileAsync(plug.ConfigPath)
                        };
                        foreach (string file in ut.FileParts) // improvement - this is sequential, make threaded
                        {
                            var chN = file.Split("\\");
                            if (_connection.State == HubConnectionState.Disconnected)
                            {
                                _connection.StartAsync().GetAwaiter().GetResult();
                            }
                            ChunkedData sendData = new ChunkedData
                            {
                                Version = plug.version,
                                PluginName = plug.name,
                                Extension = plug.Extension,
                                FilePathName = file,
                                ChunkName = chN[chN.Length - 1],
                                Bytes = await Helper.GetBytesFromChunkedFileAsync(file)
                            };

                            await _connection.InvokeAsync<ChunkedData>("SendChunkedFile", sendData, plug.name, ut.FileParts.Count, itemCount, sendModel);
                        }
                        Directory.Delete(tempFolderPath, true);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task Post([FromBody] List<string> value)
        {
            await Task.Run(() =>
            {
                foreach (var plugin in _plugins.Plugin)
                {
                    foreach (var name in value)
                    {
                        if (plugin.name.ToLower() == name.ToLower())
                        {
                            plugin.IsSync = false;
                        }
                    }
                }

                using (StreamWriter myWriter = new StreamWriter(PATH, false))
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(Plugins));
                    mySerializer.Serialize(myWriter, _plugins);
                }
            });
        }

        [HttpPut]
        public async Task Put([FromBody] string value)
        {
            await Task.Run(() =>
            {
                foreach (var plugin in _plugins.Plugin)
                {
                    if (plugin.name.ToLower() == value.ToLower())
                    {
                        plugin.IsSync = false;
                    }
                }

                using (StreamWriter myWriter = new StreamWriter(PATH, false))
                {
                    XmlSerializer mySerializer = new XmlSerializer(typeof(Plugins));
                    mySerializer.Serialize(myWriter, _plugins);
                }
            });
        }
    }
}