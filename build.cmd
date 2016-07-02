@REM "%ProgramFiles%\Git\bin\git.exe" submodule update --init --recursive
@REM "%ProgramFiles%\Git\bin\git.exe" submodule sync
@REM nuget restore <>.sln

@set msbuild="%ProgramFiles(x86)%\msbuild\14.0\Bin\MSBuild.exe"
@if not exist %msbuild% @set msbuild="%ProgramFiles%\MSBuild\14.0\Bin\MSBuild.exe"
@if not exist %msbuild% @set msbuild="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
@if not exist %msbuild% @set msbuild="%ProgramFiles%\MSBuild\12.0\Bin\MSBuild.exe"

%msbuild% /m /nr:false /p:Configuration=Debug  /p:Platform="Any CPU" /v:M MaterialSkin.sln
%msbuild% /m /nr:false /p:Configuration=Release /p:Platform="Any CPU" /v:M MaterialSkin.sln

@PAUSE