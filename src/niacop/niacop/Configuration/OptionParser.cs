using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using niacop.Services;

namespace niacop.Configuration {
    public class OptionParser {
        // - regexes
        private Regex nodeRegex = new Regex(@"\[(\w+)\]");
        private Regex setRegex = new Regex(@"(\w+)\s*=\s*(\w+)");

        // - data
        public string node;
        public Dictionary<string, string> values = new Dictionary<string, string>();

        public void parse(string configFileContent) {
            var lines = configFileContent.Split('\n');
            for (var lc = 0; lc < lines.Length; lc++) {
                var step = lines[lc].Trim();
                if (step.StartsWith("#")) {
                    continue;
                }

                try {
                    // try to parse node
                    var nodeInfo = nodeRegex.Match(step);
                    if (nodeInfo.Success) {
                        node = nodeInfo.Groups[1].Value;
                        continue;
                    }

                    // parse option set
                    var setInfo = setRegex.Match(step);
                    if (setInfo.Success) {
                        var varName = setInfo.Groups[1].Value;
                        var varValue = setInfo.Groups[2].Value;
                        var key = node == null ? varName : node + "." + varName;
                        values[key] = varValue;
                    }
                } catch (Exception e) {
                    Logger.log($"error parsing config at line {lc}: {step}\n{e}", Logger.Level.Error);
                    throw;
                }
            }
        }

        public string get(string key) {
            return values[key];
        }
    }
}