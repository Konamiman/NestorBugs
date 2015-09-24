using System;
using System.Linq;
using System.Xml.Linq;
using System.ComponentModel;
using Microsoft.Data.Entity.Design.Extensibility;

namespace Konamiman.VsExtensions.EntityModelExtraAttributes
{
    class AutoGenerateValidationAttributesProperty
    {
        internal static XName elementName = XName.Get("AutoGenerateValidationAttributes", GlobalData.XmlNamespace);

        private XElement parent;
        private PropertyExtensionContext context;

        const bool defaultValue = true;

        public AutoGenerateValidationAttributesProperty(XElement parent, PropertyExtensionContext context)
        {
            this.context = context;
            this.parent = parent;
        }

        [DisplayName("Autogenerate validation attributes")]
        [Description("Indicates whether validation properties should be automatically generated from the other entity attributes.")]
        [Category("Extra attributes")]
        [DefaultValue(defaultValue)]
        public bool PropertyValue
        {
            get
            {
                XElement child = parent.Element(elementName);
                if(child == null) {
                    return defaultValue;
                }

                bool value = defaultValue;
                bool.TryParse(child.Value, out value);
                return value;
            }

            set
            {
                using(EntityDesignerChangeScope scope = context.CreateChangeScope("Konamiman.VsExtensions.EntityModelExtraAttributes.AutoGenerateValidationAttributesProperty_set")) {
                    var element = parent.Element(elementName);
                    if(element == null) {
                        if(value != defaultValue) {
                            parent.Add(new XElement(elementName, value.ToString()));
                        }
                    }
                    else {
                        if(value == defaultValue) {
                            element.Remove();
                        }
                        else {
                            element.SetValue(value.ToString());
                        }
                    }
                    scope.Complete();
                }
            }
        }
    }
}