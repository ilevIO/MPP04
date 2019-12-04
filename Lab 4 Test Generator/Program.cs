using System;
using System.Collections.Generic;
using System.IO;
//using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGeneratorLib;

namespace Lab_4_Test_Generator
{

    class TestGeneratorHandler
    {
        TestGeneratorLib.TestGenerator testGenerator;
        public async Task GenerateTestsForDir(String dirPath, int simultFileLoadLimit, int simultTasksLimit, 
            int simultFileWriteLimit)
        {
            /*iterating over directory load files' data asynchronously*/
            /*pass this data to TestGenerator and get generated test source ?asynchronously?*/
            /*write new files with received source asynchronously*/


            //create result directory 
            DirectoryInfo directory = Directory.CreateDirectory(dirPath+"\\Generated Tests");
            //
            TransformBlock<string, string> getFileSource = new TransformBlock<string, string>(
                async (path) =>
                {
                    string fileSource = "";
                    StreamReader fileIn = File.OpenText(path);
                    fileSource = await fileIn.ReadToEndAsync();
                    return fileSource;
                }
            , new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = simultFileLoadLimit
            });

            ActionBlock<IList<TestGeneratorLib.ClassInfo>> writeClasses 
                = new ActionBlock<IList<TestGeneratorLib.ClassInfo>>(
                async (classes) =>
                {
                    foreach(TestGeneratorLib.ClassInfo classInfo in classes)
                    {
                        //pid = fork()
                        //if (pid == 0) {
                        //return classInfo
                        //}
                        using (StreamWriter writer = File.CreateText(directory.FullName +"\\"+ classInfo.name+"_Test.cs"))
                        {
                            await writer.WriteAsync(classInfo.generatedCode);
                        }
                    }
                }
                , new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = simultFileWriteLimit
                }
            );
            IList<ClassInfo> classList = new List<ClassInfo>();
            var temp = new TransformBlock<string, IList<ClassInfo>>((code) =>
            {
                SourceFile sourceFile = new SourceFile(code);
                IList<ClassInfo> classes = new List<ClassInfo>();
                foreach (SourceNamespace sourceNamespace in sourceFile.namespaces)
                {
                    foreach (SourceClass sourceClass in sourceNamespace.classes)
                    {
                        classes.Add(new ClassInfo(sourceClass.name, testGenerator.GenerateTestsForClass(sourceClass.classInfo).ToString()));

                        Console.Out.WriteLine("                                           ");
                        Console.Out.WriteLine("                                           ");
                        Console.Out.WriteLine("                                           ");
                        Console.Out.WriteLine("||||||||||||||||||||||||||||||||||||||||||");
                        Console.Out.Write(classes[classes.Count - 1].generatedCode);
                    }
                }
                return classes;
                /*MSTestGenerator generator = new MSTestGenerator();
                string generatedCode = generator.Generate(code).ToString();

                string testClassName = ExtractClassName(generatedCode);
                return new ClassInfo(testClassName, generatedCode);*/
            }, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = simultTasksLimit
            });//testGenerator.GenerateTestsForSource;
            getFileSource.LinkTo(temp, new DataflowLinkOptions() { PropagateCompletion = true });
            temp.LinkTo(writeClasses, new DataflowLinkOptions() { PropagateCompletion = true });

            //get files list
            String[] sourceFiles = Directory.GetFiles(dirPath);
            Console.WriteLine(sourceFiles);
            if (sourceFiles.Length > 0)
            {
                foreach (String filePath in sourceFiles)
                {
                    getFileSource.Post(filePath);
                    //GenerateTestsForFile(filePath);
                    /*}*/
                }
            }
            else
            {
                Console.WriteLine("Empty directory");
            }
            getFileSource.Complete();
            await writeClasses.Completion;
        }
        public TestGeneratorHandler(int tasksLimit)
        {
            testGenerator = new TestGeneratorLib.TestGenerator(tasksLimit);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //Get file path
            String dirPath = @"C:\\TestDirectory";
            int simultFileLoadLimit = 2;
            int simultTasksLimit = 2;
            int simultFileWriteLimit = 2;
            //Get 
            TestGeneratorHandler tdh = new TestGeneratorHandler(simultTasksLimit);
            tdh.GenerateTestsForDir(dirPath, simultFileLoadLimit, simultTasksLimit, simultFileWriteLimit).Wait();
        }
    }
}
