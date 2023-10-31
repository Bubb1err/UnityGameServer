@echo off 
for /L %%i in (1,1,5) do (
    start "Instance %%i" /D "C:\Users\timce\source\repos\UnityGameServer\ClientExample\bin\Debug\net7.0" dotnet ClientExample.dll
)