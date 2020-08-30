using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScChrom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScChrom_Tests.CommandLineArguments {


    public static class ContextmenuTests {
        public static void RunAllTests() {
            CreateCustomContextMenu();
        }

        public static void CreateCustomContextMenu() {
            
            string testScript = @"
                --prevent-pagechange=true              
                --on-before-contextmenu=
                    let ret = {
                        'clear' : true,
                        'entries' : [
                            {
                                'text' : 'click me',
                                'id' : 1
                            },
                            {
                                'text' : 'not me!',
                                'id' : 2
                            },
                            {
                                'text' : '',
                                'id' : 0,
                                'type' : 'separator'
                            },
                            {
                                'text' : 'cb1',
                                'id' : 4,
                                'type' : 'checkbox'
                            },
                            {
                                'text' : 'cb2',
                                'id' : 4,
                                'type' : 'checkbox',
                                'checked' : true
                            },
                            {
                                'text' : 'not me!',
                                'id' : 2                                
                            },
                        ]
                    };
                    return JSON.stringify(ret);
                --on-click-contextmenu=
                    write(clicked_id);
                    WindowController.closeMainwindow();
                
                --url=" + Program.GetScChrom_Test_html_file_url();


            List<string> lines = new List<string>();
            MainController.WrittenOut += (string content) => {
                lines.Add(content);
            };

            var args = Program.GetDefaultConfig(false);
            args.AddRange(ScChrom.Tools.Arguments.GetScriptLines(testScript));
            Program.ShowBrowserBlocking(args.ToArray());


            Assert.AreEqual(1, lines.Count, "Unnecessary lines written");
            Assert.AreEqual("1", lines[0], "Wrong item clicked");
        }
        

    }
}
