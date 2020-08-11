using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BedrockServerConfigurator.Library.ServerFiles
{
    public class Properties
    {
        public string ServerName { get; set; }
        public MinecraftGamemode Gamemode { get; set; }
        public MinecraftDifficulty Difficulty { get; set; }
        public bool AllowCheats { get; set; }
        public double MaxPlayers { get; set; }
        public bool OnlineMode { get; set; }
        public bool WhiteList { get; set; }
        public double ServerPort { get; internal set; }
        public double ServerPortv6 { get; internal set; }
        public double ViewDistance { get; set; }
        public double TickDistance { get; set; }
        public double PlayerIdleTimeout { get; set; }
        public double MaxThreads { get; set; }
        public string LevelName { get; set; }
        public string LevelSeed { get; set; }
        public MinecraftPermission DefaultPlayerPermissionLevel { get; set; }
        public bool TexturepackRequired { get; set; }
        public bool ContentLogFileEnabled { get; set; }
        public double CompressionThreshold { get; set; }
        public bool ServerAuthoritativeMovement { get; set; }
        public double PlayerMovementScoreThreshold { get; set; }
        public double PlayerMovementDistanceThreshold { get; set; }
        public double PlayerMovementDurationThresholdInMs { get; set; }
        public bool CorrectPlayerMovement { get; set; }

        private readonly string propertiesFilePath;

        /// <summary>
        /// Pass in the content of server.properties
        /// </summary>
        /// <param name="propertiesFile"></param>
        internal Properties(string propertiesFilePath, bool autoSetProperties = true)
        {
            this.propertiesFilePath = propertiesFilePath;

            if (autoSetProperties) SetProperties();
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
        public List<(string propertyName, string propertyValue)> PropertyAndValueFromFile()
        {
            return File.ReadAllText(propertiesFilePath)
                       .Split("\n")                                     // split to every line
                       .Select(a => a.Trim())                           // trim whitespace from every line
                       .Where(b => !b.StartsWith("#") && b.Length > 0)  // every line that isn't a comment like or isn't empty
                       .Select(c => c.Split("="))                       // split them with =
                       .Select(d => Tuple.Create(d[0], d[1])            // on left side is property and on right is its value
                                         .ToValueTuple())               // let's turn them into a tuple
                       .ToList();                                       // and finally into a list
        }

        /// <summary>
        /// Sets properties of this instance
        /// </summary>
        /// <returns></returns>
        public void SetProperties()
        {
            var propsVals = PropertyAndValueFromFile();
            var type = GetType();

            foreach (var (name, value) in propsVals)
            {
                var prop = type.GetProperty(FilePropertyToProperty(name));

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
        public static string PropertyToFileProperty(string classProperty)
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
        public static string FilePropertyToProperty(string fileProperty)
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
        public string GenerateProperties()
        {
            return string.Join("\n", PropertyAndValueFromFile()
                         .Select(x => $"public {getType(x.propertyValue)} {FilePropertyToProperty(x.propertyName)} {{ get; set; }}"));

            static string getType(string value)
            {
                string result = "";

                if (double.TryParse(Utilities.DecimalStringToCurrentCulture(value), out _))
                {
                    result += "double";
                }
                else if (bool.TryParse(value, out _))
                {
                    result += "bool";
                }
                else
                {
                    result += "string";
                }
                // not sure how I would generate for enums yet

                return result;
            }
        }

        /// <summary>
        /// Gets value from file property with class property name
        /// </summary>
        /// <param name="classProperty"></param>
        /// <returns></returns>
        public string FilePropertyValue(string classProperty)
        {
            return PropertyAndValueFromFile().First(x => x.propertyName == PropertyToFileProperty(classProperty)).propertyValue;
        }

        /// <summary>
        /// Converts all properties into a format which server.properties file uses
        /// </summary>
        /// <returns></returns>
        public string ClassPropertiesToFileProperties()
        {
            var properties = GetType().GetProperties();

            var result = new List<string>(properties.Length);

            foreach (var prop in properties)
            {
                var name = PropertyToFileProperty(prop.Name);
                var value = prop.GetValue(this);

                if (prop.PropertyType == typeof(double))
                {
                    value = Utilities.DecimalStringToDot(value.ToString());
                }
                else if (prop.PropertyType != typeof(string))
                {
                    value = value.ToString().ToLower();
                }

                result.Add($"{name}={value}");
            }

            return string.Join('\n', result);
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
