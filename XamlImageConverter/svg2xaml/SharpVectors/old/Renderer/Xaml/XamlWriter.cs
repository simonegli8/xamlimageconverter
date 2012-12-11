using System;
using System.IO;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Markup.Primitives;
using System.Text;
using System.Windows.Markup;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Media;

namespace SharpVectorXamlRenderingEngine
{


    // Summary:
    // Provides a single static Overload:SXamlWriter.Save method
    // that can be used for limited Extensible Application
    // Markup Language (XAML) serialization of provided runtime objects. This class
    // cannot be inherited, and only has static methods.
    public static class XamlWriter
    {
        static XamlWriter()
        {
            RootNamespace = SlRootNamespace;
        }

        public const string WPFRootNamespace = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"";
        public const string SlRootNamespace = "xmlns=\"http://schemas.microsoft.com/client/2007\"";

        // Summary:
        // Returns a Extensible Application Markup Language (XAML) string that serializes
        // the provided object.
        //
        // Parameters:
        // obj:
        // The element to be serialized. Typically, this is the root element of a page
        // or application.
        //
        // Returns:
        // Extensible Application Markup Language (XAML) string that can be written
        // to a stream or file. The logical tree of all elements that fall under the
        // provided obj element will be serialized.
        //
        // Exceptions:
        // System.Security.SecurityException:
        // the application is not running in full trust.
        //
        // System.ArgumentNullException:
        // obj is null.
        public static string Save(object obj, Dictionary<object, string> resources)
        {
            lastNames = new List<string>();
            StringBuilder sb = new StringBuilder();
            //System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-us")
            StringWriter sw = new StringWriter(sb, CultureInfo.GetCultureInfo("en-us"));

            WriteObject(obj, sw, true, resources);
            return sb.ToString();
        }

        private static List<string> lastNames;

        //WriteObject - 3 params (used primarily when isRoot is true or by the 2 param version)
        private static void WriteObject(object obj, StringWriter sw, bool isRoot, Dictionary<object, string> resources)
        {
            WriteObjectWithKey(null, obj, sw, isRoot, resources);
        }

        //WriteObject - 2 param version
        private static void WriteObject(object obj, StringWriter sw, Dictionary<object, string> resources)
        {
            WriteObjectWithKey(null, obj, sw, false, resources);
        }

        //WriteObject - 3 param version
        private static void WriteObjectWithKey(object key, object obj, StringWriter sw, Dictionary<object, string> resources)
        {
            WriteObjectWithKey(key, obj, sw, false, resources);
        }

        private static Dictionary<Type, string> contentProperties = new Dictionary<Type, string>();
        private const int space = 4;
        private static int indent = 0;


