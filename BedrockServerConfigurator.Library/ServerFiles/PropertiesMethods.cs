﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BedrockServerConfigurator.Library.ServerFiles
{
    public partial class Properties
    {
        private readonly string propertiesFilePath;

        /// <summary>
        /// Pass in the content of server.properties
        /// </summary>
        /// <param name="propertiesFile"></param>
        internal Properties(string propertiesFilePath, bool autoSetProperties = true)
        {
            this.propertiesFilePath = propertiesFilePath;

            if (autoSetProperties) SetMinecraftProperties();
        }

        /// <summary>
        /// Overwrites server.properties with current version of ServerProperties.
        /// If server is running it's recommended to call RestartServer.
        /// Call this everytime ServerProperties are updated so they will be saved.
        /// </summary>
        public void SavePropertiesToFile()
        {
            File.WriteAllText(propertiesFilePath, ClassPropertiesToFileProperties());
        }

        /// <summary>
        /// Gets all properties and their values from server.properties file
        /// </summary>
        /// <returns></returns>
        public List<Property> LoadPropertiesFromFile()
        {
            var lines = File.ReadAllLines(propertiesFilePath);

            var propertiesList = new List<Property>();
            const string summaryLine = "# ";

            var name = "";
            var value = "";
            var summaries = new List<string>();

            bool fileHasSummaries = lines.Any(x => x.StartsWith(summaryLine));

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith(summaryLine))
                {
                    var summary = trimmedLine.Replace(summaryLine, "");

                    summaries.Add(summary);
                }

                if (line.Contains("=") && trimmedLine.StartsWith(summaryLine) == false)
                {
                    var split = trimmedLine.Split('=');

                    name = split[0];
                    value = split[1];
                }
                
                if (line.Length < 3 || (line.Contains("=") && fileHasSummaries == false))
                {
                    var newProperty = new Property(name, value, summaries);
                    propertiesList.Add(newProperty);

                    name = "";
                    value = "";
                    summaries = new List<string>();
                }
            }

            return propertiesList;
        }

        /// <summary>
        /// Sets properties of this instance
        /// </summary>
        /// <returns></returns>
        public void SetMinecraftProperties()
        {
            var propsVals = LoadPropertiesFromFile();
            var type = GetType();

            foreach (var (name, value, _) in propsVals)
            {
                var prop = type.GetProperty(FormatFilePropertyToClassProperty(name));

                if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(this, bool.Parse(value));
                }
                else if (prop.PropertyType == typeof(double))
                {
                    prop.SetValue(this, double.Parse(Utilities.DecimalStringToCurrentCulture(value)));
                }
                else if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(this, value);
                }
                else if (prop.PropertyType == typeof(MinecraftGamemode))
                {
                    prop.SetValue(this, Enum.Parse<MinecraftGamemode>(value, true));
                }
                else if (prop.PropertyType == typeof(MinecraftDifficulty))
                {
                    prop.SetValue(this, Enum.Parse<MinecraftDifficulty>(value, true));
                }
                else if (prop.PropertyType == typeof(MinecraftPermission))
                {
                    prop.SetValue(this, Enum.Parse<MinecraftPermission>(value, true));
                }
            }
        }

        /// <summary>
        /// Converts class property name to format of file property -
        /// ServerName -> server-name
        /// </summary>
        /// <param name="classProperty"></param>
        /// <returns></returns>
        public static string FormatClassPropertyToFileProperty(string classProperty)
        {
            string result = "";

            for (int i = 0; i < classProperty.Length; i++)
            {
                if (i == 0)
                {
                    result += char.ToLower(classProperty[i]);
                }
                else if (char.IsUpper(classProperty[i]))
                {
                    result += $"-{char.ToLower(classProperty[i])}";
                }
                else
                {
                    result += classProperty[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Converts name of a property in a server.properties file to a format of class property -
        /// server-name -> ServerName
        /// </summary>
        /// <param name="fileProperty"></param>
        /// <returns></returns>
        public static string FormatFilePropertyToClassProperty(string fileProperty)
        {
            string result = "";

            for (int i = 0; i < fileProperty.Length; i++)
            {
                if (fileProperty[i] == '-') continue;

                if (i == 0 || fileProperty[i - 1] == '-')
                {
                    result += char.ToUpper(fileProperty[i]);
                }
                else
                {
                    result += fileProperty[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Creates properties for a class created from server.properties file
        /// </summary>
        /// <returns></returns>
        public string GeneratePropertiesClass()
        {
            return string.Join("\n", LoadPropertiesFromFile()
                         .Select(createClassProperty));

            static string createClassProperty(Property property)
            {
                var builder = new StringBuilder();

                var propertySummary = getSummary(property);
                var propertyType = getType(property);
                var propertyName = FormatFilePropertyToClassProperty(property.Name);

                var classProperty = $"public {propertyType} {propertyName} {{ get; set; }}";

                var fullClassProperty = $"{propertySummary}\n{classProperty}\n";

                return fullClassProperty;
            }
            
            static string getSummary(Property prop)
            {
                var summary =
@$"/// <summary>
/// {string.Join("\n/// ", prop.SummaryLines)}
/// </summary>";
                
                return summary;
            }

            static string getType(Property prop)
            {
                var value = prop.Value;

                if (double.TryParse(Utilities.DecimalStringToCurrentCulture(value), out _))
                {
                    return "double";
                }
                else if (bool.TryParse(value, out _))
                {
                    return "bool";
                }
                else // ENUMS
                {
                    var allowedValuesString = "Allowed values: ";
                    var lastLine = prop.SummaryLines.Last();

                    if (lastLine.StartsWith(allowedValuesString))
                    {
                        var valuesString = lastLine.Replace(allowedValuesString, "");

                        if (valuesString.Contains("\""))
                        {
                            valuesString = valuesString.Replace(" or ", " ");
                            valuesString = valuesString.Replace(", ", " ");
                            valuesString = valuesString.Replace("\"", "");
                            valuesString = valuesString.Trim();

                            var values = valuesString.Split(" ");

                            var gamemodes = Enum.GetNames<MinecraftGamemode>().Select(x => x.ToLower());
                            var difficulties = Enum.GetNames<MinecraftDifficulty>().Select(x => x.ToLower());
                            var permissions = Enum.GetNames<MinecraftPermission>().Select(x => x.ToLower());

                            if (gamemodes.Contains(values[0]))
                            {
                                return nameof(MinecraftGamemode);
                            }
                            else if (difficulties.Contains(values[0]))
                            {
                                return nameof(MinecraftDifficulty);
                            }
                            else if (permissions.Contains(values[0]))
                            {
                                return nameof(MinecraftPermission);
                            }
                        }
                    }
                }

                return "string";
            }
        }

        /// <summary>
        /// Gets value from file property with class property name
        /// </summary>
        /// <param name="classProperty"></param>
        /// <returns></returns>
        public string FilePropertyValue(string classProperty)
        {
            var fileProperty = FormatClassPropertyToFileProperty(classProperty);

            return LoadPropertiesFromFile().FirstOrDefault(x => x.Name == fileProperty).Value;
        }

        /// <summary>
        /// Converts all properties into a format which server.properties file uses
        /// </summary>
        /// <returns></returns>
        public string ClassPropertiesToFileProperties()
        {
            var properties = GetType().GetProperties();

            var strBuilder = new StringBuilder();            

            foreach (var prop in properties)
            {
                var name = FormatClassPropertyToFileProperty(prop.Name);
                var value = prop.GetValue(this);

                if (prop.PropertyType == typeof(double))
                {
                    value = Utilities.DecimalStringToDot(value.ToString());
                }
                else if (prop.PropertyType != typeof(string))
                {
                    value = value.ToString().ToLower();
                }

                strBuilder.AppendLine($"{name}={value}");
            }

            return strBuilder.ToString();
        }

        /// <summary>
        /// Returns all properties in a server.properties format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ClassPropertiesToFileProperties();
        }
    }
}
