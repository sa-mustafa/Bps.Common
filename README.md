# Bps.Common

This repo contains the most common utility needed for most of the projects I did recently. Each project required:

- Exception handling & logging mostly with [NLog](https://github.com/NLog/NLog).
- Handling single instance running for services
- Flexible threading mechanism with Processor & Processors
- and object pooling to avoid memory fragmentation.

This project is battle tested initially in many projects: MAFIS and its variants, DBScan-v1 & DBScan-v2. These projects performed biometric enrollment & identification on facial images.

The threading system in this project is the main point of this project and provides you with a system way stronger than .Net [Threading](https://docs.microsoft.com/en-us/dotnet/standard/threading/using-threads-and-threading), [ThreadPool](https://docs.microsoft.com/en-us/dotnet/api/system.threading.threadpool?view=netframework-4.8), [Task Parallel Library](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl) (TPL) mechanisms .