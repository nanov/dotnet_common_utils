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
| DirectAllocation     | HighContention    | 10                  |        25,557.19 ns |      27,911.635 ns |      1,529.931 ns |  1.00 |    0.07 |   30.1514 |  0.3662 |   239123 B |        1.00 |
| ConcurrentObjectPool | HighContention    | 10                  |         8,332.22 ns |       3,569.088 ns |        195.634 ns |  0.33 |    0.02 |    1.0986 |  0.0153 |     8846 B |        0.04 |
| DefaultObjectPool    | HighContention    | 10                  |         7,271.70 ns |       4,915.154 ns |        269.416 ns |  0.29 |    0.02 |    0.4120 |       - |     3458 B |        0.01 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | HighContention    | 50                  |       120,690.47 ns |       8,302.439 ns |        455.085 ns |  1.00 |    0.00 |  148.1934 |  2.1973 |  1180520 B |       1.000 |
| ConcurrentObjectPool | HighContention    | 50                  |        54,692.66 ns |      19,302.416 ns |      1,058.031 ns |  0.45 |    0.01 |   17.7002 |  0.3662 |   138045 B |       0.117 |
| DefaultObjectPool    | HighContention    | 50                  |       125,306.14 ns |      11,731.518 ns |        643.044 ns |  1.04 |    0.01 |    0.4883 |       - |     4705 B |       0.004 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | HighContention    | 100                 |       233,713.00 ns |      27,332.926 ns |      1,498.210 ns |  1.00 |    0.01 |  294.6777 |  4.1504 |  2356471 B |       1.000 |
| ConcurrentObjectPool | HighContention    | 100                 |       121,189.02 ns |       9,497.101 ns |        520.568 ns |  0.52 |    0.00 |   50.1709 |  1.0986 |   397165 B |       0.169 |
| DefaultObjectPool    | HighContention    | 100                 |       298,517.05 ns |      30,865.202 ns |      1,691.826 ns |  1.28 |    0.01 |    0.4883 |       - |     4971 B |       0.002 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | HighContention    | 500                 |     1,192,664.36 ns |     219,710.905 ns |     12,043.096 ns |  1.00 |    0.01 | 1470.7031 | 15.6250 | 11764809 B |       1.000 |
| ConcurrentObjectPool | HighContention    | 500                 |     1,349,392.52 ns |     386,960.396 ns |     21,210.605 ns |  1.13 |    0.02 |  480.4688 | 11.7188 |  3836691 B |       0.326 |
| DefaultObjectPool    | HighContention    | 500                 |     1,590,655.14 ns |     437,021.364 ns |     23,954.616 ns |  1.33 |    0.02 |         - |       - |     5083 B |       0.000 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | MultiThreaded     | 10                  |         7,689.67 ns |       3,881.766 ns |        212.773 ns |  1.00 |    0.03 |   12.0087 |  0.1068 |    96708 B |        1.00 |
| ConcurrentObjectPool | MultiThreaded     | 10                  |         2,709.44 ns |       2,549.009 ns |        139.720 ns |  0.35 |    0.02 |    0.2823 |       - |     2348 B |        0.02 |
| DefaultObjectPool    | MultiThreaded     | 10                  |         2,176.27 ns |         193.784 ns |         10.622 ns |  0.28 |    0.01 |    0.2747 |       - |     2289 B |        0.02 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | MultiThreaded     | 50                  |        40,452.74 ns |       2,967.426 ns |        162.655 ns |  1.00 |    0.00 |   59.3262 |  0.6104 |   473601 B |       1.000 |
| ConcurrentObjectPool | MultiThreaded     | 50                  |         7,718.64 ns |       5,498.991 ns |        301.418 ns |  0.19 |    0.01 |    1.6632 |  0.0229 |    13145 B |       0.028 |
| DefaultObjectPool    | MultiThreaded     | 50                  |        11,263.58 ns |       1,145.912 ns |         62.811 ns |  0.28 |    0.00 |    0.3357 |       - |     2811 B |       0.006 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | MultiThreaded     | 100                 |        78,333.34 ns |      10,307.186 ns |        564.972 ns |  1.00 |    0.01 |  117.6758 |  1.2207 |   944102 B |       1.000 |
| ConcurrentObjectPool | MultiThreaded     | 100                 |        18,087.63 ns |       6,891.267 ns |        377.734 ns |  0.23 |    0.00 |    4.4861 |  0.0610 |    35079 B |       0.037 |
| DefaultObjectPool    | MultiThreaded     | 100                 |        42,302.66 ns |      13,388.310 ns |        733.858 ns |  0.54 |    0.01 |    0.3662 |       - |     3171 B |       0.003 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | MultiThreaded     | 500                 |       371,437.27 ns |      93,791.837 ns |      5,141.047 ns |  1.00 |    0.02 |  588.3789 |  6.8359 |  4707432 B |       1.000 |
| ConcurrentObjectPool | MultiThreaded     | 500                 |       219,910.53 ns |      13,113.531 ns |        718.797 ns |  0.59 |    0.01 |  112.0605 |  1.7090 |   894730 B |       0.190 |
| DefaultObjectPool    | MultiThreaded     | 500                 |       387,976.89 ns |      16,253.625 ns |        890.916 ns |  1.04 |    0.01 |         - |       - |     3495 B |       0.001 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorld         | 10                  |    32,558,515.62 ns |   6,733,852.298 ns |    369,105.168 ns |  1.00 |    0.01 |         - |       - |    11806 B |       1.000 |
| ObjectPool           | RealWorld         | 10                  |    32,795,471.79 ns |  16,450,366.522 ns |    901,700.102 ns |  1.01 |    0.03 |         - |       - |       28 B |       0.002 |
| DefaultObjectPool    | RealWorld         | 10                  |    32,585,486.96 ns |  11,013,228.665 ns |    603,672.228 ns |  1.00 |    0.02 |         - |       - |       46 B |       0.004 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorld         | 50                  |   160,045,902.75 ns |   8,086,786.850 ns |    443,264.077 ns |  1.00 |    0.00 |         - |       - |    58984 B |       1.000 |
| ObjectPool           | RealWorld         | 50                  |   161,670,298.50 ns |  23,333,159.798 ns |  1,278,969.228 ns |  1.01 |    0.01 |         - |       - |      184 B |       0.003 |
| DefaultObjectPool    | RealWorld         | 50                  |   163,087,333.33 ns |  40,384,871.488 ns |  2,213,631.088 ns |  1.02 |    0.01 |         - |       - |      184 B |       0.003 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorld         | 100                 |   323,327,757.17 ns |  27,967,014.468 ns |  1,532,966.440 ns |  1.00 |    0.01 |         - |       - |   117968 B |       1.000 |
| ObjectPool           | RealWorld         | 100                 |   322,763,687.33 ns |  64,355,613.182 ns |  3,527,548.331 ns |  1.00 |    0.01 |         - |       - |      368 B |       0.003 |
| DefaultObjectPool    | RealWorld         | 100                 |   322,310,173.83 ns |  45,979,567.846 ns |  2,520,295.275 ns |  1.00 |    0.01 |         - |       - |      200 B |       0.002 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorld         | 500                 | 1,627,479,375.33 ns |  84,474,816.983 ns |  4,630,349.785 ns |  1.00 |    0.00 |         - |       - |   588736 B |       1.000 |
| ObjectPool           | RealWorld         | 500                 | 1,626,226,291.67 ns | 207,572,774.353 ns | 11,377,764.231 ns |  1.00 |    0.01 |         - |       - |      736 B |       0.001 |
| DefaultObjectPool    | RealWorld         | 500                 | 1,633,075,319.00 ns | 204,077,622.557 ns | 11,186,183.167 ns |  1.00 |    0.01 |         - |       - |      736 B |       0.001 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorldParallel | 10                  |    32,594,273.44 ns |   3,675,556.780 ns |    201,469.671 ns |  1.00 |    0.01 |         - |       - |    97632 B |        1.00 |
| ConcurrentObjectPool | RealWorldParallel | 10                  |    32,338,241.33 ns |   1,279,076.776 ns |     70,110.514 ns |  0.99 |    0.01 |         - |       - |     3540 B |        0.04 |
| DefaultObjectPool    | RealWorldParallel | 10                  |    32,890,080.73 ns |   3,530,833.335 ns |    193,536.890 ns |  1.01 |    0.01 |         - |       - |     3536 B |        0.04 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorldParallel | 50                  |   161,387,374.92 ns |  13,073,458.245 ns |    716,600.364 ns |  1.00 |    0.01 |         - |       - |   474216 B |       1.000 |
| ConcurrentObjectPool | RealWorldParallel | 50                  |   162,980,073.00 ns |  18,532,913.636 ns |  1,015,851.537 ns |  1.01 |    0.01 |         - |       - |     3480 B |       0.007 |
| DefaultObjectPool    | RealWorldParallel | 50                  |   162,360,982.75 ns |  14,422,459.158 ns |    790,543.656 ns |  1.01 |    0.01 |         - |       - |     3768 B |       0.008 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorldParallel | 100                 |   324,548,652.67 ns |  34,700,258.880 ns |  1,902,038.288 ns |  1.00 |    0.01 |         - |       - |   944872 B |       1.000 |
| ConcurrentObjectPool | RealWorldParallel | 100                 |   323,015,284.83 ns |   6,995,992.576 ns |    383,473.962 ns |  1.00 |    0.01 |         - |       - |     4104 B |       0.004 |
| DefaultObjectPool    | RealWorldParallel | 100                 |   321,360,298.67 ns |  33,554,001.251 ns |  1,839,208.039 ns |  0.99 |    0.01 |         - |       - |     4200 B |       0.004 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | RealWorldParallel | 500                 | 1,628,327,527.67 ns | 305,227,388.226 ns | 16,730,543.160 ns |  1.00 |    0.01 |         - |       - |  4708904 B |       1.000 |
| ConcurrentObjectPool | RealWorldParallel | 500                 | 1,610,366,861.33 ns | 164,091,720.941 ns |  8,994,420.964 ns |  0.99 |    0.01 |         - |       - |     4904 B |       0.001 |
| DefaultObjectPool    | RealWorldParallel | 500                 | 1,610,091,777.33 ns |  49,564,243.949 ns |  2,716,783.469 ns |  0.99 |    0.01 |         - |       - |     3944 B |       0.001 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | SingleThreaded    | 10                  |           331.58 ns |          31.588 ns |          1.731 ns |  1.00 |    0.01 |    1.4057 |       - |    11760 B |        1.00 |
| ObjectPool           | SingleThreaded    | 10                  |            29.11 ns |           9.318 ns |          0.511 ns |  0.09 |    0.00 |         - |       - |          - |        0.00 |
| DefaultObjectPool    | SingleThreaded    | 10                  |            38.31 ns |           3.932 ns |          0.216 ns |  0.12 |    0.00 |         - |       - |          - |        0.00 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | SingleThreaded    | 50                  |         1,665.53 ns |         203.177 ns |         11.137 ns |  1.00 |    0.01 |    7.0286 |       - |    58800 B |        1.00 |
| ObjectPool           | SingleThreaded    | 50                  |           144.42 ns |           5.386 ns |          0.295 ns |  0.09 |    0.00 |         - |       - |          - |        0.00 |
| DefaultObjectPool    | SingleThreaded    | 50                  |           209.31 ns |         117.665 ns |          6.450 ns |  0.13 |    0.00 |         - |       - |          - |        0.00 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | SingleThreaded    | 100                 |         3,406.30 ns |         383.113 ns |         21.000 ns |  1.00 |    0.01 |   14.0572 |       - |   117600 B |        1.00 |
| ObjectPool           | SingleThreaded    | 100                 |           252.98 ns |          47.461 ns |          2.601 ns |  0.07 |    0.00 |         - |       - |          - |        0.00 |
| DefaultObjectPool    | SingleThreaded    | 100                 |           382.52 ns |          85.314 ns |          4.676 ns |  0.11 |    0.00 |         - |       - |          - |        0.00 |
|                      |                   |                     |                     |                    |                   |       |         |           |         |            |             |
| DirectAllocation     | SingleThreaded    | 500                 |        18,328.08 ns |         512.780 ns |         28.107 ns |  1.00 |    0.00 |   70.2820 |       - |   588000 B |        1.00 |
| ObjectPool           | SingleThreaded    | 500                 |         1,441.75 ns |          38.048 ns |          2.086 ns |  0.08 |    0.00 |         - |       - |          - |        0.00 |
| DefaultObjectPool    | SingleThreaded    | 500                 |         2,030.64 ns |       2,500.373 ns |        137.054 ns |  0.11 |    0.01 |         - |       - |          - |        0.00 |
