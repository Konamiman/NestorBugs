using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Data.Entity.Design.Extensibility;

namespace Konamiman.VsExtensions.EntityModelExtraAttributes
{
    abstract class EdmStringPropertyBase
    {
        private readonly XName elementXName = XName.Get("ExtraAttributes", GlobalData.XmlNamespace);
        private readonly string elementName;

        private XElement parent;
        private PropertyExtensionContext context;

        protected EdmStringPropertyBase(XElement parent, PropertyExtensionContext context)
            :this(null, parent, context)
        {

        }

        protected EdmStringPropertyBase(string elementName, XElement parent, PropertyExtensionContext context)
        {
            this.context = context;
            this.parent = parent;

            if(elementName == null) {
                elementName = this.GetType().Name;
                if(elementName.EndsWith("Property")) {
                    elementName = elementName.Substring(0, elementName.Length - 8);
                }
            }

            this.elementName = elementName;
            this.elementXName = XName.Get(elementName, GlobalData.XmlNamespace);
        }

        public virtual string PropertyValue
        {
            get
            {
                XElement child = parent.Element(elementXName);
                return (child == null ? String.Empty : child.Value);
            }

            set
            {
                using(EntityDesignerChangeScope scope = context.CreateChangeScope("Konamiman.VsExtensions.EntityModelExtraAttributes." + elementName + "_set")) {
                    var element = parent.Element(elementXName);
                    if(element == null) {
                        if(value != string.Empty) {
                            parent.Add(new XElement(elementXName, value));
                        }
                    }
                    else {
                        if(value == string.Empty) {
                            element.Remove();
                        }
                        else {
                            element.SetValue(value);
                        }
                    }
                    scope.Complete();
                }
            }
        }
    }
}
