﻿using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Arya.Framework.Settings
{

    // 
    // This source code was auto-generated by xsd, Version=2.0.50727.42.
    // 

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public partial class Field
    {

        private string valueField;

        /// <remarks/>
        [XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ProjectPreferences
    {
        private AssetUrlSection assetUrlSectionField;
        private SearchOptions searchOptionsField;

        /// <remarks/>
        [XmlElementAttribute("SearchOptions", typeof(SearchOptions), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SearchOptions SearchOptions
        {
            get
            {
                return this.searchOptionsField;
            }
            set
            {
                this.searchOptionsField = value;
            }
        }

        [XmlElementAttribute("AssetUrlSection", typeof(AssetUrlSection), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AssetUrlSection AssetUrlSection
        {
            get
            {
                return this.assetUrlSectionField;
            }
            set
            {
                this.assetUrlSectionField = value;
            }
        }

        [XmlElementAttribute("ListSeparator", typeof(string), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ListSeparator { get; set; }

        [XmlElementAttribute("ReturnSeparator", typeof(string), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ReturnSeparator { get; set; }

    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class AssetUrlSection
    {

        private Url[] urlField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Url", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Url[] Urls
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class Url
    {
        private static readonly Regex RxImageUrl = new Regex("\\[(.+)\\]", RegexOptions.Compiled);

        private string valueField;

        private string targetField;

        private string typeField;

        private string aryaAssetAttributeNameField;

        private string orderField;

        [XmlIgnoreAttribute]
        public string AssetAttributeName
        {
            get
            {
                if (aryaAssetAttributeNameField == null)
                {
                    try
                    {
                        aryaAssetAttributeNameField =
                            RxImageUrl.Matches(valueField)[0].Groups[1].Value;
                    }
                    catch (Exception)
                    {
                        aryaAssetAttributeNameField = string.Empty;
                    }
                }

                return aryaAssetAttributeNameField;
            }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string Target
        {
            get
            {
                return this.targetField;
            }
            set
            {
                this.targetField = value;
            }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [XmlAttributeAttribute()]
        public string Order
        {
            get
            {
                return this.orderField;
            }
            set
            {
                this.orderField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class SearchOptions
    {

        private Field[] standardField;

        private Field[] customField;

        /// <remarks/>
        [XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItemAttribute("Field", typeof(Field))]
        public Field[] Standard
        {
            get
            {
                if (standardField == null || standardField.Length == 0)
                {
                    var itemIDField = new Field() { Value = "ItemID" };
                    var taxonomyField = new Field { Value = "Taxonomy" };
                    standardField = new[] { itemIDField, taxonomyField };
                }
                return this.standardField;
            }
            set
            {
                this.standardField = value;
            }
        }

        /// <remarks/>
        [XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItemAttribute("Field", typeof(Field))]
        public Field[] Custom
        {
            get
            {
                return this.customField;
            }
            set
            {
                this.customField = value;
            }
        }
    }

}