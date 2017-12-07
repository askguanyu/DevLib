@echo off

set "PROJ=DevLib.Metadata"
set "CONFIG=Debug"

@echo --------------------------------
@echo NuGet start to pack...

nuget pack "..\..\Source\%PROJ%\%PROJ%.csproj" -Properties "Configuration=%CONFIG%" -OutputDirectory "..\..\Source\%PROJ%\bin\%CONFIG%"


@echo --------------------------------
@echo List nupkg files...

dir "..\..\Source\%PROJ%\bin\%CONFIG%\*.nupkg"

for /f "delims=" %%x in ('dir /od /b "..\..\Source\%PROJ%\bin\%CONFIG%\%PROJ%*.nupkg"') do set LATEST=%%x

if %Errorlevel% NEQ 0 goto End


@echo --------------------------------

choice /M "Do you want to publish %LATEST% ?"

if Errorlevel 2 goto No
if Errorlevel 1 goto Yes

goto End

:No
goto End

:Yes
nuget push "..\..\Source\%PROJ%\bin\%CONFIG%\%LATEST%" %1 -Verbosity detailed

:End
@echo --------------------------------
@echo Done!