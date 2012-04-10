using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Web;

namespace Contracts.Common
{
    /*
      * MEF Configuration for ISA France
      * 
      * This document provides a quick overview how to configure a web service using MEF 
      * (Microsoft’s “Manager Extensibility Framework”). 
      * 
      * Introduction
      * 
      * In MEF, a CompositionContainer creates object instances by composing these out of 
      * a pool of known types. Usually, you add all types from a given assembly (or all 
      * assemblies in a given directory) to the CompositionContainer to make them all 
      * available without effort, all at once. In the ISA France development setting, we 
      * need flexibility to switch between different implementations (such as ‘the production 
      * implementation’ and ‘the mockup implementation’). When all implementations are 
      * available at the same time, MEF cannot determine which implementation to choose. 
      * Therefore, the CompositionContainer is configured through a dedicated configuration
      * section in the web.config file.
      * 
      * The <mefConfig> configuration section
      * 
      * The <mefConfig> configuration section can be processed using the MEFConfigurationElement
      * type. This type allows the developer to retrieve a fully configured CompositionContainer, 
      * using a static property. 
      * 
      * Which types to include
      * 
      * The basic idea is that all .NET types which should be available to the CompositionContainer 
      * need to be explicitly listed .  Instead of just listing all type names in one big list,
      * the <mefConfig> element contains <provider> elements. In the example below, you can see 
      * the <provider> elements with name attributes “BaseTypes” and “FrontEndServices”. The
      * value attribute determines the element names of child elements which contain types that 
      * should be included in the CompositionContainer. For example, the <provider name="BaseTypes"
      * value="baseType">  element ensures that all types listed in <baseType> elements are included 
      * in the CompositionContainer. 
      * 
      * <mefConfig>
      *   <provider name="BaseTypes" value="baseType">
      *     <baseType type="VWFSAG.Isa.Server.Common.Logging.Log, VWFSAG.Isa.Server.Common" />
      *     <baseType type="VWFSAG.Isa.Server.Common.Utils.MockupResponseAggregator, VWFSAG.Isa.Server.Common" />
      *   </provider>
      *   <provider name="FrontEndServices" value="frontendService">
      *     <frontendService type="VWFSAG.Isa.Server.Frontend.Services.HealthMonitorService, VWFSAG.Isa.Server.Frontend" />
      *     <frontendService type="VWFSAG.Isa.Server.Frontend.Services.InfrastructureService, VWFSAG.Isa.Server.Frontend" />
      *     <frontendService type="VWFSAG.Isa.Server.Frontend.Services.LanguageService, VWFSAG.Isa.Server.Frontend" />
      *     <frontendService type="VWFSAG.Isa.Server.Frontend.Services.LogOnService, VWFSAG.Isa.Server.Frontend" />
      *     ...
      *   </provider>
      *   <provider name="HealthMonitorService" value="production">
      *     <production type="VWFSAG.Isa.Server.Frontend.Provider.HealthMonitorServiceProvider, VWFSAG.Isa.Server.Frontend"/>
      *   </provider>
      * 
 
      * The element names of the actual types can be freely chosen. This enables us to easily switch 
      * between different implementations. In the example below, you see two types listed as LogOnProvider
      * implementations, a mock implementation and a production one. By tweaking the value attribute,
      * you can simply choose which implementation to use. 
      * 
      * <mefConfig>
      *   ...
      *   <provider name="LogOnProvider" value="mock">
      *     <mock type="Frontend.MockUp.Provider.LogOnServiceProvider" 
      *           assembly="..\Frontend.MockUp\bin\Debug\Frontend.MockUp.dll" />
      *     <production type="VWFSAG.Isa.Server.Frontend.Provider.LogOnServiceProvider, VWFSAG.Isa.Server.Frontend" />
      *   </provider>
      *   ...
      * </mefConfig>
      * 
      * How to find the type
      * 
      * When the element only contains a type attribute, the type is expected to be directly accessible 
      * via the usual assembly loading mechanism. When the element also contains an assembly attribute, 
      * this attribute points to the file system location of the type…
      * */
    public class MEFConfigurationElement : IConfigurationSectionHandler
    {
        internal static class Names
        {
            internal const string MefConfig = "mefConfig";
            internal const string Provider = "provider";
            internal const string Name = "name";
            internal const string Value = "value";
            internal const string Type = "type";
            internal const string Assembly = "assembly";
        }

        public static MEFConfigurationElement LoadMEFConfigurationElement()
        {
            var sect = ConfigurationManager.GetSection(Names.MefConfig) as MEFConfigurationElement;

            if (sect == null)
            {
                throw new CompositionException(string.Format("Cannot load MEF configuration section <{0}>", Names.MefConfig));
            }

            return sect;
        }

        private static CompositionContainer _CompositionContainer;
        private static readonly object _CompositionContainerLock = new object();
        public static CompositionContainer ConfiguredCompositionContainer
        {
            get
            {
                if (_CompositionContainer == null)
                {
                    lock (_CompositionContainerLock)
                    {
                        if (_CompositionContainer == null)
                        {
                            var cfg = LoadMEFConfigurationElement();
                            _CompositionContainer = cfg.GetCompositionContainer();
                        }
                    }
                }
                return _CompositionContainer;
            }
        }

        public CompositionContainer GetCompositionContainer()
        {
            return CompositionContainerFactory.Create(new TypeCatalog(Types));
        }

        public List<Type> Types { get; private set; }

