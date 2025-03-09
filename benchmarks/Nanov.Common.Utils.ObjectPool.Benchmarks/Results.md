// * Summary *

BenchmarkDotNet v0.14.0, macOS Sequoia 15.3.1 (24D70) [Darwin 24.3.0]
Apple M4 Max, 1 CPU, 14 logical and 14 physical cores
.NET SDK 9.0.200
[Host]   : .NET 9.0.2 (9.0.225.6610), Arm64 RyuJIT AdvSIMD
ShortRun : .NET 9.0.2 (9.0.225.6610), Arm64 RyuJIT AdvSIMD

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3

| Method               | Categories        | OperationsPerInvoke | Mean                | Error              | StdDev            | Ratio | RatioSD | Gen0      | Gen1    | Allocated  | Alloc Ratio |
|--------------------- |------------------ |-------------------- |--------------------:|-------------------:|------------------:|------:|--------:|----------:|--------:|-----------:|------------:|
| DirectAllocation     | HighContention    | 10                  |        22,182.64 ns |       3,129.927 ns |        171.562 ns |  1.00 |    0.01 |   30.0903 |  0.3662 |   239150 B |        1.00 |
| ConcurrentObjectPool | HighContention    | 10                  |         9,009.82 ns |       5,131.929 ns |        281.298 ns |  0.41 |    0.01 |    1.2665 |  0.0153 |    10200 B |        0.04 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | HighContention    | 100                 |       230,445.83 ns |       3,556.347 ns |        194.935 ns |  1.00 |    0.00 |  294.4336 |  4.3945 |  2356488 B |        1.00 |
| ConcurrentObjectPool | HighContention    | 100                 |       257,213.48 ns |      83,973.387 ns |      4,602.865 ns |  1.12 |    0.02 |   79.1016 |  1.9531 |   628431 B |        0.27 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | HighContention    | 1000                |     2,388,547.53 ns |     392,946.012 ns |     21,538.697 ns |  1.00 |    0.01 | 2941.4063 | 15.6250 | 23524904 B |        1.00 |
| ConcurrentObjectPool | HighContention    | 1000                |     2,843,014.92 ns |   1,435,384.262 ns |     78,678.255 ns |  1.19 |    0.03 |  921.8750 | 19.5313 |  7353176 B |        0.31 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | MultiThreaded     | 10                  |         7,974.86 ns |      12,263.059 ns |        672.180 ns |  1.01 |    0.11 |   12.0087 |  0.1068 |    96717 B |        1.00 |
| ConcurrentObjectPool | MultiThreaded     | 10                  |         2,529.38 ns |       1,162.417 ns |         63.716 ns |  0.32 |    0.03 |    0.2823 |       - |     2359 B |        0.02 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | MultiThreaded     | 100                 |        78,732.65 ns |      61,383.442 ns |      3,364.634 ns |  1.00 |    0.05 |  117.6758 |  1.2207 |   944114 B |        1.00 |
| ConcurrentObjectPool | MultiThreaded     | 100                 |        25,810.92 ns |      11,234.313 ns |        615.791 ns |  0.33 |    0.01 |    6.7749 |  0.0916 |    52878 B |        0.06 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | MultiThreaded     | 1000                |       723,201.82 ns |     249,091.984 ns |     13,653.572 ns |  1.00 |    0.02 | 1175.7813 | 11.7188 |  9411460 B |        1.00 |
| ConcurrentObjectPool | MultiThreaded     | 1000                |       646,442.93 ns |     269,761.542 ns |     14,786.540 ns |  0.89 |    0.02 |  250.9766 |  3.9063 |  2006742 B |        0.21 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorld         | 10                  |    32,574,072.51 ns |  11,929,737.215 ns |    653,909.154 ns |  1.00 |    0.02 |         - |       - |    11806 B |       1.000 |
| ObjectPool           | RealWorld         | 10                  |    32,475,152.81 ns |   8,344,767.760 ns |    457,404.881 ns |  1.00 |    0.02 |         - |       - |       28 B |       0.002 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorld         | 100                 |   323,451,499.83 ns |  32,186,521.498 ns |  1,764,251.859 ns |  1.00 |    0.01 |         - |       - |   117968 B |       1.000 |
| ObjectPool           | RealWorld         | 100                 |   324,736,528.00 ns |  18,973,961.973 ns |  1,040,026.885 ns |  1.00 |    0.01 |         - |       - |      368 B |       0.003 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorld         | 1000                | 3,226,237,513.33 ns | 192,982,334.496 ns | 10,578,012.986 ns |  1.00 |    0.00 |         - |       - |  1176736 B |       1.000 |
| ObjectPool           | RealWorld         | 1000                | 3,243,915,861.00 ns | 111,861,796.483 ns |  6,131,522.551 ns |  1.01 |    0.00 |         - |       - |      736 B |       0.001 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorldParallel | 10                  |    32,457,845.48 ns |   1,830,151.595 ns |    100,316.785 ns |  1.00 |    0.00 |         - |       - |    97596 B |        1.00 |
| ConcurrentObjectPool | RealWorldParallel | 10                  |    32,234,770.85 ns |  11,175,681.669 ns |    612,576.825 ns |  0.99 |    0.02 |         - |       - |     3540 B |        0.04 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorldParallel | 100                 |   324,164,361.17 ns |  20,512,247.513 ns |  1,124,345.506 ns |  1.00 |    0.00 |         - |       - |   944840 B |       1.000 |
| ConcurrentObjectPool | RealWorldParallel | 100                 |   323,094,222.17 ns |  27,746,904.285 ns |  1,520,901.458 ns |  1.00 |    0.01 |         - |       - |     4216 B |       0.004 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorldParallel | 1000                | 3,204,786,361.33 ns | 132,423,747.907 ns |  7,258,592.496 ns |  1.00 |    0.00 | 1000.0000 |       - |  9412904 B |       1.000 |
| ConcurrentObjectPool | RealWorldParallel | 1000                | 3,226,928,653.00 ns | 824,097,583.588 ns | 45,171,569.532 ns |  1.01 |    0.01 |         - |       - |     4904 B |       0.001 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | SingleThreaded    | 10                  |           349.78 ns |         234.752 ns |         12.868 ns |  1.00 |    0.04 |    1.4057 |       - |    11760 B |        1.00 |
| ObjectPool           | SingleThreaded    | 10                  |            27.87 ns |           6.144 ns |          0.337 ns |  0.08 |    0.00 |         - |       - |          - |        0.00 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | SingleThreaded    | 100                 |         3,305.03 ns |         421.114 ns |         23.083 ns |  1.00 |    0.01 |   14.0572 |       - |   117600 B |        1.00 |
| ObjectPool           | SingleThreaded    | 100                 |           251.69 ns |          29.065 ns |          1.593 ns |  0.08 |    0.00 |         - |       - |          - |        0.00 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | SingleThreaded    | 1000                |        33,378.91 ns |       1,992.466 ns |        109.214 ns |  1.00 |    0.00 |  140.5640 |       - |  1176000 B |        1.00 |
| ObjectPool           | SingleThreaded    | 1000                |         2,617.70 ns |       1,511.913 ns |         82.873 ns |  0.08 |    0.00 |         - |       - |          - |        0.00 |

