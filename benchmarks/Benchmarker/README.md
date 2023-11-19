# Benchmarker

This tool is used to gauge memory and startup timings in a very crude manner.

It's not meant to serve as a benchmark for platform performance, but rather as a tool to help us to identify performance
of the framework on startup and after heavy load.

It should only be ran on major changes that may affect Startup or high load runtime performance and does not replace
the test suite.

Guidance for interpreting results:
- We *always* want to be faster, lower memory, and higher req/s than `controllers`. That is the ultimate goal.
- Ideally, we want to be very close to `minimal`.
- In AOT land, we should always be close to `minimal-aot`.

Sample run:

| Framework     | Startup Time | Memory Usage (MiB) - Startup | Memory Usage (MiB) - Load Test | Requests/sec |
|---------------|--------------|------------------------------|--------------------------------|--------------|
| carter        | 115          | 23.1                         | 269.6                          | 121725.32    |
| controllers   | 143          | 24.14                        | 308.9                          | 106056.19    |
| fastendpoints | 134          | 23.86                        | 303.6                          | 118512.82    |
| minimal       | 103          | 21.68                        | 258.2                          | 123264.17    |
| minimal-aot   | 21           | 20.81                        | 26.96                          | 144059.81    |
| reaper        | 109          | 20.41                        | 294.2                          | 121946.15    |
| reaper-aot    | 21           | 18.89                        | 30.83                          | 139910.28    |

For actual performance data, refer to TechEmpower.

Until Reaper is added officially, local machine (OSX, M1 Ultra, 128GB RAM) test results are available [here for JSON](https://www.techempower.com/benchmarks/#section=test&shareid=75585734-6c92-4a79-8cc9-dab0979ffb38&hw=ph&test=json)
and [here for Plaintext](https://www.techempower.com/benchmarks/#section=test&shareid=75585734-6c92-4a79-8cc9-dab0979ffb38&hw=ph&test=plaintext).