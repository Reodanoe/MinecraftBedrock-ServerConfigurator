using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BedrockServerConfigurator
{
    public class Properties
    {
        public string ServerName { get; set; }
        public string Gamemode { get; set; }
        public string Difficulty { get; set; }
        public bool AllowCheats { get; set; }
        public double MaxPlayers { get; set; }
        public bool OnlineMode { get; set; }
        public bool WhiteList { get; set; }
        public double ServerPort { get; set; }
        public double ServerPortv6 { get; set; }
        public double ViewDistance { get; set; }
        public double TickDistance { get; set; }
        public double PlayerIdleTimeout { get; set; }
        public double MaxThreads { get; set; }
        public string LevelName { get; set; }
        public string LevelSeed { get; set; }
        public string DefaultPlayerPermissionLevel { get; set; }
        public bool TexturepackRequired { get; set; }
        public bool ContentLogFileEnabled { get; set; }
        public double CompressionThreshold { get; set; }
        public bool ServerAuthoritativeMovement { get; set; }
        public double PlayerMovementScoreThreshold { get; set; }
        public double PlayerMovementDistanceThreshold { get; set; }
        public double PlayerMovementDurationThresholdInMs { get; set; }
        public bool CorrectPlayerMovement { get; set; }

        private readonly string propertiesFile;

        /// <summary>
        /// Pass in the content of server.properties
        /// </summary>
        /// <param name="propertiesFile"></param>
        public Properties(string propertiesFile, bool autoSetProperties = true)
        {
            this.propertiesFile = propertiesFile;

            if (autoSetProperties) SetProperties();
        }

        /// <summary>
        /// Gets property name and its value from server.properties file
        /// </summary>
        /// <returns></returns>
        public List<(string propertyName, string propertyValue)> PropertyAndValueFromFile()
        {
            return propertiesFile
                .Split("\n")
                .Select(a => a.Trim())
                .Where(b => !b.StartsWith("#") && b.Length > 0)
                .Select(c => c.Split("="))
                .Select(d => Tuple.Create(d[0], d[1])
                                  .ToValueTuple())
                .ToList();
        }

        /// <summary>
        /// Sets properties of this instance
        /// </summary>
        /// <returns></returns>
        public void SetProperties()
        {
            var propsVals = PropertyAndValueFromFile();

            var properties = this;
            var type = properties.GetType();

            foreach (var (name, value) in propsVals)
            {
                var prop = type.GetProperty(FilePropertyToProperty(name));

                if (double.TryParse(value, out double valueDouble))
                {
                    prop.SetValue(properties, valueDouble);
                }
                else if (bool.TryParse(value, out bool valueBool))
                {
                    prop.SetValue(properties, valueBool);
                }
                else
                {
                    prop.SetValue(properties, value);
                }
            }
        }

        /// <summary>
        /// Converts class property name to format of file property
        /// </summary>
        /// <param name="classProperty"></param>
        /// <returns></returns>
        public static string PropertyToFileProperty(MemberInfo classProperty)
        {
            string propertyName = classProperty.Name;

            string result = "";

            for (int i = 0; i < propertyName.Length; i++)
            {
                if (i == 0)
                {
                    result += char.ToLower(propertyName[i]);
                }
                else if (char.IsUpper(propertyName[i]))
                {
                    result += $"-{char.ToLower(propertyName[i])}";
                }
                else
                {
                    result += propertyName[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Converts name of a property in a server.properties file to a format of class property
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
            string getType(string value)
            {
                string result = "";

                if (double.TryParse(value, out _))
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

                return result;
            }

            return string.Join("\n", PropertyAndValueFromFile()
                .Select(x => $"public {getType(x.propertyValue)} {FilePropertyToProperty(x.propertyName)} {{ get; set; }}"));
        }

        /// <summary>
        /// Returns all propertis in a server.properties format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join("\n",
                this.GetType()
                .GetProperties()
                .Select(x => $"{PropertyToFileProperty(x)}={(x.GetValue(this).GetType() == typeof(bool) ? x.GetValue(this).ToString().ToLower() : x.GetValue(this))}"));
        }
    }
}
