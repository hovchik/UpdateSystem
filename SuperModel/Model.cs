using System;

namespace SuperModel
{
	[Serializable]
	public class Model
	{
		public int Version { get; set; }
		public string Extension { get; set; }

		public byte[] Files { get; set; }
		public string PluginName { get; set; }
	}

    [Serializable]
    public class ChunkedData
    {
        public int Version { get; set; }
        public string PluginName { get; set; }
        public string Extension { get; set; }
        public string ChunkName { get; set; }
        public string FilePathName { get; set; }
        public byte[] Bytes { get; set; }
    }
    [Serializable]
    public class ConfigModel
    {
        public string PluginName { get; set; }
        public byte[] JsonConfigFile { get; set; }
    }

    [Flags]
    public enum VersioningType
    {
        None,
        Added,
        NeedUpdate,
        Dismiss
    }
}