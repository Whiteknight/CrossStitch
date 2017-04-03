set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe"

%msbuild% CrossStitch.Http.NancyFx.csproj /t:Build /p:Configuration="Release"
::C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild CrossStitch.Http.NancyFx.csproj /t:Build /p:Configuration="Release 4.5"
%msbuild% CrossStitch.Http.NancyFx.csproj /t:Package