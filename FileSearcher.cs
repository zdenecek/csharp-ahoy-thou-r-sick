using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhoCorasick;


using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class FileSearcher
{

    public FileSearcher(int searchWorkerCount, int maxQueueSize)
    {
        SearchWorkerCount = searchWorkerCount;
        MaxQueueSize = maxQueueSize;
    }

    public int SearchWorkerCount { get; }
    public int MaxQueueSize { get; }


    public int ProcessedFilesCount { get => processedFilesCount; private set => processedFilesCount = value; }
    private List<string> foundFiles = new();
    public IReadOnlyList<string> FoundFiles { get => foundFiles.AsReadOnly(); }

    private bool ended = false;

    public event Action? OnSearchComplete;
    public event Action<string>? OnFileFound;

    private byte[][] transformNeedleToBytes(string needle)
    {
        Encoding[] encodings = { Encoding.ASCII, Encoding.UTF8, Encoding.Unicode };

        byte[][] transformed = new byte[encodings.Length][];

        for (int i = 0; i < encodings.Length; i++)
        {
            transformed[i] = encodings[i].GetBytes(needle);
        }

        return transformed;
    }

    public void Search(string directoryPath, string needle)
    {
        var needleBytes = transformNeedleToBytes(needle);

        CreateProcessorWorkers(SearchWorkerCount, needleBytes);
        CrawlDirectory(directoryPath);

        lock (queueLock)
        {
            while (fileQueue.Count > 0)
                Monitor.Wait(queueLock);

            ended = true;
        }

        for (int i = 0; i < SearchWorkerCount; i++)
        {
            workerThreads![i].Join();
        }

        OnSearchComplete?.Invoke();
    }


    private readonly object queueLock = new object();
    private Queue<string> fileQueue = new Queue<string>();


    private Thread[]? workerThreads;
    private int processedFilesCount = 0;

    public void CreateProcessorWorkers(int numWorkerThreads, byte[][] needle)
    {
        workerThreads = new Thread[numWorkerThreads];

        for (int i = 0; i < numWorkerThreads; i++)
        {
            workerThreads[i] = new Thread(() => ProcessFile(needle));
            workerThreads[i].Start();
        }
    }

    public void CrawlDirectory(string directoryPath)
    {
        string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            lock (queueLock)
            {
                // Wait until the queue has capacity
                while (fileQueue.Count >= MaxQueueSize)
                {
                    Monitor.Wait(queueLock);
                }

                // Add file to the queue and notify waiting threads
                fileQueue.Enqueue(file);
                Monitor.PulseAll(queueLock);
            }
        }

        lock (queueLock)
        {
            // Notify waiting threads that no more files will be added
            ProcessedFilesCount++;
            Monitor.PulseAll(queueLock);
        }
    }

    private void ProcessFile(byte[][] needle)
    {
        while (!ended)
        {
            string file;

            lock (queueLock)
            {
                // Wait until there are files in the queue or all files have been processed
                while (fileQueue.Count == 0 && ProcessedFilesCount == 0)
                {
                    Monitor.Wait(queueLock);
                }

                // Check if all files have been processed and exit if true
                if (fileQueue.Count == 0 && ProcessedFilesCount > 0)
                {
                    return;
                }

                // Get file from the queue and notify waiting threads
                file = fileQueue.Dequeue();
                Monitor.PulseAll(queueLock);
            }

            // Process the file (replace this with your actual file processing logic)
            SearchInFile(file);
        }

        void SearchInFile(string file)
        {
            using (FileStream f = File.OpenRead(file))
            {

                Interlocked.Increment(ref processedFilesCount);

                if (AhoKodak.Matches(f, needle))
                {
                    lock (foundFiles)
                    {
                        foundFiles.Add(file);
                    }

                    OnFileFound?.Invoke(file);
                }
            }
        }
    }


}