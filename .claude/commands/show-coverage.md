Run the coverage script and display the results inline.

Steps:
1. Run `dotnet tool restore` from the repo root to ensure reportgenerator is available.
2. Run `.\coverage.ps1` to execute all tests with coverage collection and generate the HTML report.
   - If the user specified a service, run `.\coverage.ps1 -Service <ServiceName>` instead.
3. Read `coverage-report/Summary.txt` and display the coverage percentages inline.
4. Tell the user the HTML report is open in their browser at `coverage-report/index.html`.
5. If any test projects failed, list them.