        public MEFConfigurationElement()
        {
            Types = new List<Type>();
        }

        public object Create(object parent, object configContext, XmlNode section)
        {
            if (section == null) throw new ArgumentNullException("section");

            var outerXml = section.OuterXml;

            try
            {
                var providerElems = XElement.Parse(outerXml).Elements(Names.Provider);

                foreach (var providerElem in providerElems)
                {
                    var providerName = GetAttributeValue(providerElem, Names.Name, true, string.Format("<{0}>", Names.MefConfig));
                    var childElemName = GetAttributeValue(providerElem, Names.Value, true, providerName);
                    var values = GetTypesFromElement(providerElem, childElemName, providerName);

                    values.ToList().ForEach(type =>
                    {
                        if (!Types.Contains(type))
                        {
                            Types.Add(type);
                        }
                    });
                }

                return this;
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine("The MEF configuration contains errors...");
                for (Exception e = ex; e != null; e = e.InnerException)
                {
                    sb.AppendLine(string.Format("{0}: {1}", e.GetType().Name, e.Message));
                    sb.AppendLine("---------------------------");
                }
                sb.Append(outerXml);

                throw new ArgumentException(sb.ToString(), "section", ex);
            }
        }

        private static string GetAttributeValue(XElement e, XName key, bool isMandatory, string providerName)
        {
            var att = e.Attribute(key);
            if (att == null)
            {
                if (isMandatory)
                {
                    throw new ArgumentException(
                        string.Format(
                            "Element \"{0}\" does not contain mandatory \"{1}\" attribute (in the {2} provider config)",
                            e.ToString(), key.ToString(), providerName), "e");
                }
                else
                {
                    return string.Empty;
                }
            }

            return att.Value;
        }

        private static IEnumerable<Type> GetTypesFromElement(XElement parentElement, XName childElementName, string providerName)
        {
            if (parentElement == null) throw new ArgumentNullException("parentElement");
            if (childElementName == null) throw new ArgumentNullException("childElementName");

            var innerElems = parentElement.Elements(childElementName);
            var result = new List<Type>();

            foreach (var i in innerElems)
            {
                var typeName = GetAttributeValue(i, Names.Type, true, providerName);
                var assemblyFile = GetAttributeValue(i, Names.Assembly, false, providerName);
                if (string.IsNullOrEmpty(assemblyFile))
                {
                    #region Load from loaded assembly

                    var type = Type.GetType(typeName);
                    if (type == null)
                    {
                        throw new TypeLoadException(string.Format(
                            "Could not load type {0} from existing assembly file {1}",
                            typeName, assemblyFile));
                    }

                    result.Add(type);
                    continue;

                    #endregion
                }
                else
                {
                    #region Load assembly from disk

                    Func<string, string> abs = x => new FileInfo(x).FullName;

                    string physicalAssemblyFile = null;

                    if (HttpContext.Current != null)
                    {
                        var p = HttpContext.Current.Server.MapPath(".");

                        physicalAssemblyFile = abs(Path.Combine(abs(p), assemblyFile));
                    }
                    else
                    {
                        physicalAssemblyFile = abs(assemblyFile);
                    }

                    if (File.Exists(physicalAssemblyFile))
                    {
                        var assembly = Assembly.LoadFrom(physicalAssemblyFile);
                        var instance = assembly.CreateInstance(typeName);
                        if (instance == null)
                        {
                            throw new TypeLoadException(string.Format(
                                "Could not load type {0} from physical assembly file {1}",
                                typeName, physicalAssemblyFile));
                        }
                        result.Add(instance.GetType());
                    }
                    else
                    {
                        Trace.TraceWarning(string.Format("Cannot locate type {0}", typeName));
                        throw new TypeLoadException(string.Format(
                            "Could not load physical assembly file {0}", physicalAssemblyFile));
                    }

                    #endregion
                }
            }

            if (result.Count == 0)
            {
                throw new ArgumentException(string.Format(
                    "The MEF configuration for provider \"{0}\" does not have any provider elements named \"{1}\"",
                    providerName, childElementName.LocalName));
            }
            return result;
        }

        /*
* <configuration>
     <configSections>
       <section name="mefConfig"  type="VWFSAG.Isa.Server.Common.Composition.MEFConfigurationElement, VWFSAG.Isa.Server.Common" />
     </configSections>
      <mefConfig>
        <provider name="LogOnProvider" value="mock">
          <!-- must be two times .. because of the WCF/ in the path -->
          <mock type="Frontend.MockUp.Provider.LogOnServiceProvider" assembly="..\Frontend.MockUp\bin\Debug\Frontend.MockUp.dll" />
          <production type="VWFSAG.Isa.Server.Frontend.Provider.LogOnServiceProvider, VWFSAG.Isa.Server.Frontend" />
        </provider>
      </mefConfig>
         
         <mefConfig>
       <provider name="LoggingProvider" value="mock">
         <mock type="Logging Mock 1" />
         <mock type="Logging Mock 2" />
         <production type="Production Logger" />
       </provider>
       <provider name="LogOnBackendProvider" value="mock">
         <mock type="Mock Logon" />
         <production type="Production Login" />
       </provider>
       <provider name="CarConfigurator" value="production">
         <mock type="Mock CC" />
         <production type="production CC" />
       </provider>
       <prov name="Some stff" value="jkl">
         <jkl  />
       </prov>
     </mefConfig>

*/
    }
}
