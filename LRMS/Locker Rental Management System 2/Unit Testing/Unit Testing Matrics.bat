REM Create a 'GeneratedReports' folder if it does not exist
if not exist "%~dp0GeneratedReports" mkdir "%~dp0GeneratedReports"

REM Run the tests against the targeted output
call :RunOpenCoverUnitTestMetrics

REM Generate the report output based on the test results
if %errorlevel% equ 0 (
 call :RunReportGeneratorOutput
)

REM Launch the report
if %errorlevel% equ 0 (
 call :RunLaunchReport
)
exit /b %errorlevel%

:RunOpenCoverUnitTestMetrics
"D:\Visual Studio Projects\Workshop1\Locker Rental Management System 2\packages\OpenCover.4.7.922\tools\OpenCover.Console.exe" ^
-register:user ^
-target:"%D:\Visual Studio Projects\Workshop1\Locker Rental Management System 2\Locker Rental Management System 2\bin\Release\Locker Rental Management System 2.exe" ^
-output:"%~dp0\GeneratedReports\LRMSReport.xml"
exit /b %errorlevel%
 
:RunReportGeneratorOutput
"D:\Visual Studio Projects\Workshop1\Locker Rental Management System 2\packages\ReportGenerator.4.0.15\tools\net47\ReportGenerator.exe" ^
-reports:"%~dp0\GeneratedReports\LRMSReport.xml" ^
-targetdir:"%~dp0\GeneratedReports\ReportGenerator Output"
exit /b %errorlevel%
 
:RunLaunchReport
start "report" "%~dp0\GeneratedReports\ReportGenerator Output\index.htm"
exit /b %errorlevel%