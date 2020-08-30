using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom.Tools.Tests {
  
    public static class CommandlineTests {

        public static void RunAllTests() {
            ParseConfigArgsTest_overwrite();
            ParseConfigArgsTest_simple();
            ParseConfigArgsTest_stacked();
        }
        
        public static void ParseConfigArgsTest_simple() {
            string[] lines = new string[] {
                "--url=test.com",
                "--window-pos-y=100",
                "--injected-javascript=",
                "(function() {console.log('hello');})();",
                "--multiline=",
                "first line",
                "second line",
                "third line",
                "--splitline=zero line",
                "first line",
                "second line",
                "third line",
            };
            Arguments.ParseConfigArgs(lines);

            Assert.AreEqual("test.com", Arguments.GetArgument("url"), "Failed to parse simple value");
            Assert.AreEqual(100, Arguments.GetArgumentInt("window-pos-y", 0), "Failed to parse integer value");
            Assert.AreEqual("(function() {console.log('hello');})();", Arguments.GetArgument("injected-javascript"), "Failed to parse line break correctly");
            Assert.AreEqual("first line\nsecond line\nthird line", Arguments.GetArgument("multiline"), "Failed to parse multiple line breaks correctly");
            Assert.AreEqual("zero line\nfirst line\nsecond line\nthird line", Arguments.GetArgument("splitline"), "Failed to parse multiple line breaks with initial line correctly");
        }

       
        public static void ParseConfigArgsTest_stacked() {
            string[] lines = new string[] {
                "--url=test.com",
                "--on-console-message<first>=(function() {console.log('hello');})();",
                "--on-console-message<second>=(function() {console.log('second');})();",
                "--on-console-message<third>=",
                "(function() {console.log('third');})();",
            };
            Arguments.ParseConfigArgs(lines);

            Assert.AreEqual("test.com", Arguments.GetArgument("url"), "Failed to parse simple value");

            var args = Arguments.GetStackedArguments("on-console-message");
            Assert.AreEqual("(function() {console.log('hello');})();", args["first"], "Failed to get first stacked argument");
            Assert.AreEqual("(function() {console.log('second');})();", args["second"], "Failed to get second stacked argument");
            Assert.AreEqual("(function() {console.log('third');})();", args["third"], "Failed to get third stacked argument");
        }
        

        public static void ParseConfigArgsTest_overwrite() {
            string[] lines = new string[] {
                "--url=test.com",
                "--url=otherone.com",
                "--on-console-message<first>=(function() {console.log('hello');})();",
                "--on-console-message<first>=(function() {console.log('bye');})();",
                "--on-console-message<second>=(function() {console.log('second');})();",
            };
            
            Arguments.ParseConfigArgs(lines);

            Assert.AreEqual("otherone.com", Arguments.GetArgument("url"), "Failed to overwrite argument");
            var args = Arguments.GetStackedArguments("on-console-message");
            Assert.AreEqual("(function() {console.log('bye');})();", args["first"], "Failed to overwrite first stacked argument");
            Assert.AreEqual("(function() {console.log('second');})();", args["second"], "Failed to get second stacked argument");
        }
    }
}