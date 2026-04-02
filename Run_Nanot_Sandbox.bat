@echo off
echo Iniciando NANOT Sandbox (Version Nativa C#)...
cd GodotEngine
dotnet build
if %ERRORLEVEL% neq 0 (
    echo Error de compilacion. Verifica el log de MSBuild.
    pause
    exit /b
)
C:\tools\godot\Godot_v4.3-stable_mono_win64\Godot_v4.3-stable_mono_win64.exe --path .
