# Benchmarker

This tool is used to gauge memory and startup timings in a very crude manner.

It's not meant to serve as a benchmark for platform performance, but rather as a tool to help us to identify performance
of the framework on startup and after heavy load.

It should only be ran on major changes that may affect Startup or high load runtime performance and does not replace
the test suite.

For actual performance data, refer to TechEmpower.

Until Reaper is added officially, local machine (OSX, M1 Ultra, 128GB RAM) test results are available [here for JSON](https://www.techempower.com/benchmarks/#section=test&shareid=75585734-6c92-4a79-8cc9-dab0979ffb38&hw=ph&test=json)
and [here for Plaintext](https://www.techempower.com/benchmarks/#section=test&shareid=75585734-6c92-4a79-8cc9-dab0979ffb38&hw=ph&test=plaintext).