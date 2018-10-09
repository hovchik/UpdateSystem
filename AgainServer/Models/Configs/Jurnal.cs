// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class Plugins
{
    private PluginsPlugin[] pluginField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Plugin")]
    public PluginsPlugin[] Plugin
    {
        get
        {
            return this.pluginField;
        }
        set
        {
            this.pluginField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class PluginsPlugin
{
    private bool isSyncField;

    private string pathField;

    private string configPathField;

    private string extensionField;

    private string nameField;

    private int versionField;

    /// <remarks/>
    public bool IsSync
    {
        get
        {
            return this.isSyncField;
        }
        set
        {
            this.isSyncField = value;
        }
    }

    /// <remarks/>
    public string Path
    {
        get
        {
            return this.pathField;
        }
        set
        {
            this.pathField = value;
        }
    }

    /// <remarks/>
    public string ConfigPath
    {
        get
        {
            return this.configPathField;
        }
        set
        {
            this.configPathField = value;
        }
    }

    /// <remarks/>
    public string Extension
    {
        get
        {
            return this.extensionField;
        }
        set
        {
            this.extensionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public int version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }
}