using System;
using System.Collections.Generic;
using System.IO;
//using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lab_4_Test_Generator
{

    class TestGeneratorHandler
    {
        TestGeneratorLib.TestGenerator testGenerator;
        public void GenerateTestsForClass(ClassInfo classInfo) {

        }
        public async Task GenerateTestsForFile(String filePath)
        {
            IList<ClassInfo> classes = new List<ClassInfo>();
            //get classes in file
            int classesCount = classes.Count;
            foreach (ClassInfo classInfo in classes)
            {

                    GenerateTestsForClass(
            }
        }
        public void GenerateTestsForDir(String dirPath)
        {
            //get files list
            String[] sourceFiles = Directory.GetFiles(dirPath);
            if (sourceFiles.Length > 0)
            {
                foreach (String filePath in sourceFiles)
                {
                    TransformBlock<string, string> getFileSourceCode = new TransformBlock<>
                    GenerateTestsForFile(filePath);
                    /*}*/
                }
            } else
            {
                //print "Empty directory"
            }
        }
        public TestGeneratorHandler()
        {
            testGenerator = new TestGeneratorLib.TestGenerator();
        }
    }
    class Program
    {
        
        static void Main(string[] args)
        {
            //Get file path
            String dirPath = "";
            //
            TestGeneratorHandler tdh = new TestGeneratorHandler();
            tdh.GenerateTestsForDir(dirPath);
        }
    }
}
