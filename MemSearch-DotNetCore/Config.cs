using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace MemSearch
{
    internal static class Config
    {
        public const string CONFIG_PATH = "inputs.yaml";

        /// <summary>
        /// As you may have guessed, this loads the configuration. Either from a base64 string or CONFIG_PATH in the working directory
        /// </summary>
        /// <param name="base64Config"></param>
        /// <returns></returns>
        public static IEnumerable<Input> Load(String base64Config = null)
        {
            List<Input> res = new List<Input>();
            YamlStream ys = new YamlStream();
            if (string.IsNullOrEmpty(base64Config))
            {
                if (!File.Exists(CONFIG_PATH))
                    throw new FileNotFoundException($"Config file not found {CONFIG_PATH}");

                using (StreamReader sr = new StreamReader(CONFIG_PATH))
                    ys.Load(sr);
            }
            else
            {
                byte[] unBase64CharArray = Convert.FromBase64String(base64Config);
                String configStr = Encoding.ASCII.GetString(unBase64CharArray);
                ys.Load(new StringReader(configStr));
            }

            var actualRootNode = ys.Documents[0].RootNode;

            IEnumerable<YamlNode> rootNode;
            if (actualRootNode is YamlSequenceNode)
                rootNode = (YamlSequenceNode)actualRootNode;
            else
                throw new FormatException("Input YAML Base64 is invalid: should be list");

            foreach (var node in rootNode)
            {
                // Get all the raw values
                String title = GetYamlValue(node, "title");
                String marker = GetYamlValue(node, "marker");
                String regex = GetYamlValue(node, "regex");
                String urlDecode = GetYamlValue(node, "url_decode");

                res.Add(new Input()
                {
                    Title = title,
                    Marker = marker,
                    Regex = regex,
                    UrlDecode = string.IsNullOrEmpty(urlDecode) ? false : Boolean.Parse(urlDecode)
                });
            }

            return res;
        }
        private static YamlNode GetYamlNode(YamlNode p, string name)
        {
            if (p == null)
                return null;
            try
            {
                return p[new YamlScalarNode(name)];
            }
            catch (KeyNotFoundException)
            {
                // Silently ignore...
                // Why can't it just return null...?
            }
            return null;
        }

        private static string GetYamlValue(YamlNode p, string name, object def = null)
        {
            var n = GetYamlNode(p, name);
            return n == null ? def?.ToString() : ((YamlScalarNode)n).Value;
        }

    }
}