        //WriteObject - 4 params (used primarily when isRoot is true or by the 3 param version)
        private static void WriteObjectWithKey(object key, object obj, StringWriter sw, bool isRoot, Dictionary<object, string> resources)
        {            
            List<MarkupProperty> propertyElements = new List<MarkupProperty>();
            //If the value is a string
            string s = obj as string;
            if (s != null)
            {
                //TODO: in a dictionary, this should be serialized as a <s:String />
                sw.Write(s);
                return;
            }
            MarkupProperty contentProperty = null;
            string contentPropertyName = null;
            MarkupObject markupObj = MarkupWriter.GetMarkupObjectFor(obj);
            Type objectType = obj.GetType();
            sw.Write(Environment.NewLine);
            sw.Write(new string(' ', indent * space));
            sw.Write("<" + markupObj.ObjectType.Name);
            if (isRoot)
            {
                sw.Write(" ");
                sw.Write(RootNamespace);
                sw.Write(" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
            }

            if (key != null)
            {
                string keyString = key.ToString();
                if (keyString.Length > 0)
                    sw.Write(" x:Key=\"" + keyString + "\"");
                else
                    //TODO: key may not be a string, what about x:Type...
                    throw new NotImplementedException("Sample XamlWriter cannot yet handle keys that aren't strings");
            }

            //Look for CPA info in our cache that keeps contentProperty names per Type
            //If it doesn't have an entry, go get the info and store it.
            if (!contentProperties.ContainsKey(objectType))
            {
                string lookedUpContentProperty = string.Empty;
                foreach (Attribute attr in markupObj.Attributes)
                {
                    ContentPropertyAttribute cpa = attr as ContentPropertyAttribute;
                    if (cpa != null)
                        lookedUpContentProperty = cpa.Name;
                }
                contentProperties.Add(objectType, lookedUpContentProperty);
            }
            contentPropertyName = contentProperties[objectType];


            string contentString = string.Empty;
            foreach (MarkupProperty markupProperty in markupObj.Properties)
            {
                if (markupProperty.Name != contentPropertyName
                    || (!markupProperty.IsComposite))
                {
                    if (markupProperty.IsValueAsString)
                        contentString = markupProperty.Value as string;
                    else if (!markupProperty.IsComposite)
                    {
                        if (markupProperty.Name == "Name")
                        {
                            string name = (string)markupProperty.Value;
                            if (!string.IsNullOrEmpty(name))
                            {
                                sw.Write(" x:");
                                sw.Write(markupProperty.Name);
                                sw.Write("=\"");
                                int cnt = 1;
                                while (lastNames.Contains(name))
                                {
                                    name = string.Concat(markupProperty.Value, "_", cnt++);
                                }
                                lastNames.Add(name);
                                sw.Write(name);
                                sw.Write("\"");
                            }
                        }
                        else
                        {
                            sw.Write(" ");
                            sw.Write(markupProperty.Name);
                            sw.Write("=\"");
                            sw.Write(markupProperty.Value);
                            sw.Write("\"");
                        }
                    }
                    else if (markupProperty.Value.GetType() == typeof(NullExtension))
                    {
                        sw.Write(" ");
                        sw.Write(markupProperty.Name);
                        sw.Write("=\"{x:Null}\"");
                    }
                    else
                    {
                        string resourceKey;
                        if (resources.TryGetValue(markupProperty.Value, out resourceKey))
                        {
                            sw.Write(" ");
                            sw.Write(markupProperty.Name);
                            sw.Write("=\"{StaticResource " + resourceKey + "}\"");
                        }
                        else
                            propertyElements.Add(markupProperty);
                    }
                }
                else
                    contentProperty = markupProperty;
            }

            if (contentProperty != null || propertyElements.Count > 0 || contentString != string.Empty)
            {
                sw.Write(">");
                indent++;
                try
                {
                    foreach (MarkupProperty markupProp in propertyElements)
                    {
                        string propElementName = markupObj.ObjectType.Name + "." + markupProp.Name;
                        sw.Write(Environment.NewLine);
                        sw.Write(new string(' ', indent * space));
                        sw.Write("<" + propElementName + ">");
                        indent++;
                        try
                        {
                            WriteChildren(sw, markupProp, resources);                            
                        }
                        finally
                        {
                            indent--;
                        }
                        sw.Write(Environment.NewLine);
                        sw.Write(new string(' ', indent * space));
                        sw.Write("</" + propElementName + ">");
                    }
                    if (contentString != string.Empty)
                        sw.Write(contentString);
                    else if (contentProperty != null)
                        WriteChildren(sw, contentProperty, resources);                    
                }
                finally
                {
                    indent--;
                }
                sw.Write(Environment.NewLine);
                sw.Write(new string(' ', indent * space));
                sw.Write("</" + markupObj.ObjectType.Name + ">");
            }

            else
            {
                sw.Write("/>");
            }
        }

        private static void WriteChildren(StringWriter sw, MarkupProperty markupProp, Dictionary<object, string> resources)
        {
            if (!markupProp.IsComposite)
            {
                XamlWriter.WriteObject(markupProp.Value, sw, resources);
            }
            else
            {
                IList collection = markupProp.Value as IList;
                IDictionary dictionary = markupProp.Value as IDictionary;
                if (collection != null)
                {
                    foreach (object o in collection)
                        XamlWriter.WriteObject(o, sw, resources);
                }
                else if (dictionary != null)
                {
                    foreach (object key in dictionary.Keys)
                    {
                        XamlWriter.WriteObjectWithKey(key, dictionary[key], sw, resources);
                    }
                }
                else
                    XamlWriter.WriteObject(markupProp.Value, sw, resources);
            }
        }


        //
        // Summary:
        // Saves Extensible Application Markup Language (XAML) information into a provided
        // stream to serialize the provided object.
        //
        // Parameters:
        // obj:
        // The element to be serialized. Typically, this is the root element of a page
        // or application.
        //
        // stream:
        // Destination stream for the serialized XAML information.
        //
        // Exceptions:
        // System.Security.SecurityException:
        // the application is not running in full trust.
        //
        // System.ArgumentNullException:
        // obj is null -or- stream is null.
        public static void Save(object obj, Stream stream, Dictionary<object, string> resources)
        {
            StreamWriter writer = new StreamWriter(stream);
            stream.Seek(0, SeekOrigin.Begin); //this line may not be needed.
            writer.Write(Save(obj, resources));
            writer.Flush();
        }

        public static string RootNamespace { get; set; }
    }

}


