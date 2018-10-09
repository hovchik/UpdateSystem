
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.

using System.Collections.Generic;

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class PluginVersions
{

    private List<PluginVersionsPlugin> pluginField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Plugin")]
    public List<PluginVersionsPlugin> Plugin
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
public partial class PluginVersionsPlugin
{

    private string nameField;

    private int versionField;

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

