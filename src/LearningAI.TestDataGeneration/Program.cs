var rootDirectoryInfo = new DirectoryInfo("TestData/Knowledgebase");

var processingQueue = new Queue<DirectoryInfo>();

processingQueue.Enqueue(rootDirectoryInfo);

while (processingQueue.Count > 0)
{
    var directoryInfo = processingQueue.Dequeue();

    foreach (var subdir in directoryInfo.GetDirectories())
    {
        processingQueue.Enqueue(subdir);
    }

    foreach (var file in directoryInfo.GetFiles())
    {
        Console.WriteLine(file.FullName);
    }
}

Console.WriteLine();